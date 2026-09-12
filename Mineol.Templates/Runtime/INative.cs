interface INative
{
    public string[] Kinds { get; }
    public void Register(Dictionary<string, Value> globals);
    public void RegiserKindOperations(KindOperations kindOperations);
}