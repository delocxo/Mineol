class ListNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
        kindOperations.AddMemberGetter(ValueKind.List, (target, memberName, pos) =>
        {
            List<Value> list = target.List;

            switch (memberName)
            {
                case "length":
                    return new Value(target.List.Count);

                case "is_empty":
                    return new Value(target.List.Count == 0);

                case "push":
                    return Value.FromFunction(
                        "push",
                        ["value"],
                        (args, pos) =>
                        {
                            list.Add(args[0]);
                            return target;
                        }
                    );

                case "push_list":
                    return Value.FromFunction(
                        "push",
                        ["other_list"],
                        (args, pos) =>
                        {
                            List<Value> other = args[0].ExpectList(pos);
                            list.AddRange(other);
                            return target;
                        }
                    );

                case "try_push":
                    return Value.FromFunction(
                        "try_push",
                        ["value"],
                        (args, pos) =>
                        {
                            for (int i = 0; i < list.Count; i++)
                                if (args[0].CheckEquality(list[i]))
                                    return Value.False;
                            list.Add(args[0]);
                            return Value.True;
                        }
                    );

                case "remove":
                    return Value.FromFunction(
                        "remove",
                        ["value"],
                        (args, pos) =>
                        {
                            for (int i = 0; i < list.Count; i++)
                                if (args[0].CheckEquality(list[i]))
                                {
                                    list.RemoveAt(i);
                                    break;
                                }
                            return target;
                        }
                    );

                case "remove_at":
                    return Value.FromFunction(
                        "remove_at",
                        ["index"],
                        (args, pos) =>
                        {
                            int index = (int)args[0].ExpectIntInRangeEx(0, list.Count, pos);
                            list.RemoveAt(index);
                            return target;
                        }
                    );

                case "pop":
                    return Value.FromFunction(
                        "pop",
                        [],
                        (args, pos) =>
                        {
                            if (list.Count == 0)
                                throw new Error("Canno vt pop an empty list", pos);
                            Value value = list[^1];
                            list.RemoveAt(list.Count - 1);
                            return value;
                        }
                    );

                case "contains":
                    return Value.FromFunction(
                        "contains",
                        ["needle"],
                        (args, pos) =>
                        {
                            for (int i = 0; i < list.Count; i++)
                                if (args[0].CheckEquality(list[i]))
                                    return Value.True;
                            return Value.False;
                        }
                    );

                case "index_of":
                    return Value.FromFunction(
                        "index_of",
                        ["needle"],
                        (args, pos) =>
                        {
                            for (int i = 0; i < list.Count; i++)
                                if (args[0].CheckEquality(list[i]))
                                    return new Value(i);
                            return new Value(-1);
                        }
                    );

                case "copy":
                    return Value.FromFunction(
                        "copy",
                        [],
                        (args, pos) =>
                        {
                            return new Value([.. list]);
                        }
                    );

                case "reversed":
                    return Value.FromFunction(
                        "reversed",
                        [],
                        (args, pos) =>
                        {
                            List<Value> copy = [.. list];
                            copy.Reverse();
                            return new Value(copy);
                        }
                    );

                case "reverse":
                    return Value.FromFunction(
                        "reverse",
                        [],
                        (args, pos) =>
                        {
                            list.Reverse();
                            return target;
                        }
                    );

                case "slice":
                    return Value.FromFunction(
                        "slice",
                        ["start", "end"],
                        (args, pos) =>
                        {
                            int start = (int)args[0].ExpectIntInRangeIn("Invalid slice start", 0, list.Count, pos);
                            int end = (int)args[1].ExpectIntInRangeIn("invalid slice end", start, list.Count, pos);
                            return new Value(list.GetRange(start, end - start));
                        }
                    );
            }

            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });
    }

    public void Register(Dictionary<string, Value> globals)
    {

    }
}