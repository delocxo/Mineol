using TaskValue = System.Threading.Tasks.Task<Value>;

class TaskNative : INative
{
    public string[] Kinds { get; } = ["std_task"];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
        int taskKind = ValueKind.GetId("std_task");

        kindOperations.AddMemberGetter(taskKind, (target, memberName, pos) =>
        {
            TaskValue taskValue = target.As<TaskValue>();

            switch (memberName)
            {
                case "wait":
                    return Value.FromFunction(
                        "wait",
                        [],
                        (args, pos) =>
                        {
                            taskValue
                                .GetAwaiter()
                                .GetResult();
                            return target;
                        }
                    );

                case "result":
                    return Value.FromFunction(
                        "result",
                        [],
                        (args, pos) =>
                        {
                            return taskValue
                                .GetAwaiter()
                                .GetResult();
                        }
                    );

                case "is_completed":
                    return Value.FromFunction(
                        "is_completed",
                        [],
                        (args, pos) =>
                        {
                            return new Value(taskValue.IsCompleted);
                        }
                    );

                case "is_completed_successfully":
                    return Value.FromFunction(
                        "is_completed_successfully",
                        [],
                        (args, pos) =>
                        {
                            return new Value(taskValue.IsCompletedSuccessfully);
                        }
                    );

                case "is_faulted":
                    return Value.FromFunction(
                        "is_faulted",
                        [],
                        (args, pos) =>
                        {
                            return new Value(taskValue.IsFaulted);
                        }
                    );

                case "error":
                    return Value.FromFunction(
                        "error",
                        [],
                        (args, pos) =>
                        {
                            if (taskValue.Exception == null)
                                return new Value("");

                            return new Value(taskValue.Exception.GetBaseException().Message);
                        }
                    );
            }
            throw new Error($"{target.KindName} does not contain '{memberName}'", pos);
        });

        kindOperations.AddToString(taskKind, (target, pos) => "<task>");

        kindOperations.AddDefaultEquality<TaskValue>(taskKind);

        kindOperations.AddDefaultHash(taskKind);
    }

    public void Register(Dictionary<string, Value> globals)
    {
        int taskKind = ValueKind.GetId("std_task");

        globals.AddFunction("task_start", ["callback"], (args, pos) =>
        {
            var function = args[0].ExpectFunction("Expected a function callback", pos);

            TaskValue taskValue = Task.Run(() =>
            {
                return RuntimeFunctions.Call(args[0], [], pos);
            });

            return new Value(taskValue, taskKind);
        });
    }
}
