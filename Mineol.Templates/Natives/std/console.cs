class ConsoleNative : INative
{
    public string[] Kinds => [];

    public void Register(Dictionary<string, Value> globals, NativeMembers nativeMembers)
    {
        globals.AddFunction("console_writeline", ["value"], (args, _) =>
        {
            Console.WriteLine(args[0]);
            return Value.Null;
        });

        globals.AddFunction("console_write", ["value"], (args, _) =>
        {
            Console.Write(args[0]);
            return Value.Null;
        });

        globals.AddFunction("console_input", [], (args, _) =>
        {
            return new Value(Console.ReadLine() ?? "");
        });

        globals.AddFunction("console_prompt", ["prompt"], (args, _) =>
        {
            Console.Write(args[0]);
            return new Value(Console.ReadLine() ?? "");
        });

        globals.AddFunction("console_error_writeline", ["value"], (args, _) =>
        {
            Console.Error.WriteLine(args[0]);
            return Value.Null;
        });

        globals.AddFunction("console_error_write", ["value"], (args, _) =>
        {
            Console.Error.Write(args[0]);
            return Value.Null;
        });

        globals.AddFunction("console_readkey", [], (args, _) =>
        {
            var key = Console.ReadKey(intercept: true);

            if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
                return new Value(key.KeyChar.ToString());

            return new Value(key.Key.ToString());
        });

        globals.AddFunction("console_clear", [], (args, _) =>
        {
            Console.Clear();
            return Value.Null;
        });
    }
}