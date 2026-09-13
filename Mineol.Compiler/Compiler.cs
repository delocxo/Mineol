using System.Diagnostics;
using System.Globalization;
using System.Text;
using Scope = System.Collections.Generic.Dictionary<string, Local>;

class Local
{
    public Local(string mangledName, bool isConst)
    {
        MangledName = mangledName;
        IsConst = isConst;
    }

    public string MangledName { get; }
    public bool IsConst { get; }
}

class Compiler
{
    public StringBuilder StringBuilder { get; } = new StringBuilder();
    public Dictionary<string, Position> Uses = new Dictionary<string, Position>();
    public Stack<Scope> Scopes { get; } = new Stack<Scope>();
    public Dictionary<string, int> NextLocalIndexs { get; } = new Dictionary<string, int>();

    bool AtTopLevel => Scopes.Count == 1;

    readonly Dictionary<TokenType, string> _opToArithmetic = new Dictionary<TokenType, string>()
    {
        { TokenType.Add, "Arithmetic.Add" },
        { TokenType.Sub, "Arithmetic.Sub" },
        { TokenType.Mul, "Arithmetic.Mul" },
        { TokenType.Div, "Arithmetic.Div" },
        { TokenType.Mod, "Arithmetic.Mod" },
        { TokenType.Less, "Arithmetic.Less" },
        { TokenType.Greater, "Arithmetic.Greater" },
        { TokenType.LessEq, "Arithmetic.LessEqual" },
        { TokenType.GreaterEq, "Arithmetic.GreaterEqual" },
        { TokenType.IsEqual, "Arithmetic.Equals" },
        { TokenType.NotEqual, "Arithmetic.NotEquals" },
    };

    Dictionary<string, bool> _compiledFiles = new Dictionary<string, bool>();

    const int _indentSize = 4;
    int _indentLevel = 1;
    int _nextAnonymousFunctionIndex = 0;
    int _nextAnonymousRecordIndex = 0;
    int _nextEnumIndex = 0;
    int _functionDepth = 0;

    public Compiler()
    {
        Scopes.Push(new Scope());
    }

    public void CompileFile(string source, bool isEntry, Position position)
    {
        if (!File.Exists(source))
            if (isEntry)
            {
                Console.Error.WriteLine($"File '{source}' does not exist");
                Environment.Exit(1);
            }
            else
            {
                throw new Error($"File '{source}' does not exist", position);
            }

        if (_compiledFiles.TryGetValue(source, out bool finished))
        {
            if (!finished)
                throw new Error($"Circular import detected: '{source}'", position);
            return;
        }

        _compiledFiles[source] = false;

        Lexer lexer = new Lexer(File.ReadAllText(source), source);
        Parser parser = new Parser(lexer.Lex());
        List<Stmt> ast = parser.Parse();
        new Sematics().Check(ast);

        CompileStmts(ast);

        _compiledFiles[source] = true;
    }

    void CompileStmts(List<Stmt> stmts)
    {
        foreach (Stmt stmt in stmts)
            CompileStmt(stmt);
    }

    void CompileBody(List<Stmt> stmts)
    {
        BeginScope();

        foreach (Stmt stmt in stmts)
            CompileStmt(stmt);

        EndScope();
    }

