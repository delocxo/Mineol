using System.Diagnostics;

static class DotnetInstalled
{
    public static void Check()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                ConsoleExtensions.WriteErrorExit("Dotnet is not installed");
            }

            process!.WaitForExit();

            if (process!.ExitCode == 1)
                ConsoleExtensions.WriteErrorExit("Dotnet is not installed");
        }
        catch
        {
            ConsoleExtensions.WriteErrorExit("Dotnet is not installed");
        }
    }
}