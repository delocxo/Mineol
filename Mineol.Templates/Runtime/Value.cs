using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

struct Value
{
    public int Kind { get; set; }
    public long Int { get; }
    public double Float { get; }
    public bool Bool { get; }
    public object? Object { get; }
    public string String => (string)Object!;
    public FunctionObject FunctionObject => (FunctionObject)Object!;
    public List<Value> List => (List<Value>)Object!;
    public RecordObject RecordObject => (RecordObject)Object!;

    public Value(long value)
    {
        Kind = ValueKind.Int;
        Int = value;
    }

    public Value(double value)
    {
        Kind = ValueKind.Float;
        Float = value;
    }

    public Value(bool value)
    {
        Kind = ValueKind.Bool;
        Bool = value;
    }

    public Value(string value)
    {
        Kind = ValueKind.String;
        Object = value;
    }

    public Value(FunctionObject functionObject)
    {
        Kind = ValueKind.Function;
        Object = functionObject;
    }

    public Value(List<Value> list)
    {
        Kind = ValueKind.List;
        Object = list;
    }

    public Value(RecordObject recordObject)
    {
        Kind = ValueKind.Record;
        Object = recordObject;
    }

    public Value(object obj, int kind)
    {
        Kind = kind;
        Object = obj;
    }

    public static Value Null => new Value() { Kind = ValueKind.Null };
    public static Value True => new Value(true);
    public static Value False => new Value(false);

    public bool KindIs(int kind) => Kind == kind;
    public bool IsNumber() => KindIs(ValueKind.Int) || KindIs(ValueKind.Float);
    public double AsFloat() => KindIs(ValueKind.Float) ? Float : Int;
    public string KindName => ValueKind.GetName(Kind);
    public bool IsInt() => KindIs(ValueKind.Int);
    public bool IsFloat() => KindIs(ValueKind.Float);
    public bool IsBool() => KindIs(ValueKind.Bool);
    public bool IsString() => KindIs(ValueKind.String);
    public bool IsNull() => KindIs(ValueKind.Null);
    public bool IsFunction() => KindIs(ValueKind.Function);
    public bool IsList() => KindIs(ValueKind.List);
    public bool IsRecord() => KindIs(ValueKind.Record);

    public string ExpectString(string message, Position position)
    {
        if (!IsString())
            throw new Error(message, position);

        return String;
    }

    public long ExpectInt(string message, Position position)
    {
        if (!IsInt())
            throw new Error(message, position);

        return Int;
    }

    public override string ToString()
    {
        if (KindIs(ValueKind.Int))
            return Int.ToString(CultureInfo.InvariantCulture);

        else if (KindIs(ValueKind.Float))
            return Float.ToString(CultureInfo.InvariantCulture);

        else if (KindIs(ValueKind.Bool))
            return Bool ? "true" : "false";

        else if (KindIs(ValueKind.String))
            return String;

        else if (KindIs(ValueKind.Null))
            return "null";

        else if (KindIs(ValueKind.Function))
        {
            if (FunctionObject.Name != string.Empty)
                return $"<function {FunctionObject.Name}({string.Join(", ", FunctionObject.Parameters)})>";
            else
                return $"<function({string.Join(", ", FunctionObject.Parameters)})>";
        }
        else if (IsList())
            return $"[{string.Join(", ", List.Select(x => x.ToStringWithQuotes()))}]";

        else if (IsRecord())
        {
            string[] contents = RecordObject.Fields
                .Select(x =>
                    $"{x.Key}{(x.Value.isConst ? "!" : "")} = {x.Value.Value.ToStringWithQuotes()}")
                .ToArray();

            return $"{{ {string.Join(", ", contents)} }}";
        }

        return "invalid type";
    }

    public string ToStringWithQuotes()
    {
        if (IsString())
            return $"'{String}'";

        return ToString();
    }

    public bool CheckEquality(Value other)
    {
        if (IsNumber() && other.IsNumber())
        {
            if (KindIs(ValueKind.Float) || other.KindIs(ValueKind.Float))
                return AsFloat() == other.AsFloat();

            return Int == other.Int;
        }

        else if (KindIs(ValueKind.Bool) && other.KindIs(ValueKind.Bool))
            return Bool == other.Bool;

        else if (KindIs(ValueKind.String) && other.KindIs(ValueKind.String))
            return string.Equals(String, other.String, StringComparison.Ordinal);

        else if (KindIs(ValueKind.Null) && other.KindIs(ValueKind.Null))
            return true;

        else if (KindIs(ValueKind.Function) && other.KindIs(ValueKind.Function))
            return FunctionObject == other.FunctionObject;

        else if (IsList() && other.IsList())
            return List == other.List;

        return false;
    }

    public bool IsTruthy()
    {
        if (IsNull())
            return false;

        else if (IsBool())
            return Bool;

        return true;
    }

    public int GetExitCode()
    {
        if (IsInt())
            return (int)Int;

        else if (IsFloat())
            return (int)Float;

        else if (IsBool())
            return Bool ? 0 : 1;

        return 0;
    }
}