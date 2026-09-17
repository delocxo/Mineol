using System.Globalization;
using System.Text;

class StringNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
        kindOperations.AddMemberGetter(ValueKind.String, (target, memberName, pos) =>
        {
            string value = target.String;

            switch (memberName)
            {
                case "length":
                    return Value.FromFunction(
                        "length",
                        [],
                        (args, pos) => new Value(value.Length)
                    );

                case "is_empty":
                    return Value.FromFunction(
                        "is_empty",
                        [],
                        (args, pos) => new Value(value.Length == 0)
                    );

                case "is_whitespace":
                    return Value.FromFunction(
                        "is_whitespace",
                        [],
                        (args, pos) =>
                        {
                            return new Value(string.IsNullOrWhiteSpace(value));
                        }
                    );

                case "is_int":
                    return Value.FromFunction(
                        "is_int",
                        [],
                        (args, pos) =>
                        {
                            return new Value(long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _));
                        }
                    );

                case "is_number":
                    return Value.FromFunction(
                        "is_number",
                        [],
                        (args, pos) =>
                        {
                            return new Value(double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _));
                        }
                    );

                case "starts_with":
                    return Value.FromFunction(
                        "starts_with",
                        ["needle"],
                        (args, pos) =>
                        {
                            return new Value(value.StartsWith(args[0].ToString(pos), StringComparison.Ordinal));
                        }
                    );

                case "ends_with":
                    return Value.FromFunction(
                        "ends_with",
                        ["needle"],
                        (args, pos) =>
                        {
                            return new Value(value.EndsWith(args[0].ToString(pos), StringComparison.Ordinal));
                        }
                    );

                case "to_lower":
                    return Value.FromFunction(
                        "to_lower",
                        [],
                        (args, pos) =>
                        {
                            return new Value(value.ToLowerInvariant());
                        }
                    );

                case "to_upper":
                    return Value.FromFunction(
                        "to_upper",
                        [],
                        (args, pos) =>
                        {
                            return new Value(value.ToUpperInvariant());
                        }
                    );

                case "contains":
                    return Value.FromFunction(
                        "contains",
                        ["needle"],
                        (args, pos) =>
                        {
                            return new Value(value.Contains(args[0].ToString(pos), StringComparison.Ordinal));
                        }
                    );

                case "index_of":
                    return Value.FromFunction(
                        "index_of",
                        ["needle"],
                        (args, pos) =>
                        {
                            return new Value(value.IndexOf(args[0].ToString(pos), StringComparison.Ordinal));
                        }
                    );

                case "slice":
                    return Value.FromFunction(
                        "slice",
                        ["start", "end"],
                        (args, pos) =>
                        {
                            int start = (int)args[0].ExpectIntInRangeIn("Invalid slice start", 0, value.Length, pos);
                            int end = (int)args[1].ExpectIntInRangeIn("invalid slice end", start, value.Length, pos);
                            return new Value(value.Substring(start, end - start));
                        }
                    );

                case "replace":
                    return Value.FromFunction(
                        "replace",
                        ["old", "new"],
                        (args, pos) =>
                        {
                            return new Value(value.Replace(
                                args[0].ToString(pos),
                                args[1].ToString(pos),
                                StringComparison.Ordinal
                            ));
                        }
                    );

                case "trim_start":
                    return Value.FromFunction(
                        "trim_start",
                        [],
                        (args, pos) =>
                        {
                            return new Value(value.TrimStart());
                        }
                    );

                case "trim_end":
                    return Value.FromFunction(
                        "trim_end",
                        [],
                        (args, pos) =>
                        {
                            return new Value(value.TrimEnd());
                        }
                    );

                case "trim":
                    return Value.FromFunction(
                        "trim",
                        [],
                        (args, pos) =>
                        {
                            return new Value(value.Trim());
                        }
                    );

                case "split":
                    return Value.FromFunction(
                        "split",
                        ["separator"],
                        (args, pos) =>
                        {
                            return new Value(value
                                .Split(args[0].ToString(pos))
                                .Select(x => new Value(x))
                                .ToList());
                        }
                    );
            }

            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });
    }

    public void Register(Dictionary<string, Value> globals)
    {
        globals["string_empty"] = new Value(string.Empty);
        globals["string_newline"] = new Value("\n");

        globals.AddFunction("string_join", ["separator", "items"], (args, pos) =>
        {
            if (args[1].IsIterable())
                return new Value(string.Join(
                    args[0].ToString(pos),
                    args[1].GetIterable(pos).Select(x => x.ToString(pos))));

            throw new Error($"{args[1].KindName} cannot be joined", pos);
        });

        globals.AddFunction("string_format", ["template", "items"], (args, pos) =>
        {
            string template = args[0].ExpectString("Expected a format template", pos);
            List<Value> items = args[1].ExpectList(pos);

            StringBuilder sb = new StringBuilder();

            int currentArg = 0;

            for (int i = 0; i < template.Length; i++)
            {
                char c = template[i];

                if (c != '$')
                {
                    sb.Append(c);
                    continue;
                }

                if (i + 1 < template.Length && template[i + 1] == '$')
                {
                    sb.Append('$');
                    i++;
                    continue;
                }

                if (currentArg >= items.Count)
                    throw new Error("Not enough arguments for string format", pos);

                sb.Append(items[currentArg++].ToString(pos));
            }

            if (currentArg < items.Count)
                throw new Error("Too many arguments for string format", pos);

            return new Value(sb.ToString());
        });
    }
}
