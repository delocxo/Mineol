class FloatNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
        kindOperations.AddMemberGetter(ValueKind.Float, (target, memberName, pos) =>
        {
            double value = target.Float;

            switch (memberName)
            {
                case "is_int":
                    return Value.FromFunction(
                        "is_int",
                        [],
                        (args, pos) =>
                        {
                            return new Value(double.IsInteger(value));
                        }
                    );

                case "is_nan":
                    return Value.FromFunction(
                        "is_nan",
                        [],
                        (args, pos) =>
                        {
                            return new Value(double.IsNaN(value));
                        }
                    );

                case "is_infinity":
                    return Value.FromFunction(
                        "is_infinity",
                        [],
                        (args, pos) =>
                        {
                            return new Value(double.IsInfinity(value));
                        }
                    );

                case "is_positive_infinity":
                    return Value.FromFunction(
                        "is_positive_infinity",
                        [],
                        (args, pos) =>
                        {
                            return new Value(double.IsPositiveInfinity(value));
                        }
                    );

                case "is_negative_infinity":
                    return Value.FromFunction(
                        "is_negative_infinity",
                        [],
                        (args, pos) =>
                        {
                            return new Value(double.IsNegativeInfinity(value));
                        }
                    );

                case "is_finite":
                    return Value.FromFunction(
                        "is_finite",
                        [],
                        (args, pos) =>
                        {
                            return new Value(double.IsFinite(value));
                        }
                    );

                case "is_real_number":
                    return Value.FromFunction(
                        "is_real_number",
                        [],
                        (args, pos) =>
                        {
                            return new Value(double.IsRealNumber(value));
                        }
                    );
            }

            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });
    }

    public void Register(Dictionary<string, Value> globals)
    {
        globals["float_max"] = new Value(double.MaxValue);
        globals["float_min"] = new Value(double.MinValue);
        globals["float_e"] = new Value(double.E);
        globals["float_tau"] = new Value(double.Tau);
        globals["float_pi"] = new Value(double.Pi);
        globals["float_half_pi"] = new Value(double.Pi / 2D);
        globals["float_positive_infinity"] = new Value(double.PositiveInfinity);
        globals["float_negative_infinity"] = new Value(double.NegativeInfinity);
        globals["float_nan"] = new Value(double.NaN);
        globals["float_epsilon"] = new Value(double.Epsilon);
    }
}