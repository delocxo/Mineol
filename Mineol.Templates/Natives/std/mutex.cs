using Mutex = System.Threading.SemaphoreSlim;

class MutexNative : INative
{
    public string[] Kinds { get; } = ["std_mutex"];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
        int mutexKind = ValueKind.GetId("std_mutex");

        kindOperations.AddMemberGetter(mutexKind, (target, memberName, pos) =>
        {
            Mutex mutex = target.As<Mutex>();

            switch (memberName)
            {
                case "lock":
                    return Value.FromFunction(
                        "lock",
                        [],
                        (args, pos) =>
                        {
                            mutex.Wait();
                            return target;
                        }
                    );

                case "unlock":
                    return Value.FromFunction(
                        "unlock",
                        [],
                        (args, pos) =>
                        {
                            mutex.Release();
                            return target;
                        }
                    );

                case "try_lock":
                    return Value.FromFunction(
                        "try_lock",
                        [],
                        (args, pos) =>
                        {
                            return new Value(mutex.Wait(0));
                        }
                    );

                case "try_lock_for":
                    return Value.FromFunction(
                        "try_lock_for",
                        ["ms"],
                        (args, pos) =>
                        {
                            int ms = (int)args[0].ExpectIntInRangeIn("Invalid milliseconds", 0, int.MaxValue, pos);
                            return new Value(mutex.Wait(ms));
                        }
                    );

                case "with_lock":
                    return Value.FromFunction(
                        "with_lock",
                        ["callback"],
                        (args, pos) =>
                        {
                            args[0].ExpectFunction("Expected a function callback", pos);

                            mutex.Wait();

                            try
                            {
                                return RuntimeFunctions.Call(args[0], [], pos);
                            }
                            finally
                            {
                                mutex.Release();
                            }
                        }
                    );

            }
            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });

        kindOperations.AddToString(mutexKind, (target) => "<mutex>");

        kindOperations.AddDefaultEquality<Mutex>(mutexKind);

        kindOperations.AddDefaultHash(mutexKind);
    }

    public void Register(Dictionary<string, Value> globals)
    {
        int mutexKind = ValueKind.GetId("std_mutex");

        globals.AddFunction("mutex_new", [], (args, pos) =>
        {
            return new Value(new Mutex(1, 1), mutexKind);
        });
    }
}