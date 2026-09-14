using System.Diagnostics;

class StopWatchNative : INative
{
    public string[] Kinds { get; } = ["std_stopwatch"];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
        int swKind = ValueKind.GetId("std_stopwatch");

        kindOperations.AddMemberGetter(swKind, (target, memberName, pos) =>
        {
            Stopwatch sw = target.As<Stopwatch>();

            switch (memberName)
            {
                case "elapsed_ms":
                    return Value.FromFunction(
                        "elapsed_ms",
                        [],
                        (args, pos) =>
                        {
                            return new Value(sw.ElapsedMilliseconds);
                        }
                    );

                case "elapsed_ticks":
                    return Value.FromFunction(
                        "elapsed_ticks",
                        [],
                        (args, pos) =>
                        {
                            return new Value(sw.ElapsedTicks);
                        }
                    );

                case "is_running":
                    return Value.FromFunction(
                        "is_running",
                        [],
                        (args, pos) =>
                        {
                            return new Value(sw.IsRunning);
                        }
                    );

                case "reset":
                    return Value.FromFunction(
                        "reset",
                        [],
                        (args, pos) =>
                        {
                            sw.Reset();
                            return target;
                        }
                    );

                case "restart":
                    return Value.FromFunction(
                        "restart",
                        [],
                        (args, pos) =>
                        {
                            sw.Restart();
                            return target;
                        }
                    );

                case "start":
                    return Value.FromFunction(
                        "start",
                        [],
                        (args, pos) =>
                        {
                            sw.Start();
                            return target;
                        }
                    );

                case "stop":
                    return Value.FromFunction(
                        "stop",
                        [],
                        (args, pos) =>
                        {
                            sw.Stop();
                            return target;
                        }
                    );
            }

            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });

        kindOperations.AddToString(swKind, (target) =>
        {
            return target
                .As<Stopwatch>()
                .ToString();
        });

        kindOperations.AddDefaultEquality<Stopwatch>(swKind);

        kindOperations.AddDefaultHash(swKind);
    }

    public void Register(Dictionary<string, Value> globals)
    {
        int swKind = ValueKind.GetId("std_stopwatch");

        globals.AddFunction("stopwatch_new", [], (args, pos) =>
        {
            return new Value(Stopwatch.StartNew(), swKind);
        });
    }
}