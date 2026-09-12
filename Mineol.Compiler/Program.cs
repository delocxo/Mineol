using System.Diagnostics;
using System.Runtime.InteropServices;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: Mineol <file.mnl> run/build");
    Environment.Exit(1);
}

try
{
    RunType runType;

    if (args[1] == "run")
        runType = RunType.Run;

    else if (args[1] == "build")
        runType = RunType.Build;

    else
    {
        Console.Error.WriteLine($"Unknown command '{args[1]}'. Expected 'run' or 'build'.");
        Environment.Exit(1);
        return;
    }

    DotnetInstalled.Check();

    Compiler compiler = new Compiler();
    compiler.CompileFile(args[0], true, new Position());

    SourceGenerator sourceGenerator = new SourceGenerator();
    string result = sourceGenerator.Combine(
        compiler.StringBuilder.ToString(),
        compiler.Uses
            .Select(x => new ValueTuple<string, Position>(x.Key, x.Value))
            .ToList()
        );

    File.WriteAllText("generated.cs", result);

    string rid = RuntimeInformation.RuntimeIdentifier;

    string arguments = "";

    if (runType == RunType.Build)
        arguments = $"publish generated.cs -c Release -r {rid} --self-contained -o output";
    else if (runType == RunType.Run)
    {
        string programArgs = string.Join(" ", args.Skip(2));
        arguments = $"run generated.cs -c Release -- {programArgs}";
    }

    var startInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = arguments
    };

    Process.Start(startInfo)!.WaitForExit();
}
catch (Error e)
{
    e.Exit();
}

enum RunType
{
    Build,
    Run
}
