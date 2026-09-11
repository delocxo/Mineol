interface INative
{
    public string[] Kinds { get; }
    public void Register(Dictionary<string, Value> globals, NativeMembers nativeMembers);
}