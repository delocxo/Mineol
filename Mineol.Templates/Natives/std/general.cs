class GeneralNative : INative
{
    public string[] Kinds => [];

    public void Register(Dictionary<string, Value> globals, NativeMembers nativeMembers)
    {
        globals.AddFunction("typeof", ["value"], (args, pos) =>
        {
            return new Value(args[0].KindName);
        });

        globals.AddFunction("assert", ["condition", "err_msg"], (args, pos) =>
        {
            if (args[0].IsTruthy())
                return Value.Null;

            throw new Error(args[1].ToString(), pos);
        });
    }
}