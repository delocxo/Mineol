using System.Text;

class StringBuilderNative : INative
{
    public string[] Kinds { get; } = ["std_stringbuilder"];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
        int sbKind = ValueKind.GetId("std_stringbuilder");

        kindOperations.AddMemberGetter(sbKind, (target, memberName, pos) =>
        {
            StringBuilder sb = target.As<StringBuilder>();

            switch (memberName)
            {
                case "length":
                    return Value.FromFunction(
                        "length",
                        [],
                        (args, pos) => new Value(sb.Length)
                    );

                case "append":
                    return Value.FromFunction(
                        "append",
                        ["value"],
                        (args, pos) =>
                        {
                            sb.Append(args[0].ToString(pos));
                            return target;
                        }
                    );

                case "append_line":
                    return Value.FromFunction(
                        "append_line",
                        ["value"],
                        (args, pos) =>
                        {
                            sb.AppendLine(args[0].ToString(pos));
                            return target;
                        }
                    );

                case "clear":
                    return Value.FromFunction(
                        "clear",
                        [],
                        (args, pos) =>
                        {
                            sb.Clear();
                            return target;
                        }
                    );

                case "to_string":
                    return Value.FromFunction(
                            "to_string",
                            [],
                            (args, pos) =>
                            {
                                return new Value(sb.ToString());
                            }
                        );
            }

            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });

        kindOperations.AddToString(sbKind, (target, pos) =>
        {
            return target.As<StringBuilder>().ToString();
        });

        kindOperations.AddEquality(sbKind, (left, right, pos) =>
        {
            if (!right.KindIs(sbKind))
                return false;

            StringBuilder sb = left.As<StringBuilder>();

            return sb == right.As<StringBuilder>();
        });

        kindOperations.AddHash(sbKind, (target) =>
        {
            return target
                .As<StringBuilder>()
                .GetHashCode();
        });
    }

    public void Register(Dictionary<string, Value> globals)
    {
        int sbKind = ValueKind.GetId("std_stringbuilder");

        globals.AddFunction("sb_new", [], (args, pos) =>
        {
            return new Value(new StringBuilder(), sbKind);
        });

        globals.AddFunction("sb_from", ["from"], (args, pos) =>
        {
            return new Value(new StringBuilder(args[0].ToString(pos)), sbKind);
        });
    }
}
