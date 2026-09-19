struct Position
{
    public Position(int line, int column, string source)
    {
        Line = line;
        Column = column;
        Source = source;
    }

    public int Line { get; set; }
    public int Column { get; set; }
    public string Source { get; }
}

static class ErrorType
{
    static Dictionary<Type, EnumValue> s_types = new Dictionary<Type, EnumValue>();

    static readonly EnumObject s_enum = new EnumObject("ErrorType", new OrderedDictionary<string, EnumValue>());

    public static Value Enum = new Value(s_enum);

    public static Value Get<T>() where T : Exception
    {
        return new Value(s_types[typeof(T)]);
    }

    public static Value Get(Type type)
    {
        return new Value(s_types[type]);
    }

    public static void Register(Type type)
    {
        if (s_types.ContainsKey(type))
            return;

        string name = type.Name;

        if (name.EndsWith("Exception"))
            name = name[.."Exception".Length];

        EnumValue value = new EnumValue("ErrorType", name, s_types.Count);

        s_types[type] = value;
        s_enum.Members[name] = value;
    }
}

class Error : Exception
{
    public Value ErrorValue { get; } = Value.Null;

    Position _position;

    public Error(string message, Position position) : base(message)
    {
        _position = position;
    }

    public Error(string message, Value errorValue, Position position) : base(message)
    {
        ErrorValue = errorValue;
        _position = position;
    }

    public void Exit()
    {
        Console.Error.WriteLine(Message);
        Environment.Exit(1);
    }
}
