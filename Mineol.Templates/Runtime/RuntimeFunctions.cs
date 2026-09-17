static class RuntimeFunctions
{
    public static Value Call(Value value, List<Value> parameters, Position position)
    {
        if (value.IsFunction())
        {
            FunctionObject functionObject = value.FunctionObject;

            if (functionObject.Arity != parameters.Count)
                throw new Error($"{(functionObject.Name != "" ? $"{functionObject.Name} " : "Anonymous ")}function expects {functionObject.Arity} argument(s), got {parameters.Count}", position);

            return functionObject.Delegate(parameters, position);
        }

        throw new Error($"{value.KindName} is not callable", position);
    }

    public static Value IndexGet(Value target, Value index, Position position)
    {
        if (target.IsList())
        {
            long rawIndex = index.ExpectInt("Expected an int indexer", position);

            List<Value> list = target.List;

            if (rawIndex < 0 || rawIndex >= list.Count)
                throw new Error("Index out of list range", position);

            return list[(int)rawIndex];
        }
        else if (target.IsString())
        {
            long rawIndex = index.ExpectInt("Expected an int indexer", position);

            string str = target.String;

            if (rawIndex < 0 || rawIndex >= str.Length)
                throw new Error("Index out of string's range", position);

            return new Value(str[(int)rawIndex].ToString());
        }

        return Globals.KindOperations.GetIndex(target, index, position);
    }

    public static void IndexSet(Value target, Value index, Value value, Position position)
    {
        if (target.IsList())
        {
            long rawIndex = index.ExpectInt("Expected an int indexer", position);

            List<Value> list = target.List;

            if (rawIndex < 0 || rawIndex >= list.Count)
                throw new Error("Index out of list range", position);

            list[(int)rawIndex] = value;

            return;
        }

        Globals.KindOperations.SetIndex(target, index, value, position);
    }

    public static Value MemberGet(Value target, string memberName, Position position)
    {
        if (target.IsRecord())
            if (TryGetRecordMember(target, memberName, out Value value))
                return value;

        if (target.IsEnum())
        {
            EnumObject enumObject = target.EnumObject;

            if (enumObject.Members.TryGetValue(memberName, out EnumValue? enumValue))
                return new Value(enumValue);
        }

        return Globals.KindOperations.GetMember(target, memberName, position);
    }

    public static void MemberSet(Value target, string memberName, Value value, Position position)
    {
        if (target.IsRecord())
        {
            RecordObject recordObject = target.RecordObject;

            if (!recordObject.Fields.TryGetValue(memberName, out RecordField? field))
                throw new Error($"Anonymous record does not contain field '{memberName}'", position);

            if (field.isConst)
                throw new Error($"Record field '{memberName}' is constant and cant be changed", position);

            if (TryGetRecordMember(target, "_member_set_", out Value setter))
                Call(setter, [new Value(memberName), value], position);

            field.Value = value;
            return;
        }

        Globals.KindOperations.SetMember(target, memberName, value, position);
    }

    public static bool TryGetRecordMember(Value target, string memberName, out Value value)
    {
        RecordObject recordObject = target.RecordObject;

        if (recordObject.Fields.TryGetValue(memberName, out RecordField? field))
        {
            Value fieldValue = field.Value;

            if (fieldValue.IsFunction())
            {
                FunctionObject function = fieldValue.FunctionObject;

                if (function.Arity > 0 && function.Parameters[0] == "self")
                {
                    List<string> parameters = function.Parameters
                        .Skip(1)
                        .ToList();

                    value = new Value(
                        new FunctionObject(
                            memberName,
                            parameters,
                            (args, callPos) =>
                            {
                                List<Value> boundedArgs = [target, .. args];
                                return function.Delegate(boundedArgs, callPos);
                            }
                        )
                    );

                    return true;
                }
            }

            value = fieldValue;
            return true;
        }

        value = default;
        return false;
    }

    public static Value MakeResultRecord(Value value, bool success)
    {
        RecordObject result = new RecordObject(new OrderedDictionary<string, RecordField>
        {
            ["success"] = new RecordField(new Value(success), false),
            ["value"] = new RecordField(value, false)
        });

        return new Value(result);
    }
}