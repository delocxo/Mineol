class GeneralNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
    }

    public void Register(Dictionary<string, Value> globals)
    {
        globals.AddFunction("kind_of", ["value"], (args, pos) =>
        {
            return new Value(args[0].KindName);
        });

        globals.AddFunction("kind_is", ["value", "kind"], (args, pos) =>
        {
            string kindName = args[1].ExpectString("Expected a kind name", pos);

            if (!ValueKind.NameToId.TryGetValue(kindName, out int kind))
                throw new Error($"'{kindName}' is not a valid kind", pos);

            return new Value(args[0].Kind == kind);
        });

        globals.AddFunction("range", ["min", "max"], (args, pos) =>
        {
            int min = (int)args[0].ExpectIntInRangeIn("Invalid min range", int.MinValue, int.MaxValue, pos);
            int max = (int)args[1].ExpectIntInRangeIn("Invalid max range", int.MinValue, int.MaxValue, pos);
            return new Value(Range(min, max, 1, pos).ToList());
        });

        globals.AddFunction("range_by", ["min", "max", "step"], (args, pos) =>
        {
            int min = (int)args[0].ExpectIntInRangeIn("Invalid min range", int.MinValue, int.MaxValue, pos);
            int max = (int)args[1].ExpectIntInRangeIn("Invalid max range", int.MinValue, int.MaxValue, pos);
            int step = (int)args[2].ExpectIntInRangeIn("Invalid range step", int.MinValue, int.MaxValue, pos);
            return new Value(Range(min, max, step, pos).ToList());
        });

        globals.AddFunction("try_call", ["callback"], (args, pos) =>
        {
            var function = args[0].ExpectFunction("Expected a function callback", pos);

            try
            {
                Value result = RuntimeFunctions.Call(args[0], [], pos);
                return MakeTryCallResult(true, result, "", false);
            }
            catch (Error e)
            {
                return MakeTryCallResult(false, e.ErrorValue, e.Message, false);
            }
            catch (Exception e)
            {
                return MakeTryCallResult(false, Value.Null, e.Message, true);
            }
        });

        globals.AddFunction("panic", ["message"], (args, pos) =>
        {
            throw new Error(args[0].ToString(pos), pos);
        });

        globals.AddFunction("enumerate", ["iterable"], (args, pos) =>
        {
            List<Value> values = args[0].GetIterable(pos);

            List<Value> result = new List<Value>(values.Count);

            for (int i = 0; i < values.Count; i++)
            {
                RecordObject resultItem = new RecordObject(new()
                {
                    ["value"] = new RecordField(values[i], false),
                    ["index"] = new RecordField(new Value(i), false)
                });

                result.Add(new Value(resultItem));
            }

            return new Value(result);
        });

        globals.AddFunction("panic_with", ["message", "value"], (args, pos) =>
        {
            throw new Error(args[0].ToString(pos), args[1], pos);
        });

        globals.AddFunction("enforce", ["value", "kind"], (args, pos) =>
        {
            string kindName = args[1].ExpectString("Expected a kind name", pos);

            if (!ValueKind.NameToId.TryGetValue(kindName, out int kind))
                throw new Error($"'{kindName}' is not a valid kind", pos);

            if (args[0].Kind != kind)
                throw new Error($"Expected kind '{kindName}', got '{args[0].KindName}'", pos);

            return args[0];
        });

        globals.AddFunction("enforce_msg", ["value", "kind", "msg"], (args, pos) =>
        {
            string kindName = args[1].ExpectString("Expected a kind name", pos);

            if (!ValueKind.NameToId.TryGetValue(kindName, out int kind))
                throw new Error($"'{kindName}' is not a valid kind", pos);

            if (args[0].Kind != kind)
                throw new Error(args[2].ToString(pos), pos);

            return args[0];
        });
    }

    static IEnumerable<Value> Range(int start, int stop, int step, Position position)
    {
        if (step == 0)
            throw new Error("Step cannot be zero", position);

        if (step > 0)
        {
            for (int i = start; i < stop; i += step)
                yield return new Value(i);
        }
        else
        {
            for (int i = start; i > stop; i += step)
                yield return new Value(i);
        }
    }

    static Value MakeTryCallResult(bool success, Value data, string errMessage, bool isInternalError)
    {
        RecordObject result = new RecordObject(new OrderedDictionary<string, RecordField>
        {
            ["success"] = new RecordField(new Value(success), false),
            ["data"] = new RecordField(data, false),
            ["error"] = new RecordField(new Value(errMessage), false),
            ["is_internal_error"] = new RecordField(new Value(isInternalError), false)
        });

        return new Value(result);
    }
}
