static class ConsoleExtensions
{
    public static void WriteErrorExit(object obj)
    {
        Console.Error.WriteLine(obj);
        Environment.Exit(1);
    }
}