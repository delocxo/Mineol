class GeneralNative : INative
{
    public string[] Kinds => [];

    public void Register(Dictionary<string, Value> globals, NativeMembers nativeMembers)
    {
        globals.AddFunction("kindof", ["value"], (args, pos) =>
        {
            return new Value(args[0].KindName);
        });

        globals.AddFunction("assert", ["condition", "err_msg"], (args, pos) =>
        {
            if (args[0].IsTruthy())
                return Value.Null;

            throw new Error(args[1].ToString(), pos);
        });

        globals.AddFunction("iskind", ["value", "kind"], (args, pos) =>
        {
            string kindName = args[1].ExpectString("Expected a string", pos);

            if (!ValueKind.NameToId.TryGetValue(kindName, out int kind))
                throw new Error($"'{kindName}' is an invalid kind", pos);

            return new Value(args[0].Kind == kind);
        });

        globals.AddFunction("len", ["value"], (args, pos) =>
        {
            Value target = args[0];

            if (target.IsList())
                return new Value(target.List.Count);

            else if (target.IsString())
                return new Value(target.String.Length);

            throw new Error($"Cannot get the length of {target.KindName}", pos);
        });
    }
}