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
    public EnumObject EnumObject => (EnumObject)Object!;
    public EnumValue EnumValue => (EnumValue)Object!;

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

    public Value(EnumObject enumObject)
    {
        Kind = ValueKind.Enum;
        Object = enumObject;
    }

    public Value(EnumValue enumValue)
    {
        Kind = ValueKind.EnumValue;
        Object = enumValue;
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
    public bool IsInt() => KindIs(ValueKind.Int);
    public bool IsFloat() => KindIs(ValueKind.Float);
    public bool IsBool() => KindIs(ValueKind.Bool);
    public bool IsString() => KindIs(ValueKind.String);
    public bool IsNull() => KindIs(ValueKind.Null);
    public bool IsFunction() => KindIs(ValueKind.Function);
    public bool IsList() => KindIs(ValueKind.List);
    public bool IsRecord() => KindIs(ValueKind.Record);
    public bool IsEnum() => KindIs(ValueKind.Enum);
    public bool IsEnumValue() => KindIs(ValueKind.EnumValue);
    public string KindName => ValueKind.GetName(Kind);

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

    public long ExpectIntInRangeEx(string message, long min, long max, Position position)
    {
        if (!IsInt())
            throw new Error("Expected an int", position);

        if (Int < min || Int >= max)
            throw new Error(message, position);

        return Int;
    }

    public long ExpectIntInRangeEx(long min, long max, Position position)
    {
        return ExpectIntInRangeEx("Int out of range", min, max, position);
    }

    public long ExpectIntInRangeIn(string message, long min, long max, Position position)
    {
        if (!IsInt())
            throw new Error("Expected an int", position);

        if (Int < min || Int > max)
            throw new Error(message, position);

        return Int;
    }

    public long ExpectIntInRangeIn(long min, long max, Position position)
    {
        return ExpectIntInRangeIn("Int out of range", min, max, position);
    }

    public List<Value> ExpectList(Position position)
    {
        if (!IsList())
            throw new Error("Expected a list", position);

        return List;
    }

    public FunctionObject ExpectFunction(string message, Position position)
    {
        if (!IsFunction())
            throw new Error(message, position);

        return FunctionObject;
    }

    public void ExpectCallback(Position position)
    {
        ExpectFunction("Expected a callback function", position);
    }

    public void ExpectPredicate(Position position)
    {
        ExpectFunction("Expected a predicate function", position);
    }

    public T As<T>() => (T)Object!;

    public string ToString(Position position)
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
            return $"[{string.Join(", ", List.Select(x => x.ToStringWithQuotes(position)))}]";

        else if (IsRecord())
            return Globals.KindOperations.ToString(this, position);

        else if (IsEnum())
            return $"<enum {EnumObject.Name}>";

        else if (IsEnumValue())
            return $"{EnumValue.EnumName}.{EnumValue.MemberName}";

        return Globals.KindOperations.ToString(this, position);
    }

    public string ToStringWithQuotes(Position position)
    {
        if (IsString())
            return $"'{String}'";

        return ToString(position);
    }

    public bool CheckEquality(Value other, Position position)
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

        else if (IsRecord() && other.IsRecord())
            return Globals.KindOperations.Equals(this, other, position);

        else if (IsEnum() && other.IsEnum())
            return EnumObject == other.EnumObject;

        else if (IsEnumValue() && other.IsEnumValue())
            return EnumValue.EnumName == other.EnumValue.EnumName
                && EnumValue.MemberName == other.EnumValue.MemberName;

        return Globals.KindOperations.Equals(this, other, position);
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

    public int GetHash(Position position)
    {
        if (TryGetHash(out int hash))
            return hash;

        throw new Error($"{KindName} is not hashable", position);
    }

    public override int GetHashCode()
    {
        if (TryGetHash(out int hash))
            return hash;

        throw new InvalidOperationException($"{KindName} is not hashable");
    }

    public bool TryGetHash(out int hash)
    {
        if (IsNumber())
            hash = AsFloat().GetHashCode();

        else if (IsString())
            hash = String.GetHashCode();

        else if (IsBool())
            hash = Bool.GetHashCode();

        else if (IsNull())
            hash = 0;

        else if (IsFunction())
            hash = FunctionObject.GetHashCode();

        else if (IsList())
            hash = List.GetHashCode();

        else if (IsRecord())
            hash = RecordObject.GetHashCode();

        else if (IsEnumValue())
            hash = EnumValue.GetHashCode();
        else
            return Globals.KindOperations.TryGetHash(this, out hash);

        return true;
    }

    public List<Value> GetIterable(Position position)
    {
        if (IsString())
            return String
                .Select(x => new Value(x.ToString()))
                .ToList();

        else if (IsList())
            return List;

        return Globals.KindOperations.GetIterable(this, position);
    }

    public bool IsIterable() => IsList()
        || IsString()
        || Globals.KindOperations.HasIterable(Kind);

    public static Value FromFunction(
        string name, List<string> parameters,
        FunctionDelegate @delegate)
    {
        return new Value(new FunctionObject(name, parameters, @delegate));
    }
}
