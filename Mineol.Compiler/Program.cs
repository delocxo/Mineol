using System.Diagnostics;
using System.Runtime.InteropServices;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: Mineol <file.mnl>");
    Environment.Exit(1);
}

try
{
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

    // string rid = RuntimeInformation.RuntimeIdentifier;

    // var startInfo = new ProcessStartInfo
    // {
    //     FileName = "dotnet",
    //     Arguments = $"publish generated.cs -c Release -r {rid} --self-contained -o output"
    // };

    // Process.Start(startInfo)!.WaitForExit();
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
