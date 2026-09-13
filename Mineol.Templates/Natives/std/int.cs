class IntNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
    }

    public void Register(Dictionary<string, Value> globals)
    {
        globals["int_max"] = new Value(long.MaxValue);
        globals["int_min"] = new Value(long.MinValue);
    }
}