using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

class SysNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
    }

    public void Register(Dictionary<string, Value> globals)
    {
        globals.AddFunction("sys_args", [], (args, pos) =>
        {
            return new Value(Environment
                .GetCommandLineArgs()
                .Select(x => new Value(x))
                .ToList());
        });

        globals.AddFunction("sys_current_dir", [], (args, pos) =>
        {
            return new Value(Environment.CurrentDirectory);
        });

        globals.AddFunction("sys_processor_count", [], (args, pos) =>
        {
            return new Value(Environment.ProcessorCount);
        });

        globals.AddFunction("sys_is_64_bit_os", [], (args, pos) =>
        {
            return new Value(Environment.Is64BitOperatingSystem);
        });

        globals.AddFunction("sys_is_64_bit_process", [], (args, pos) =>
        {
            return new Value(Environment.Is64BitProcess);
        });

        globals.AddFunction("sys_runtime_version", [], (args, pos) =>
        {
            return new Value(RuntimeInformation.FrameworkDescription);
        });

        globals.AddFunction("sys_exit", ["exit_code"], (args, pos) =>
        {
            int exitCode = args[0].GetExitCode();
            Environment.Exit(exitCode);
            return Value.Null;
        });

        globals.AddFunction("sys_newline", [], (args, pos) =>
        {
            return new Value(Environment.NewLine);
        });

        globals.AddFunction("sys_process_path", [], (args, pos) =>
        {
            string? processPath = Environment.ProcessPath;
            return processPath != null ? new Value(processPath) : Value.Null;
        });

        globals.AddFunction("sys_user_name", [], (args, pos) =>
        {
            return new Value(Environment.UserName);
        });

        globals.AddFunction("sys_machine_name", [], (args, pos) =>
        {
            return new Value(Environment.MachineName);
        });

        globals.AddFunction("sys_tick_count", [], (args, pos) =>
        {
            return new Value(Environment.TickCount64);
        });

        globals.AddFunction("sys_os_version", [], (args, pos) =>
        {
            OperatingSystem os = Environment.OSVersion;

            Version version = os.Version;

            return RecordObject.Create(new OrderedDictionary<string, RecordField>
            {
                ["platform"] = new RecordField(new Value(os.Platform.ToString()), false),
                ["version"] = new RecordField(new Value(version.ToString()), false),
                ["major"] = new RecordField(new Value(version.Major), false),
                ["minor"] = new RecordField(new Value(version.Minor), false),
                ["build"] = new RecordField(new Value(version.Build), false),
                ["revision"] = new RecordField(new Value(version.Revision), false)
            });
        });

        globals.AddFunction("sys_user_domain", [], (args, pos) =>
        {
            return new Value(Environment.UserDomainName);
        });

        globals.AddFunction("sys_memory_usage", [], (args, pos) =>
        {
            return new Value(Environment.WorkingSet);
        });

        globals.AddFunction("sys_process_id", [], (args, pos) =>
        {
            return new Value(Environment.ProcessId);
        });

        globals.AddFunction("sys_system_dir", [], (args, pos) =>
        {
            return new Value(Environment.SystemDirectory);
        });

        globals.AddFunction("sys_user_interactive", [], (args, pos) =>
        {
            return new Value(Environment.UserInteractive);
        });

        globals.AddFunction("sys_current_managed_thread_id", [], (args, pos) =>
        {
            return new Value(Environment.CurrentManagedThreadId);
        });

        globals.AddFunction("sys_system_page_size", [], (args, pos) =>
        {
            return new Value(Environment.SystemPageSize);
        });

        globals.AddFunction("sys_command_line", [], (args, pos) =>
        {
            return new Value(Environment.CommandLine);
        });

        globals.AddFunction("sys_is_privileged_process", [], (args, pos) =>
        {
            return new Value(Environment.IsPrivilegedProcess);
        });

        globals.AddFunction("sys_has_shutdown_started", [], (args, pos) =>
        {
            return new Value(Environment.HasShutdownStarted);
        });

        globals.AddFunction("sys_run", ["command", "args"], (args, pos) =>
        {
            string command = args[0].ExpectString("Expected a string command", pos);
            string[] arguments = args[1]
                .ExpectList(pos)
                .Select(x => x.ExpectString("Expected an string argument", pos))
                .ToArray();

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = command,
                    UseShellExecute = true
                };

                foreach (string arg in arguments)
                    startInfo.ArgumentList.Add(arg);

                Process.Start(startInfo);

                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"Process start error: {e.Message}", pos);
            }
        });
    }
}