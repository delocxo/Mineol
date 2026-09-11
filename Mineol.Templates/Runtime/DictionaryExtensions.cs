static class DictionaryExtensions
{
    public static void AddFunction(
        this Dictionary<string, Value> dict,
        string name, List<string> parameters,
        FunctionDelegate @delegate
        )
    {
        dict[name] = new Value(new FunctionObject(name, parameters, @delegate));
    }
}