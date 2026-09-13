class ValueEqualityComparer : IEqualityComparer<Value>
{
    public bool Equals(Value x, Value y)
    {
        return x.CheckEquality(y);
    }

    public int GetHashCode(Value value)
    {
        return value.GetHashCode();
    }
}