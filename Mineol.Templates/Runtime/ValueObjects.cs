class FunctionObject
{
    public FunctionObject(string name, List<string> parameters, FunctionDelegate @delegate)
    {
        Name = name;
        Parameters = parameters;
        Delegate = @delegate;
    }

    public string Name { get; }
    public List<string> Parameters { get; }
    public FunctionDelegate Delegate { get; }

    public int Arity => Parameters.Count;
}

class RecordField
{
    public RecordField(Value value, bool constant)
    {
        Value = value;
        isConst = constant;
    }

    public Value Value { get; set; }
    public bool isConst { get; }
}

class RecordObject
{
    public RecordObject(OrderedDictionary<string, RecordField> fields)
    {
        Fields = fields;
    }

    public OrderedDictionary<string, RecordField> Fields { get; }
}