class KindInfo
{
    public KindInfo(int id, string name, bool @protected)
    {
        Id = id;
        Name = name;
        IsProtected = @protected;
    }

    public int Id { get; }
    public string Name { get; }
    public bool IsProtected { get; }
}

static class ValueKind
{
    static List<KindInfo> s_types = new List<KindInfo>();
    public static Dictionary<string, int> NameToId = new Dictionary<string, int>();

    // Core
    public static int Int = Register("int");
    public static int Float = Register("float");
    public static int String = Register("string");
    public static int Bool = Register("bool");
    public static int Null = Register("null");
    public static int Function = Register("function");
    public static int List = Register("list");
    public static int Record = Register("record");
    public static int Enum = Register("enum");
    public static int EnumValue = Register("enum_value");

    public static int Register(string name, bool @protected = true)
    {
        if (NameToId.TryGetValue(name, out int existing))
            return existing;

        int id = s_types.Count;

        s_types.Add(new KindInfo(id, name, @protected));
        NameToId[name] = id;

        return id;
    }

    public static KindInfo Get(int id) => s_types[id];
    public static int GetId(string name) => NameToId[name];
    public static string GetName(int id) => Get(id).Name;
}