    void CompileStmt(Stmt stmt)
    {
        switch (stmt)
        {
            case VarStmt varStmt:
                {
                    Local local = SetLocal(varStmt.Name, varStmt.IsConst, varStmt.Position, out bool created);

                    bool isFunctionExpr = varStmt.Expr is FunctionExpr;

                    if (isFunctionExpr)
                        EmitLine($"Value {local.MangledName} = Value.Null;");

                    string expr = CompileVarExpr(varStmt);

                    if (created)
                    {
                        if (local.IsConst)
                            EmitLine("// Constant Variable");
                        EmitLine($"{(isFunctionExpr ? "" : "Value ")}{local.MangledName} = {expr};");
                    }
                    else
                        EmitLine($"{local.MangledName} = {expr};");

                    break;
                }

            case IfStmt ifStmt:
                {
                    for (int i = 0; i < ifStmt.IfBranches.Count; i++)
                    {
                        IfBranch ifBranch = ifStmt.IfBranches[i];

                        string condition = CompileExpr(ifBranch.Condition);

                        if (i == 0)
                            EmitLine($"if ({condition}.IsTruthy())");
                        else
                            EmitLine($"else if ({condition}.IsTruthy())");

                        EmitLine("{");

                        CompileBody(ifBranch.Stmts);

                        EmitLine("}");
                    }

                    if (ifStmt.ElseBody != null)
                    {
                        EmitLine("else");

                        EmitLine("{");

                        CompileBody(ifStmt.ElseBody);

                        EmitLine("}");
                    }

                    break;
                }

            case WhileStmt whileStmt:
                {
                    string condition = CompileExpr(whileStmt.IfBranch.Condition);

                    EmitLine($"while ({condition}.IsTruthy())");

                    EmitLine("{");

                    CompileBody(whileStmt.IfBranch.Stmts);

                    EmitLine("}");

                    break;
                }

            case BreakStmt:
                EmitLine("break;");
                break;

            case ContinueStmt:
                EmitLine("continue;");
                break;

            case ReturnStmt returnStmt:
                {
                    if (returnStmt.Expr != null)
                    {
                        string expr = CompileExpr(returnStmt.Expr);
                        if (_functionDepth <= 0)
                            EmitLine($"return {expr}.GetExitCode();");
                        else
                            EmitLine($"return {expr};");
                        break;
                    }
                    EmitLine(AtTopLevel ? $"return 0;" : "return Value.Null;");
                    break;
                }

            case CallStmt callStmt:
                {
                    string expr = CompileCallExpr(callStmt.CallExpr);

                    EmitLine($"{expr};");

                    break;
                }

            case UseStmt useStmt:
                {
                    if (Uses.ContainsKey(useStmt.FilePath))
                        break;

                    Uses[useStmt.FilePath] = useStmt.Position;
                    break;
                }

            case IndexStmt indexStmt:
                {
                    IndexExpr indexExpr = indexStmt.IndexExpr;

                    string target = Indent(CompileExpr(indexExpr.Target));
                    string index = Indent(CompileExpr(indexExpr.Index));
                    string value = Indent(CompileExpr(indexStmt.Expr));

                    EmitLine($"""
                    RuntimeFunctions.IndexSet(
                    {target},
                    {index},
                    {value},
                        {PosToRuntimePos(indexExpr.Position)});
                    """);

                    break;
                }

            case MemberStmt memberStmt:
                {
                    MemberExpr memberExpr = memberStmt.MemberExpr;

                    string target = Indent(CompileExpr(memberExpr.Target));
                    string name = Indent($"\"{memberExpr.MemberName}\"");
                    string value = Indent(CompileExpr(memberStmt.Expr));

                    EmitLine($"""
                    RuntimeFunctions.MemberSet(
                    {target},
                    {name},
                    {value},
                        {PosToRuntimePos(memberExpr.Position)});
                    """);

                    break;
                }

            case ImportStmt importStmt:
                {
                    CompileFile(importStmt.FilePath, false, importStmt.Position);
                    break;
                }
        }
    }

