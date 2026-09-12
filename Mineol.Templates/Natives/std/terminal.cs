class TerminalNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
    }

    public void Register(Dictionary<string, Value> globals)
    {
        globals.AddFunction("term_print", ["value"], (args, pos) =>
        {
            Console.Write(args[0]);
            return Value.Null;
        });

        globals.AddFunction("term_println", ["value"], (args, pos) =>
        {
            Console.WriteLine(args[0]);
            return Value.Null;
        });

        globals.AddFunction("term_err_print", ["value"], (args, pos) =>
        {
            Console.Error.Write(args[0]);
            return Value.Null;
        });

        globals.AddFunction("term_err_println", ["value"], (args, pos) =>
        {
            Console.Error.WriteLine(args[0]);
            return Value.Null;
        });

        globals.AddFunction("term_readln", [], (args, pos) =>
        {
            return new Value(Console.ReadLine() ?? "");
        });

        globals.AddFunction("term_prompt", ["prompt"], (args, pos) =>
        {
            Console.Write(args[0]);
            return new Value(Console.ReadLine() ?? "");
        });

        globals.AddFunction("term_clear", [], (args, pos) =>
        {
            Console.Clear();
            return Value.Null;
        });
    }
}