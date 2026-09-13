using System.Globalization;

class CastNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
    }

    public void Register(Dictionary<string, Value> globals)
    {
        globals.AddFunction("cast_to_string", ["value"], (args, pos) =>
        {
            return new Value(args[0].ToString());
        });

        globals.AddFunction("cast_to_int", ["value"], (args, pos) =>
        {
            Value value = args[0];

            if (value.IsInt())
                return value;

            else if (value.IsFloat())
                return new Value((long)value.Float);

            else if (value.IsBool())
                return new Value(value.Bool ? 1L : 0L);

            else if (value.IsString() &&
                long.TryParse(
                    value.String,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long result))
            {
                return new Value(result);
            }

            throw new Error($"{value.KindName} cannot be cast to int", pos);
        });

        globals.AddFunction("cast_to_float", ["value"], (args, pos) =>
        {
            Value value = args[0];

            if (value.IsFloat())
                return value;

            else if (value.IsInt())
                return new Value((double)value.Int);

            else if (value.IsBool())
                return new Value(value.Bool ? 1D : 0D);

            else if (value.IsString() &&
                double.TryParse(
                    value.String,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double result))
            {
                return new Value(result);
            }

            throw new Error($"{value.KindName} cannot be cast to float", pos);
        });

        globals.AddFunction("cast_to_bool", ["value"], (args, pos) =>
        {
            Value value = args[0];

            if (value.IsBool())
                return value;

            else if (value.IsFloat())
                return new Value(value.Float != 0);

            else if (value.IsInt())
                return new Value(value.Int != 0);

            else if (value.IsString())
            {
                if (string.Equals(value.String, "true", StringComparison.OrdinalIgnoreCase))
                    return Value.True;

                else if (string.Equals(value.String, "false", StringComparison.OrdinalIgnoreCase))
                    return Value.False;

                else if (string.Equals(value.String, "1", StringComparison.OrdinalIgnoreCase))
                    return Value.True;

                else if (string.Equals(value.String, "0", StringComparison.OrdinalIgnoreCase))
                    return Value.False;
            }

            throw new Error($"{value.KindName} cannot be cast to bool", pos); ;
        });
    }
}