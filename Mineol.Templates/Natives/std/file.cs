using System.Globalization;

class FileNative : INative
{
    public string[] Kinds { get; } = [];

    public void RegiserKindOperations(KindOperations kindOperations)
    {
    }

    public void Register(Dictionary<string, Value> globals)
    {
        globals.AddFunction("file_exists", ["path"], (args, pos) =>
        {
            return new Value(File.Exists(args[0].ToString()));
        });

        globals.AddFunction("file_read", ["path"], (args, pos) =>
        {
            string filePath = args[0].ToString();
            CheckFileExistence(filePath, pos);
            try
            {
                return new Value(File.ReadAllText(filePath));
            }
            catch (Exception e)
            {
                throw new Error($"File read error: {e.Message}", pos);
            }
        });

        globals.AddFunction("file_write", ["path", "content"], (args, pos) =>
        {
            string filePath = args[0].ToString();
            try
            {
                File.WriteAllText(filePath, args[1].ToString());
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File write error: {e.Message}", pos);
            }
        });
    }

    void CheckFileExistence(string path, Position position)
    {
        if (!File.Exists(path))
            throw new Error($"File '{path}' does not exist", position);
    }
}