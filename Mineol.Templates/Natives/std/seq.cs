class SeqNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
        kindOperations.AddExtensionMemberGetter((target, memberName, pos, out value) =>
        {
            if (!target.IsIterable())
            {
                value = default;
                return false;
            }

            switch (memberName)
            {
                case "s_select":
                    value = Value.FromFunction(
                        "s_select",
                        ["callback"],
                        (args, pos) =>
                        {
                            args[0].ExpectCallback(pos);

                            List<Value> source = target.GetIterable(pos);
                            List<Value> result = new List<Value>(source.Count);

                            foreach (Value item in source)
                                result.Add(RuntimeFunctions.Call(args[0], [item], pos));

                            return new Value(result);
                        }
                    );

                    return true;

                case "s_to_list":
                    value = Value.FromFunction(
                        "s_to_list",
                        [],
                        (args, pos) => new Value(target.GetIterable(pos))
                    );

                    return true;

                case "s_filter":
                    value = Value.FromFunction(
                        "s_filter",
                        ["predicate"],
                        (args, pos) =>
                        {
                            args[0].ExpectPredicate(pos);

                            List<Value> source = target.GetIterable(pos);
                            List<Value> result = new List<Value>(source.Count);

                            foreach (Value item in source)
                            {
                                Value value = RuntimeFunctions.Call(args[0], [item], pos);
                                if (value.IsTruthy())
                                    result.Add(item);
                            }

                            return new Value(result);
                        }
                    );

                    return true;

                case "s_foreach":
                    value = Value.FromFunction(
                        "s_foreach",
                        ["callback"],
                        (args, pos) =>
                        {
                            args[0].ExpectCallback(pos);

                            List<Value> source = target.GetIterable(pos);

                            foreach (Value item in source)
                                RuntimeFunctions.Call(args[0], [item], pos);

                            return target;
                        }
                    );

                    return true;

                case "s_to_hashmap":
                    value = Value.FromFunction(
                        "s_to_hashmap",
                        ["key_callback", "value_callback"],
                        (args, pos) =>
                        {
                            if (!ValueKind.NameToId.TryGetValue("std_hashmap", out int hashmapKind))
                                throw new Error("to_hashmap requires std/hashmap.cs to be loaded", pos);

                            args[0].ExpectCallback(pos);
                            args[1].ExpectCallback(pos);

                            List<Value> source = target.GetIterable(pos);
                            HashMapObject hashMapObject = new HashMapObject(new(new ValueEqualityComparer()));

                            foreach (Value item in source)
                            {
                                Value key = RuntimeFunctions.Call(args[0], [item], pos);
                                Value mappedValue = RuntimeFunctions.Call(args[1], [item], pos);

                                key.GetHash(pos);

                                if (hashMapObject.Values.ContainsKey(key))
                                    throw new Error($"Duplicate key '{key.ToString(pos)}'", pos);

                                hashMapObject.Values[key] = mappedValue;
                            }

                            return new Value(hashMapObject, hashmapKind);
                        }
                    );

                    return true;

                case "s_take":
                    value = Value.FromFunction(
                        "s_take",
                        ["amount"],
                        (args, pos) =>
                        {
                            List<Value> source = target.GetIterable(pos);

                            int amount = (int)args[0].ExpectIntInRangeIn("Invalid take amount", 0, source.Count, pos);

                            List<Value> result = source.GetRange(0, amount);

                            return new Value(result);
                        }
                    );

                    return true;

                case "s_skip":
                    value = Value.FromFunction(
                        "s_skip",
                        ["amount"],
                        (args, pos) =>
                        {
                            List<Value> source = target.GetIterable(pos);

                            int amount = (int)args[0].ExpectIntInRangeIn("Invalid skip amount", 0, source.Count, pos);

                            List<Value> result = new List<Value>(source.Count - amount);

                            for (int i = amount; i < source.Count; i++)
                                result.Add(source[i]);

                            return new Value(result);
                        }
                    );

                    return true;
            }

            value = default;
            return false;
        });
    }

    public void Register(Dictionary<string, Value> globals)
    {
        globals.AddFunction(
                "seq_generate",
                ["count", "callback"],
                (args, pos) =>
                {
                    int amount = (int)args[0].ExpectIntInRangeIn("Invalid generate count", 0, int.MaxValue, pos);
                    args[1].ExpectCallback(pos);

                    List<Value> result = new List<Value>(amount);

                    for (int i = 0; i < amount; i++)
                        result.Add(RuntimeFunctions.Call(args[1], [new Value(i)], pos));

                    return new Value(result);
                }
        );
    }
}