    string CompileExpr(Expr expr)
    {
        switch (expr)
        {
            case IntExpr intExpr:
                {
                    return NewValue($"{intExpr.Value.ToString(CultureInfo.InvariantCulture)}L");
                }

            case FloatExpr floatExpr:
                {
                    return NewValue($"{floatExpr.Value.ToString(CultureInfo.InvariantCulture)}D");
                }

            case StringExpr stringExpr:
                {
                    return NewValue($"\"{ToCSharpString(stringExpr.Value)}\"");
                }

            case BoolExpr boolExpr:
                {
                    return boolExpr.Value ? "Value.True" : "Value.False";
                }

            case NullExpr:
                {
                    return "Value.Null";
                }

            case NameExpr nameExpr:
                {
                    if (TryResolveLocal(nameExpr.Name, out Local? local))
                        return local!.MangledName;

                    string name = $"\"{ToCSharpString(nameExpr.Name)}\"";

                    return $"""
                    Globals.GetGlobal(
                    {Indent(name)},
                        {PosToRuntimePos(nameExpr.Position)})
                    """;
                }

            case UnaryExpr unaryExpr:
                {
                    string rightExpr = CompileExpr(unaryExpr.Right);
                    string right = Indent(rightExpr);

                    if (unaryExpr.Op == TokenType.Sub)
                        return $"""
                        Arithmetic.Negate(
                        {right}, 
                            {PosToRuntimePos(unaryExpr.Position)})
                        """;
                    else
                        return $"""
                        Arithmetic.Flip(
                        {right})
                        """;
                }

            case BinaryExpr binaryExpr:
                {
                    if (binaryExpr.Op == TokenType.And)
                    {
                        string aLeft = Indent(CompileExpr(binaryExpr.Left));
                        var aRight = CompileCapturedExpr(binaryExpr.Right);

                        if (string.IsNullOrWhiteSpace(aRight.Emitted))
                        {
                            string expr1 = IndentContinuation(aRight.Expr);

                            return $"""
                            Arithmetic.And(
                            {aLeft},
                                () => {expr1})
                            """;
                        }

                        return $$"""
                        Arithmetic.And(
                        {{aLeft}},
                            () => 
                            {
                        {{IndentBy(aRight.Emitted.Trim(), 2)}}
                            return {{IndentContinuation(aRight.Expr)}};
                            })
                        """;
                    }
                    else if (binaryExpr.Op == TokenType.Or)
                    {
                        string aLeft = Indent(CompileExpr(binaryExpr.Left));
                        var aRight = CompileCapturedExpr(binaryExpr.Right);

                        if (string.IsNullOrWhiteSpace(aRight.Emitted))
                        {
                            string expr1 = IndentContinuation(aRight.Expr);

                            return $"""
                            Arithmetic.Or(
                            {aLeft},
                                () => {expr1})
                            """;
                        }

                        return $$"""
                        Arithmetic.Or(
                        {{aLeft}},
                            () => 
                            {
                        {{IndentBy(aRight.Emitted.Trim(), 2)}}
                            return {{IndentContinuation(aRight.Expr)}};
                            })
                        """;
                    }

                    string left = Indent(CompileExpr(binaryExpr.Left));
                    string right = Indent(CompileExpr(binaryExpr.Right));

                    if (_opToArithmetic.TryGetValue(binaryExpr.Op, out string? arithmetic))
                    {
                        return $"""
                        {arithmetic}(
                        {left},
                        {right},
                            {PosToRuntimePos(binaryExpr.Position)})
                        """;
                    }

                    throw new UnreachableException("Invalid binary operator");
                }

            case FunctionExpr functionExpr:
                return CompileFunction(functionExpr, $"\"\"");

            case CallExpr callExpr:
                return CompileCallExpr(callExpr);

            case ListExpr listExpr:
                {
                    List<string> values = listExpr.Exprs
                        .Select(expr => Indent(CompileExpr(expr)))
                        .ToList();

                    if (values.Count == 0)
                        return NewValue("[]");

                    return NewValue($"[\n{string.Join(",\n", values)}\n]");
                }

            case IndexExpr indexExpr:
                {
                    string target = Indent(CompileExpr(indexExpr.Target));
                    string index = Indent(CompileExpr(indexExpr.Index));

                    return $"""
                    RuntimeFunctions.IndexGet(
                    {target},
                    {index},
                        {PosToRuntimePos(indexExpr.Position)})
                    """;
                }

            case RecordExpr recordExpr:
                {
                    int index = _nextAnonymousRecordIndex++;
                    string recordLocal = $"_anonymous_record_{index}";

                    List<(VarStmt Var, string Value)> fields = new List<(VarStmt Var, string Value)>();

                    for (int i = 0; i >= 0; i++)
                    {
                        VarStmt varStmt = recordExpr.VarStmts[i];
                        string value = CompileVarExpr(varStmt);
                        fields.Add((varStmt, value));
                    }

                    EmitLine($"RecordObject {recordLocal} = new RecordObject(new OrderedDictionary<string, RecordField>");
                    EmitLine("{");

                    _indentLevel++;

                    for (int i = 0; i < recordExpr.VarStmts.Count; i++)
                    {
                        var field = fields[i];
                        VarStmt varStmt = field.Var;

                        string name = $"\"{varStmt.Name}\"";
                        string isConst = varStmt.IsConst ? "true" : "false";

                        string comma = i < fields.Count - 1 ? "," : "";

                        if (varStmt.IsConst)
                            EmitLine("// Constant Field");

                        EmitLine($"[{name}] = new RecordField({field.Value}, {isConst}){comma}");
                    }

                    _indentLevel--;

                    EmitLine("});");

                    return NewValue(recordLocal);
                }

            case MemberExpr memberExpr:
                {
                    string target = Indent(CompileExpr(memberExpr.Target));
                    string name = Indent($"\"{memberExpr.MemberName}\"");

                    return $"""
                    RuntimeFunctions.MemberGet(
                    {target},
                    {name},
                        {PosToRuntimePos(memberExpr.Position)})
                    """;
                }
        }

        throw new Error("Invalid expression", expr.Position);
    }

