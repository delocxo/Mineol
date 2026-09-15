class ReflectionNative : INative
{
    public string[] Kinds => [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
        kindOperations.AddMemberGetter(ValueKind.Enum, (target, memberName, pos) =>
        {
            EnumObject enumObject = target.EnumObject;

            switch (memberName)
            {
                case "name":
                    return Value.FromFunction(
                        "name",
                        [],
                        (args, pos) =>
                        {
                            return new Value(enumObject.Name);
                        }
                    );

                case "members":
                    return Value.FromFunction(
                        "members",
                        [],
                        (args, pos) =>
                        {
                            List<Value> result = new List<Value>(enumObject.Members.Count);

                            foreach (var member in enumObject.Members)
                                result.Add(MakeEnumValueRecord(member.Value));

                            return new Value(result);
                        }
                    );

                case "has_member":
                    return Value.FromFunction(
                        "has_member",
                        ["member_name"],
                        (args, pos) =>
                        {
                            string name = args[0].ExpectString("Expected a member name", pos);
                            return new Value(enumObject.Members.ContainsKey(name));
                        }
                    );

                case "get_member":
                    return Value.FromFunction(
                        "get_member",
                        ["member_name"],
                        (args, pos) =>
                        {
                            string name = args[0].ExpectString("Expected a member name", pos);

                            if (!enumObject.Members.TryGetValue(name, out EnumValue? enumValue))
                                throw new Error($"'{enumObject.Name}' does not contain member '{name}'", pos);

                            return MakeEnumValueRecord(enumValue);
                        }
                    );

                case "get_member_at":
                    return Value.FromFunction(
                        "get_member_at",
                        ["index"],
                        (args, pos) =>
                        {
                            int index = (int)args[0].ExpectIntInRangeEx("Invalid index", 0, enumObject.Members.Count, pos);
                            return MakeEnumValueRecord(enumObject.Members.GetAt(index).Value);
                        }
                    );

                case "info":
                    return Value.FromFunction(
                        "info",
                        [],
                        (args, pos) =>
                        {
                            List<Value> members = new List<Value>(enumObject.Members.Count);

                            foreach (var member in enumObject.Members)
                                members.Add(MakeEnumValueRecord(member.Value));

                            RecordObject enumRecord = new RecordObject(new OrderedDictionary<string, RecordField>
                            {
                                ["name"] = new RecordField(new Value(enumObject.Name), false),
                                ["members"] = new RecordField(new Value(members), false)
                            });

                            return new Value(enumRecord);
                        }
                    );
            }

            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });

        kindOperations.AddMemberGetter(ValueKind.Function, (target, memberName, pos) =>
        {
            FunctionObject function = target.FunctionObject;

            switch (memberName)
            {
                case "name":
                    return Value.FromFunction(
                        "name",
                        [],
                        (args, pos) =>
                        {
                            return function.Name != "" ? new Value(function.Name) : Value.Null;
                        }
                    );

                case "arity":
                    return Value.FromFunction(
                        "arity",
                        [],
                        (args, pos) =>
                        {
                            return new Value(function.Arity);
                        }
                    );

                case "parameters":
                    return Value.FromFunction(
                        "parameters",
                        [],
                        (args, pos) =>
                        {
                            return new Value(function.Parameters
                                .Select(x => new Value(x))
                                .ToList());
                        }
                    );

                case "has_parameter":
                    return Value.FromFunction(
                        "has_parameter",
                        ["parameter"],
                        (args, pos) =>
                        {
                            string name = args[0].ExpectString("Expected a parameter name", pos);
                            return new Value(function.Parameters.Contains(name));
                        }
                    );

                case "info":
                    return Value.FromFunction(
                        "info",
                        [],
                        (args, pos) =>
                        {
                            RecordObject functionRecord = new RecordObject(new OrderedDictionary<string, RecordField>
                            {
                                ["name"] = new RecordField(function.Name != "" ? new Value(function.Name) : Value.Null, false),
                                ["parameters"] = new RecordField(new Value(function.Parameters
                                .Select(x => new Value(x))
                                .ToList()), false),
                                ["arity"] = new RecordField(new Value(function.Arity), false)

                            });

                            return new Value(functionRecord);
                        }
                    );
            }

            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });

        kindOperations.AddMemberGetter(ValueKind.EnumValue, (target, memberName, pos) =>
        {
            EnumValue enumValue = target.EnumValue;

            switch (memberName)
            {
                case "name":
                    return Value.FromFunction(
                        "name",
                        [],
                        (args, pos) =>
                        {
                            return new Value(enumValue.MemberName);
                        }
                    );

                case "enum_name":
                    return Value.FromFunction(
                        "enum_name",
                        [],
                        (args, pos) =>
                        {
                            return new Value(enumValue.EnumName);
                        }
                    );

                case "index":
                    return Value.FromFunction(
                        "index",
                        [],
                        (args, pos) =>
                        {
                            return new Value(enumValue.Index);
                        }
                    );

                case "info":
                    return Value.FromFunction(
                        "info",
                        [],
                        (args, pos) =>
                        {
                            return MakeEnumValueRecord(enumValue);
                        }
                    );
            }

            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });

        kindOperations.AddMemberGetter(ValueKind.Record, (target, memberName, pos) =>
{
    RecordObject recordObject = target.RecordObject;

    switch (memberName)
    {
        case "fields":
            return Value.FromFunction(
                "fields",
                [],
                (args, pos) =>
                {
                    List<Value> result = new List<Value>(recordObject.Fields.Count);

                    foreach (var field in recordObject.Fields)
                        result.Add(MakeRecordFieldRecord(field.Key, field.Value));

                    return new Value(result);
                }
            );

        case "has_field":
            return Value.FromFunction(
                "has_field",
                ["field_name"],
                (args, pos) =>
                {
                    string name = args[0].ExpectString("Expected a field name", pos);
                    return new Value(recordObject.Fields.ContainsKey(name));
                }
            );

        case "get_field":
            return Value.FromFunction(
                "get_field",
                ["field_name"],
                (args, pos) =>
                {
                    string name = args[0].ExpectString("Expected a field name", pos);

                    if (!recordObject.Fields.TryGetValue(name, out RecordField? field))
                        throw new Error($"Record does not contain field '{name}'", pos);

                    return MakeRecordFieldRecord(name, field);
                }
            );

        case "get_field_at":
            return Value.FromFunction(
                "get_field_at",
                ["index"],
                (args, pos) =>
                {
                    int index = (int)args[0].ExpectIntInRangeEx("Invalid index", 0, recordObject.Fields.Count, pos);

                    var field = recordObject.Fields.GetAt(index);

                    return MakeRecordFieldRecord(field.Key, field.Value);
                }
            );

        case "field_count":
            return Value.FromFunction(
                "field_count",
                [],
                (args, pos) =>
                {
                    return new Value(recordObject.Fields.Count);
                }
            );

        case "info":
            return Value.FromFunction(
                "info",
                [],
                (args, pos) =>
                {
                    List<Value> fields = new List<Value>(recordObject.Fields.Count);

                    foreach (var field in recordObject.Fields)
                        fields.Add(MakeRecordFieldRecord(field.Key, field.Value));

                    RecordObject recordInfo = new RecordObject(new OrderedDictionary<string, RecordField>
                    {
                        ["field_count"] = new RecordField(new Value(recordObject.Fields.Count), false),
                        ["fields"] = new RecordField(new Value(fields), false)
                    });

                    return new Value(recordInfo);
                }
            );
    }

    throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
});
    }

    public void Register(Dictionary<string, Value> globals)
    {

    }

    Value MakeEnumValueRecord(EnumValue enumValue)
    {
        RecordObject enumValueRecord = new RecordObject(new OrderedDictionary<string, RecordField>
        {
            ["enum_name"] = new RecordField(new Value(enumValue.EnumName), false),
            ["name"] = new RecordField(new Value(enumValue.MemberName), false),
            ["index"] = new RecordField(new Value(enumValue.Index), false)
        });

        return new Value(enumValueRecord);
    }

    Value MakeRecordFieldRecord(string name, RecordField field)
    {
        RecordObject fieldRecord = new RecordObject(new OrderedDictionary<string, RecordField>
        {
            ["name"] = new RecordField(new Value(name), false),
            ["value"] = new RecordField(field.Value, false),
            ["is_const"] = new RecordField(new Value(field.isConst), false)
        });

        return new Value(fieldRecord);
    }
}