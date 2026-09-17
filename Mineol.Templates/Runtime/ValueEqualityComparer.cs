class ValueEqualityComparer : IEqualityComparer<Value>
{
    public bool Equals(Value x, Value y)
    {
        return x.CheckEquality(y, Globals.ProtocalPosition);
    }

    public int GetHashCode(Value value)
    {
        return value.GetHashCode(Globals.ProtocalPosition);
    }
}
