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
                return MakeTryCallResult(true, result, "");
            }
            catch (Error e)
            {
                return MakeTryCallResult(false, Value.Null, e.Message);
            }
            catch (Exception e)
            {
                return MakeTryCallResult(false, Value.Null, e.Message);
            }
        });

        globals.AddFunction("panic", ["message"], (args, pos) =>
        {
            throw new Error(args[0].ToString(), pos);
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

    static Value MakeTryCallResult(bool success, Value data, string errMessage)
    {
        RecordObject result = new RecordObject(new OrderedDictionary<string, RecordField>
        {
            ["success"] = new RecordField(new Value(success), false),
            ["data"] = new RecordField(data, false),
            ["error"] = new RecordField(new Value(errMessage), false)
        });

        return new Value(result);
    }
}