    string CompileCallExpr(CallExpr callExpr)
    {
        string target = Indent(CompileExpr(callExpr.target));

        List<string> arguments = callExpr.Arguments
            .Select(CompileExpr)
            .ToList();

        string indentedArguments = Indent($"[{string.Join(", ", arguments)}]");

        string pos = PosToRuntimePos(callExpr.Position);

        return
        $"""
        RuntimeFunctions.Call(
        {target},
        {indentedArguments},
            {pos})
        """;
    }

    string CompileFunction(FunctionExpr functionExpr, string name)
    {
        int index = _nextAnonymousFunctionIndex++;
        string functionLocal = $"_anonymous_function_{index}";

        EmitLine($"FunctionDelegate {functionLocal} = (args, _) =>");

        EmitLine("{");

        BeginScope();

        for (int i = 0; i < functionExpr.Parameters.Count; i++)
        {
            string parameter = functionExpr.Parameters[i];
            Local local = DeclareLocal(parameter, false, functionExpr.Position);
            EmitLine($"Value {local.MangledName} = args[{i}];");
        }

        _functionDepth++;

        CompileStmts(functionExpr.Stmts);

        _functionDepth--;

        if (functionExpr.Stmts.Count == 0 || functionExpr.Stmts.Last() is not ReturnStmt)
            EmitLine("return Value.Null;");

        EndScope();

        EmitLine("};");

        return NewValue($"new FunctionObject({name}, [{string.Join(", ", functionExpr.Parameters.Select(x => $"\"{x}\""))}], {functionLocal})");
    }

    string CompileEnum(EnumExpr enumExpr)
    {
        int index = _nextEnumIndex++;
        string enumLocal = $"_enum_{enumExpr.Name}_{index}";
        string enumName = $"\"{enumExpr.Name}\"";

        EmitLine($"EnumObject {enumLocal} = new EnumObject({enumName}, new OrderedDictionary<string, EnumValue>");
        EmitLine("{");

        _indentLevel++;

        for (int i = 0; i < enumExpr.Enums.Count; i++)
        {
            string name = $"\"{enumExpr.Enums[i]}\"";
            string comma = i < enumExpr.Enums.Count - 1 ? "," : "";

            EmitLine($"[{name}] = new EnumValue({enumName}, {name}, {i}){comma}");
        }

        _indentLevel--;

        EmitLine("});");

        return NewValue(enumLocal);

    }

    void BeginScope()
    {
        _indentLevel++;
        Scopes.Push(new Scope());
    }

    void EndScope()
    {
        Scopes.Pop();
        _indentLevel--;
    }

    bool TryResolveLocal(string name, out Local? local)
    {
        foreach (Scope scope in Scopes)
        {
            if (scope.TryGetValue(name, out local))
                return true;
        }
        local = null;
        return false;
    }

    Local SetLocal(string name, bool isConst, Position position, out bool created)
    {
        if (TryResolveLocal(name, out Local? local))
        {
            if (isConst)
                throw new Error($"'{name}' already exist", position);

            if (local!.IsConst)
                throw new Error($"Const '{name}' cannot be reassigned", position);

            created = false;
            return local;
        }

        created = true;
        return DeclareLocal(name, isConst, position);
    }

    Local DeclareLocal(string name, bool isConst, Position position)
    {
        Scope scope = Scopes.Peek();

        if (scope.ContainsKey(name))
            throw new Error($"'{name}' already exist", position);

        if (!NextLocalIndexs.ContainsKey(name))
            NextLocalIndexs[name] = 0;

        int index = NextLocalIndexs[name]++;
        string mangledName = $"_{name}_{index}";

        Local local = new Local(mangledName, isConst);

        scope.Add(name, local);

        return local;
    }

    void EmitLine(string text)
    {
        string padding = new string(' ', _indentLevel * _indentSize);

        string[] lines = text
            .ReplaceLineEndings("\n")
            .Split("\n");

        foreach (string line in lines)
        {
            StringBuilder.Append(padding);
            StringBuilder.AppendLine(line);
        }
    }

    string ToCSharpString(string str) => str
        .Replace("\\", "\\\\")
        .Replace("\"", "\\\"")
        .Replace("\t", "\\t")
        .Replace("\n", "\\n")
        .Replace("\r", "\\r")
        .Replace("\f", "\\f")
        .Replace("\a", "\\a")
        .Replace("\b", "\\b")
        .Replace("\e", "\\e")
        .Replace("\v", "\\v")
        .Replace("\0", "\\0");

    string NewValue(string content)
    {
        return $"new Value({content})";
    }

    string Indent(string text)
    {
        string padding = new string(' ', _indentSize);
        return string.Join('\n', text.Replace("\r\n", "\n").Split('\n').Select(line => padding + line));
    }

    string IndentBy(string text, int levels)
    {
        string padding = new string(' ', _indentSize * levels);
        return string.Join('\n', text.Replace("\r\n", "\n").Split('\n').Select(line => padding + line));
    }

    string IndentContinuation(string text)
    {
        string padding = new string(' ', _indentSize);

        string[] lines = text.Replace("\r\n", "\n").Split('\n');

        if (lines.Length == 1)
            return text;

        for (int i = 1; i < lines.Length; i++)
            lines[i] = padding + lines[i];

        return string.Join("\n", lines);
    }

    string PosToRuntimePos(Position position)
    {
        return $"new Position({position.Line}, {position.Column}, \"{position.Source}\")";
    }

    string CompileVarExpr(VarStmt varStmt)
    {
        if (varStmt.Expr is FunctionExpr functionExpr)
            return CompileFunction(functionExpr, $"\"{varStmt.Name}\"");
        else if (varStmt.Expr is EnumExpr enumExpr)
            return CompileEnum(enumExpr);
        else
            return CompileExpr(varStmt.Expr);
    }

    (string Expr, string Emitted) CompileCapturedExpr(Expr expr)
    {
        int start = StringBuilder.Length;

        string result = CompileExpr(expr);

        string emitted = StringBuilder
            .ToString(start, StringBuilder.Length - start);

        StringBuilder.Length = start;

        return (result, emitted);
    }
}