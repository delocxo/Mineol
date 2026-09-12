class GeneralNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
    }

    public void Register(Dictionary<string, Value> globals)
    {
        globals.AddFunction("kind_of", ["value"], (args, pos) =>
        {
            return new Value(args[0].KindName);
        });

        globals.AddFunction("kind_is", ["value", "kind"], (args, pos) =>
        {
            string kindName = args[1].ExpectString("Expected a kind name", pos);

            if (!ValueKind.NameToId.TryGetValue(kindName, out int kind))
                throw new Error($"'{kindName}' is not a valid kind", pos);

            return new Value(args[0].Kind == kind);
        });
    }
}