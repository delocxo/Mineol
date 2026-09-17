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
            string filePath = args[0].ToString(pos);

            try
            {
                File.WriteAllText(filePath, args[1].ToString(pos));
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File write error: {e.Message}", pos);
            }
        });

        globals.AddFunction("file_append", ["path", "content"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);

            try
            {
                File.AppendAllText(filePath, args[1].ToString(pos));
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File append error: {e.Message}", pos);
            }
        });

        globals.AddFunction("file_delete", ["path"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);
            CheckFileExistence(filePath, pos);

            try
            {
                File.Delete(filePath);
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File delete error: {e.Message}", pos);
            }
        });

        globals.AddFunction("file_copy", ["source", "destination"], (args, pos) =>
        {
            string source = args[0].ToString(pos);
            string destination = args[1].ToString(pos);

            CheckFileExistence(source, pos);

            try
            {
                File.Copy(source, destination);
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File copy error: {e.Message}", pos);
            }
        });

        globals.AddFunction("file_move", ["source", "destination"], (args, pos) =>
        {
            string source = args[0].ToString(pos);
            string destination = args[1].ToString(pos);

            CheckFileExistence(source, pos);

            try
            {
                File.Move(source, destination);
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File move error: {e.Message}", pos);
            }
        });

        globals.AddFunction("file_size", ["path"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);
            CheckFileExistence(filePath, pos);

            try
            {
                return new Value(new FileInfo(filePath).Length);
            }
            catch (Exception e)
            {
                throw new Error($"File size error: {e.Message}", pos);
            }
        });

        globals.AddFunction("file_read_lines", ["path"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);
            CheckFileExistence(filePath, pos);

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                List<Value> result = new List<Value>(lines.Length);

                foreach (string line in lines)
                    result.Add(new Value(line));

                return new Value(result);
            }
            catch (Exception e)
            {
                throw new Error($"File read lines error: {e.Message}", pos);
            }
        });

        globals.AddFunction("file_write_lines", ["path", "lines"], (args, pos) =>
        {
            string filePath = args[0].ToString(pos);
            List<Value> lines = args[1].ExpectList(pos);

            try
            {
                string[] result = new string[lines.Count];

                for (int i = 0; i < lines.Count; i++)
                    result[i] = lines[i].ToString(pos);

                File.WriteAllLines(filePath, result);

                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File write lines error: {e.Message}", pos);
            }
        });
    }

    void CheckFileExistence(string path, Position position)
    {
        if (!File.Exists(path))
            throw new Error($"File '{path}' does not exist", position);
    }
}
