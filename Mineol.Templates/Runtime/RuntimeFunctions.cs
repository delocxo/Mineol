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
        long rawIndex = index.ExpectInt("Expected an int indexer", position);

        if (target.IsList())
        {
            List<Value> list = target.List;

            if (rawIndex < 0 || rawIndex >= list.Count)
                throw new Error("Index out of list range", position);

            return list[(int)rawIndex];
        }
        else if (target.IsString())
        {
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
            return GetRecordMember(target, memberName, position);

        if (target.IsEnum())
        {
            EnumObject enumObject = target.EnumObject;

            if (enumObject.Members.TryGetValue(memberName, out EnumValue? enumValue))
                return new Value(enumValue);

            throw new Error($"Enum '{memberName}' does not contain '{memberName}", position);
        }

        return Globals.KindOperations.GetMember(target, memberName, position);
    }

    public static void MemberSet(Value target, string memberName, Value value, Position position)
    {
        if (target.IsRecord())
        {
            RecordObject recordObject = target.RecordObject;

            if (recordObject.Fields.TryGetValue(memberName, out RecordField? field))
            {
                if (field.isConst)
                    throw new Error($"Anonymous record field '{memberName}' is constant and cant be changed", position);

                field.Value = value;
                return;
            }

            throw new Error($"Anonymous record does not contain field '{memberName}'", position);
        }

        Globals.KindOperations.SetMember(target, memberName, value, position);
    }

    static Value GetRecordMember(Value target, string memberName, Position position)
    {
        RecordObject recordObject = target.RecordObject;

        if (recordObject.Fields.TryGetValue(memberName, out RecordField? field))
        {
            Value value = field.Value;

            if (value.IsFunction())
            {
                FunctionObject function = value.FunctionObject;

                if (function.Arity > 0 && function.Parameters[0] == "self")
                {
                    List<string> parameters = function.Parameters
                        .Skip(1)
                        .ToList();

                    return new Value(
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
                }
            }

            return value;
        }

        throw new Error($"Anonymous record does not contain field '{memberName}'", position);
    }
}