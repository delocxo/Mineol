using System.IO;

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
            return new Value(File.Exists(args[0].ToString(pos)));
        });

        globals.AddFunction("file_read", ["path"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);

            return new Value(File.ReadAllText(filePath));
        });

        globals.AddFunction("file_write", ["path", "content"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);

            File.WriteAllText(filePath, args[1].ToString(pos));
            return Value.Null;
        });

        globals.AddFunction("file_append", ["path", "content"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);

            File.AppendAllText(filePath, args[1].ToString(pos));
            return Value.Null;
        });

        globals.AddFunction("file_delete", ["path"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);

            File.Delete(filePath);
            return Value.Null;
        });

        globals.AddFunction("file_copy", ["source", "destination"], (args, pos) =>
        {
            string source = args[0].ToString(pos);
            string destination = args[1].ToString(pos);

            File.Copy(source, destination);
            return Value.Null;
        });

        globals.AddFunction("file_move", ["source", "destination"], (args, pos) =>
        {
            string source = args[0].ToString(pos);
            string destination = args[1].ToString(pos);

            File.Move(source, destination);
            return Value.Null;
        });

        globals.AddFunction("file_size", ["path"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);

            return new Value(new FileInfo(filePath).Length);
        });

        globals.AddFunction("file_read_lines", ["path"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);

            string[] lines = File.ReadAllLines(filePath);

            List<Value> result = new List<Value>(lines.Length);

            foreach (string line in lines)
                result.Add(new Value(line));

            return new Value(result);
        });

        globals.AddFunction("file_write_lines", ["path", "lines"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);
            List<Value> lines = args[1].ExpectList(pos);

            string[] result = new string[lines.Count];

            for (int i = 0; i < lines.Count; i++)
                result[i] = lines[i].ToString(pos);

            File.WriteAllLines(filePath, result);

            return Value.Null;
        });
    }
}
