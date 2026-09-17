static class RecordOperations
{
    public static void Register(KindOperations kindOperations)
    {
        kindOperations.AddBinary(ValueKind.Record, Binary);
        kindOperations.AddEquality(ValueKind.Record, Equality);
        kindOperations.AddToString(ValueKind.Record, ToString);
        kindOperations.AddIndexGetter(ValueKind.Record, IndexGet);
        kindOperations.AddIndexSetter(ValueKind.Record, IndexSet);
        kindOperations.AddHash(ValueKind.Record, Hash);
        kindOperations.AddUnary(ValueKind.Record, Unary);
        kindOperations.AddMemberGetter(ValueKind.Record, MemberGet);
        // kindOperations.AddMemberSetter(ValueKind.Record, MemberSet);
        kindOperations.AddCopy(ValueKind.Record, Copy);
    }

    static bool Binary(Value left, Value right, BinaryOperation binaryOperation, Position position, out Value result)
    {
        string memberName = binaryOperation switch
        {
            BinaryOperation.Add => "_add_",
            BinaryOperation.Sub => "_sub_",
            BinaryOperation.Mul => "_mul_",
            BinaryOperation.Div => "_div_",
            BinaryOperation.Mod => "_mod_",
            BinaryOperation.Less => "_less_",
            BinaryOperation.LessEqual => "_less_equal_",
            BinaryOperation.Greater => "_greater_",
            BinaryOperation.GreaterEqual => "_greater_equal_",
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

    static bool Equality(Value left, Value right, Position position)
    {
        bool fallback = left.RecordObject == right.RecordObject;

        if (!RuntimeFunctions.TryGetRecordMember(left, "_equals_", out Value method))
            return fallback;

        Value result = RuntimeFunctions.Call(method, [right], position);

        return result.IsTruthy();
    }

    static string ToString(Value target, Position position)
    {
        if (!RuntimeFunctions.TryGetRecordMember(target, "_to_string_", out Value method))
        {
            string[] contents = target.RecordObject.Fields
                .Select(x =>
                    $"{x.Key}{(x.Value.isConst ? "!" : "")} = {x.Value.Value.ToStringWithQuotes(position)}")
                .ToArray();

            return $"{{ {string.Join(", ", contents)} }}";
        }

        Value result = RuntimeFunctions.Call(method, [], position);

        return result.ToString(position);
    }

    static Value IndexGet(Value target, Value index, Position position)
    {
        if (!RuntimeFunctions.TryGetRecordMember(target, "_index_get_", out Value method))
            throw new Error("Record could not be index read", position);

        Value result = RuntimeFunctions.Call(method, [index], position);

        return result;
    }

    static void IndexSet(Value target, Value index, Value value, Position position)
    {
        if (!RuntimeFunctions.TryGetRecordMember(target, "_index_set_", out Value method))
            throw new Error("Record could not be index set", position);

        RuntimeFunctions.Call(method, [index, value], position);
    }

    static int Hash(Value target, Position position)
    {
        if (!RuntimeFunctions.TryGetRecordMember(target, "_hash_", out Value method))
            throw new Error("Record could not be hashed", position);

        Value result = RuntimeFunctions.Call(method, [], position);

        return (int)result.ExpectIntInRangeIn("Expected a 32 bit hash", int.MinValue, int.MaxValue, position);
    }

    static bool Unary(Value right, UnaryOperation unaryOperation, Position position, out Value result)
    {
        string memberName = unaryOperation switch
        {
            UnaryOperation.Flip => "_flip_",
            UnaryOperation.Negate => "_negate_",
            _ => ""
        };

        if (memberName == "" || !RuntimeFunctions.TryGetRecordMember(right, memberName, out Value method))
        {
            result = default;
            return false;
        }

        if (!method.IsFunction())
            throw new Error($"Record member '{memberName}' must be a function", position);

        result = RuntimeFunctions.Call(method, [], position);
        return true;
    }

    static Value MemberGet(Value target, string memberName, Position position)
    {
        if (!RuntimeFunctions.TryGetRecordMember(target, "_member_get_", out Value method))
            throw new Error("Record could not be member get", position);

        Value result = RuntimeFunctions.Call(method, [new Value(memberName)], position);

        return result;
    }

    // static void MemberSet(Value target, string memberName, Value value, Position position)
    // {
    //     if (!RuntimeFunctions.TryGetRecordMember(target, "_member_set_", out Value method))
    //         throw new Error("Record could not be member set", position);

    //     Value result = RuntimeFunctions.Call(method, [new Value(memberName), value], position);
    // }

    static Value Copy(Value target, Position position)
    {
        if (!RuntimeFunctions.TryGetRecordMember(target, "_copy_", out Value method))
            throw new Error("Record could not be copied", position);

        Value result = RuntimeFunctions.Call(method, [], position);

        return result;
    }
}
