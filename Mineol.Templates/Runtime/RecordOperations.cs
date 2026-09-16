static class RecordOperations
{
    public static void Register(KindOperations kindOperations)
    {
        kindOperations.AddBinary(ValueKind.Record, Binary);
        kindOperations.AddEquality(ValueKind.Record, Equality);
    }

    static bool Binary(Value left, Value right, BinaryOperation binaryOperation, Position position, out Value result)
    {
        string memberName = binaryOperation switch
        {
            BinaryOperation.Add => "add",
            BinaryOperation.Sub => "sub",
            BinaryOperation.Mul => "mul",
            BinaryOperation.Div => "div",
            BinaryOperation.Mod => "mod",
            BinaryOperation.Less => "less",
            BinaryOperation.LessEqual => "less_equal",
            BinaryOperation.Greater => "greater",
            BinaryOperation.GreaterEqual => "greater_equal",
            _ => ""
        };

        if (memberName == "" || !RuntimeFunctions.TryGetRecordMember(left, memberName, out Value method))
        {
            result = default;
            return false;
        }

        if (!method.IsFunction())
            throw new Error($"Record member '{memberName}' must be a function", position);

        result = RuntimeFunctions.Call(method, [right], position);
        return true;
    }

    static bool Equality(Value left, Value right)
    {
        bool fallback = left.RecordObject == right.RecordObject;

        if (!RuntimeFunctions.TryGetRecordMember(left, "equals", out Value method))
            return fallback;

        Value result = RuntimeFunctions.Call(method, [right], Globals.ProtocalPosition);

        return result.IsTruthy();
    }
}