class HashMapObject
{
    public HashMapObject(OrderedDictionary<Value, Value> values)
    {
        Values = values;
    }

    public OrderedDictionary<Value, Value> Values { get; }
}

class HashMapNative : INative
{
    public string[] Kinds { get; } = ["std_hashmap"];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
        int hashMapKind = ValueKind.GetId("std_hashmap");

        kindOperations.AddMemberGetter(hashMapKind, (target, memberName, pos) =>
        {
            var hashmap = target.As<HashMapObject>().Values;

            switch (memberName)
            {
                case "count":
                    return Value.FromFunction(
                        "count",
                        [],
                        (args, pos) => new Value(hashmap.Count)
                    );

                case "is_empty":
                    return Value.FromFunction(
                        "is_empty",
                        [],
                        (args, pos) => new Value(hashmap.Count == 0)
                    );

                case "contains_key":
                    return Value.FromFunction(
                        "contains_key",
                        ["key"],
                        (args, pos) =>
                        {
                            return new Value(hashmap.ContainsKey(args[0]));
                        }
                    );

                case "contains_value":
                    return Value.FromFunction(
                        "contains_value",
                        ["value"],
                        (args, pos) =>
                        {
                            return new Value(hashmap.ContainsValue(args[0]));
                        }
                    );

                case "get_value_at":
                    return Value.FromFunction(
                        "get_value_at",
                        ["index"],
                        (args, pos) =>
                        {
                            int index = (int)args[0].ExpectIntInRangeEx(0, hashmap.Count, pos);
                            return hashmap.GetAt(index).Value;
                        }
                    );

                case "get_key_at":
                    return Value.FromFunction(
                        "get_key_at",
                        ["index"],
                        (args, pos) =>
                        {
                            int index = (int)args[0].ExpectIntInRangeEx(0, hashmap.Count, pos);
                            return hashmap.GetAt(index).Key;
                        }
                    );

                case "get_key_value_at":
                    return Value.FromFunction(
                        "get_key_value_at",
                        ["index"],
                        (args, pos) =>
                        {
                            int index = (int)args[0].ExpectIntInRangeEx(0, hashmap.Count, pos);
                            var keyValuePair = hashmap.GetAt(index);
                            return MakeKeyValuePair(keyValuePair.Key, keyValuePair.Value);
                        }
                    );

                case "try_get":
                    return Value.FromFunction(
                            "try_get",
                            ["key"],
                            (args, pos) =>
                            {
                                if (hashmap.TryGetValue(args[0], out Value value))
                                    return RuntimeFunctions.MakeResultRecord(value, true);

                                return RuntimeFunctions.MakeResultRecord(Value.Null, false);
                            }
                        );

                case "to_list":
                    return Value.FromFunction(
                            "to_list",
                            [],
                            (args, pos) =>
                            {
                                return HashMapToList(hashmap);
                            }
                        );

                case "copy":
                    return Value.FromFunction(
                            "copy",
                            [],
                            (args, pos) =>
                            {
                                var copied = new OrderedDictionary<Value, Value>(hashmap, new ValueEqualityComparer());
                                return new Value(new HashMapObject(copied), hashMapKind);
                            }
                        );

                case "reversed":
                    return Value.FromFunction(
                            "reversed",
                            [],
                            (args, pos) =>
                            {
                                var copied = new OrderedDictionary<Value, Value>(hashmap.Reverse(), new ValueEqualityComparer());
                                return new Value(new HashMapObject(copied), hashMapKind);
                            }
                        );

                case "reverse":
                    return Value.FromFunction(
                            "reverse",
                            [],
                            (args, pos) =>
                            {
                                var reversed = hashmap
                                    .Reverse()
                                    .ToList();

                                hashmap.Clear();

                                foreach (var pair in reversed)
                                    hashmap.Add(pair.Key, pair.Value);

                                return target;
                            }
                        );

                case "clear":
                    return Value.FromFunction(
                            "clear",
                            [],
                            (args, pos) =>
                            {
                                hashmap.Clear();
                                return target;
                            }
                        );

                case "remove":
                    return Value.FromFunction(
                            "remove",
                            ["key"],
                            (args, pos) =>
                            {
                                return new Value(hashmap.Remove(args[0]));
                            }
                        );

                case "keys":
                    return Value.FromFunction(
                            "keys",
                            [],
                            (args, pos) =>
                            {
                                return new Value(hashmap.Keys.ToList());
                            }
                        );

                case "values":
                    return Value.FromFunction(
                            "values",
                            [],
                            (args, pos) =>
                            {
                                return new Value(hashmap.Values.ToList());
                            }
                        );

            }

            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });

        kindOperations.AddIndexGetter(hashMapKind, (target, index, pos) =>
        {
            index.GetHash(pos);

            HashMapObject hashMapObject = target.As<HashMapObject>();

            if (hashMapObject.Values.TryGetValue(index, out Value value))
                return value;

            throw new Error($"'{index}' is not a valid key", pos);
        });

        kindOperations.AddIndexSetter(hashMapKind, (target, index, value, pos) =>
        {
            index.GetHash(pos);

            HashMapObject hashMapObject = target.As<HashMapObject>();

            hashMapObject.Values[index] = value;
        });

        kindOperations.AddEquality(hashMapKind, (left, right) =>
        {
            if (!right.KindIs(hashMapKind))
                return false;

            HashMapObject hashMapObject = left.As<HashMapObject>();

            return hashMapObject == right.As<HashMapObject>();
        });

        kindOperations.AddHash(hashMapKind, (target) =>
        {
            return target.As<HashMapObject>().GetHashCode();
        });

        kindOperations.AddToString(hashMapKind, (target) =>
        {
            HashMapObject hashMapObject = target.As<HashMapObject>();

            string[] keyValuePairs = hashMapObject.Values
                .Select(x =>
                     $"{x.Key.ToStringWithQuotes()}: {x.Value.ToStringWithQuotes()}"
                )
                .ToArray();

            return $"{{ {string.Join(", ", keyValuePairs)} }}";
        });

        kindOperations.AddIterable(hashMapKind, (target) =>
        {
            return HashMapToList(target.As<HashMapObject>().Values).List;
        });
    }

    public void Register(Dictionary<string, Value> globals)
    {
        int hashMapKind = ValueKind.GetId("std_hashmap");

        globals.AddFunction("hashmap_new", [], (args, pos) =>
        {
            return new Value(
                new HashMapObject(new OrderedDictionary<Value, Value>(new ValueEqualityComparer())),
                hashMapKind);
        });
    }

    Value MakeKeyValuePair(Value key, Value value)
    {
        RecordObject keyValuePair = new RecordObject(new OrderedDictionary<string, RecordField>
        {
            ["key"] = new RecordField(key, false),
            ["value"] = new RecordField(value, false)
        });

        return new Value(keyValuePair);
    }

    Value HashMapToList(OrderedDictionary<Value, Value> hashmap)
    {
        List<Value> list = new List<Value>(hashmap.Count);

        foreach (var keyValuePair in hashmap)
            list.Add(MakeKeyValuePair(keyValuePair.Key, keyValuePair.Value));

        return new Value(list);
    }
}