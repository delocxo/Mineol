using System;
using System.Collections.Generic;
using System.Text;

abstract record Expr(Position Position);
record IntExpr(long Value, Position Position) : Expr(Position);
record FloatExpr(double Value, Position Position) : Expr(Position);
record StringExpr(string Value, Position Position) : Expr(Position);
record BoolExpr(bool Value, Position Position) : Expr(Position);
record NullExpr(Position Position) : Expr(Position);
record NameExpr(string Name, Position Position) : Expr(Position);
record UnaryExpr(Expr Right, TokenType Op, Position Position) : Expr(Position);
record BinaryExpr(Expr Left, Expr Right, TokenType Op, Position Position) : Expr(Position);
record FunctionExpr(List<Stmt> Stmts, List<string> Parameters, bool IsExpr, Position Position) : Expr(Position);
record CallExpr(Expr target, List<Expr> Arguments, Position Position) : Expr(Position);
record ListExpr(List<Expr> Exprs, Position Position) : Expr(Position);
record IndexExpr(Expr Target, Expr Index, Position Position) : Expr(Position);
record RecordExpr(List<VarStmt> VarStmts, Position Position) : Expr(Position);
record MemberExpr(Expr Target, string MemberName, Position Position) : Expr(Position);
record EnumExpr(string Name, List<string> Enums, Position Position) : Expr(Position);

abstract class Stmt
{
    public Stmt(Position position)
    {
        Position = position;
    }

    public Position Position { get; }
}

class VarStmt : Stmt
{
    public VarStmt(bool @const, Expr expr, string name, Position position) : base(position)
    {
        IsConst = @const;
        Expr = expr;
        Name = name;
    }

    public bool IsConst { get; }
    public Expr Expr { get; }
    public string Name { get; }
}

class IfBranch
{
    public IfBranch(Expr condition, List<Stmt> stmts)
    {
        Condition = condition;
        Stmts = stmts;
    }

    public Expr Condition { get; }
    public List<Stmt> Stmts { get; }
}

class IfStmt : Stmt
{
    public IfStmt(List<IfBranch> ifBranches, List<Stmt>? elseBody, Position position) : base(position)
    {
        IfBranches = ifBranches;
        ElseBody = elseBody;
    }

    public List<IfBranch> IfBranches { get; }
    public List<Stmt>? ElseBody { get; }
}

class WhileStmt : Stmt
{
    public WhileStmt(IfBranch ifBranch, Position position) : base(position)
    {
        IfBranch = ifBranch;
    }

    public IfBranch IfBranch { get; }
}

class BreakStmt : Stmt
{
    public BreakStmt(Position position) : base(position)
    {
    }
}

class ContinueStmt : Stmt
{
    public ContinueStmt(Position position) : base(position)
    {
    }
}
class ReturnStmt : Stmt
{
    public ReturnStmt(Expr? expr, Position position) : base(position)
    {
        Expr = expr;
    }

    public Expr? Expr { get; }
}

class CallStmt : Stmt
{
    public CallStmt(CallExpr callExpr) : base(callExpr.Position)
    {
        CallExpr = callExpr;
    }

    public CallExpr CallExpr { get; }
}

class UseStmt : Stmt
{
    public UseStmt(string filePath, Position position) : base(position)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }
}

class IndexStmt : Stmt
{
    public IndexStmt(IndexExpr indexExpr, Expr expr) : base(indexExpr.Position)
    {
        IndexExpr = indexExpr;
        Expr = expr;
    }

    public IndexExpr IndexExpr { get; }
    public Expr Expr { get; }
}

class MemberStmt : Stmt
{
    public MemberStmt(MemberExpr memberExpr, Expr expr) : base(memberExpr.Position)
    {
        MemberExpr = memberExpr;
        Expr = expr;
    }

    public MemberExpr MemberExpr { get; }
    public Expr Expr { get; }
}

class ImportStmt : Stmt
{
    public ImportStmt(string filePath, Position position) : base(position)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }
}

class ForStmt : Stmt
{
    public ForStmt(string iterableName, Expr iterable, List<Stmt> body, Position position) : base(position)
    {
        IterableName = iterableName;
        Iterable = iterable;
        Body = body;
    }

    public string IterableName { get; }
    public Expr Iterable { get; }
    public List<Stmt> Body { get; }
}