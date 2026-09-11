class CastNative : INative
{
    public string[] Kinds => [];

    public void Register(Dictionary<string, Value> globals, NativeMembers nativeMembers)
    {
        globals.AddFunction("cast_string", ["value"], (args, _) =>
        {
            return new Value(args[0].ToString());
        });

        globals.AddFunction("cast_float", ["value"], (args, pos) =>
        {
            Value target = args[0];

            if (target.IsFloat())
                return target;

            else if (target.IsInt())
                return new Value((double)target.Int);

            else if (target.IsBool())
                return new Value(target.Bool ? 1D : 0D);

            else if (target.IsString())
            {
                if (double.TryParse(target.String, out double result))
                    return new Value(result);

                throw new Error("Failed cast string to float", pos);
            }

            throw new Error($"{target.KindName} cannot be casted to a float", pos);
        });

        globals.AddFunction("cast_int", ["value"], (args, pos) =>
        {
            Value target = args[0];

            if (target.IsFloat())
                return new Value((long)target.Float);

            else if (target.IsInt())
                return target;

            else if (target.IsBool())
                return new Value(target.Bool ? 1L : 0L);

            else if (target.IsString())
            {
                if (long.TryParse(target.String, out long result))
                    return new Value(result);

                throw new Error("Failed cast string to int", pos);
            }

            throw new Error($"{target.KindName} cannot be casted to an int", pos);
        });

        globals.AddFunction("cast_bool", ["value"], (args, pos) =>
        {
            Value target = args[0];

            if (target.IsFloat())
                return new Value(target.Float != 0);

            else if (target.IsInt())
                return new Value(target.Int != 0);

            else if (target.IsBool())
                return target;

            else if (target.IsString())
            {
                if (string.Equals(target.String, "true", StringComparison.Ordinal))
                    return Value.True;

                if (string.Equals(target.String, "false", StringComparison.Ordinal))
                    return Value.False;

                throw new Error("Failed to cast string to bool", pos);
            }

            throw new Error($"{target.KindName} cannot be casted to a bool", pos);
        });
    }
}