class SysNative : INative
{
    public string[] Kinds => [];

    public void Register(Dictionary<string, Value> globals, NativeMembers nativeMembers)
    {
        globals.AddFunction("get_sys_args", [], (args, pos) =>
        {
            return new Value(Environment
                .GetCommandLineArgs()
                .Skip(1)
                .Select(x => new Value(x))
                .ToList());
        });
    }
}