class Depth
{
    public int LoopDepth { get; set; }
    public int IfDepth { get; set; }

    public Depth CopyClear()
    {
        int previousLoop = LoopDepth;
        int previousIf = IfDepth;
        LoopDepth = 0;
        IfDepth = 0;
        return new Depth
        {
            LoopDepth = previousLoop,
            IfDepth = previousIf
        };
    }

    public void CopyFromOtherDepth(Depth other)
    {
        LoopDepth = other.LoopDepth;
        IfDepth = other.IfDepth;
    }
}

class Sematics
{
    Depth _depth = new Depth();

    public void Check(List<Stmt> stmts)
    {
        foreach (Stmt stmt in stmts)
        {
            if (stmt is IfStmt ifStmt)
                CheckIfStmt(ifStmt);

            else if (stmt is VarStmt varStmt)
                CheckExpr(varStmt.Expr);

            else if (stmt is WhileStmt whileStmt)
                CheckWhileStmt(whileStmt);

            else if (stmt is BreakStmt breakStmt && _depth.LoopDepth <= 0)
                throw new Error("Cannot use break outside a loop", breakStmt.Position);

            else if (stmt is ContinueStmt continueStmt && _depth.LoopDepth <= 0)
                throw new Error("Cannot use continue outside a loop", continueStmt.Position);

            else if (stmt is ReturnStmt returnStmt)
            {
                if (returnStmt.Expr != null)
                    CheckExpr(returnStmt.Expr);
            }

            else if (stmt is CallStmt callStmt)
                CheckExpr(callStmt.CallExpr);

            else if (stmt is IndexStmt indexStmt)
            {
                CheckExpr(indexStmt.IndexExpr);
                CheckExpr(indexStmt.Expr);
            }

            else if (stmt is MemberStmt memberStmt)
            {
                CheckExpr(memberStmt.MemberExpr);
                CheckExpr(memberStmt.Expr);
            }

            else if (stmt is ForStmt forStmt)
            {
                CheckExpr(forStmt.Iterable);
                Check(forStmt.Body);
            }
        }
    }

    void CheckIfStmt(IfStmt ifStmt)
    {
        foreach (IfBranch ifBranch in ifStmt.IfBranches)
        {
            _depth.IfDepth++;

            CheckExpr(ifBranch.Condition);

            Check(ifBranch.Stmts);

            _depth.IfDepth--;
        }

        if (ifStmt.ElseBody != null)
        {
            _depth.IfDepth++;

            Check(ifStmt.ElseBody);

            _depth.IfDepth--;
        }
    }

    void CheckWhileStmt(WhileStmt whileStmt)
    {
        _depth.LoopDepth++;

        CheckExpr(whileStmt.IfBranch.Condition);

        Check(whileStmt.IfBranch.Stmts);

        _depth.LoopDepth--;
    }

    void CheckExpr(Expr expr)
    {
        if (expr is FunctionExpr functionExpr)
        {
            functionExpr.Parameters.ThrowIfDuplicates((x) => $"'{x}' is a duplicate function parameter", functionExpr.Position);

            Depth depth = _depth.CopyClear();

            Check(functionExpr.Stmts);

            _depth.CopyFromOtherDepth(depth);
        }

        else if (expr is UnaryExpr unaryExpr)
            CheckExpr(unaryExpr.Right);

        else if (expr is BinaryExpr binaryExpr)
        {
            CheckExpr(binaryExpr.Left);
            CheckExpr(binaryExpr.Right);
        }

        else if (expr is CallExpr callExpr)
        {
            CheckExpr(callExpr.target);
            foreach (Expr arg in callExpr.Arguments)
                CheckExpr(arg);
        }

        else if (expr is ListExpr listExpr)
            foreach (Expr item in listExpr.Exprs)
                CheckExpr(item);

        else if (expr is IndexExpr indexExpr)
        {
            CheckExpr(indexExpr.Target);
            CheckExpr(indexExpr.Index);
        }

        else if (expr is RecordExpr recordExpr)
        {
            HashSet<string> fields = new HashSet<string>();

            foreach (VarStmt varStmt in recordExpr.VarStmts)
            {
                if (!fields.Add(varStmt.Name))
                    throw new Error($"'{varStmt.Name}' is a duplicate record field", varStmt.Position);

                CheckExpr(varStmt.Expr);
            }
        }

        else if (expr is MemberExpr memberExpr)
        {
            CheckExpr(memberExpr.Target);
        }

        else if (expr is EnumExpr enumExpr)
            enumExpr.Enums.ThrowIfDuplicates((x) => $"{x} is a duplicate enum value of '{enumExpr.Name}'", enumExpr.Position);
    }
}