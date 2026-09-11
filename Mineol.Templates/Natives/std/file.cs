class FileNative : INative
{
    public string[] Kinds => [];

    public void Register(Dictionary<string, Value> globals, NativeMembers nativeMembers)
    {
        globals.AddFunction("file_read", ["path"], (args, pos) =>
        {
            string filePath = args[0].ExpectString("Expected string file path", pos);

            try
            {
                return new Value(File.ReadAllText(filePath));
            }
            catch (Exception e)
            {
                throw new Error($"File Error: {e}", pos);
            }
        });

        globals.AddFunction("file_write", ["path", "content"], (args, pos) =>
        {
            string filePath = args[0].ExpectString("Expected string file path", pos);
            Value content = args[0];
            try
            {
                File.WriteAllText(filePath, content.ToString());
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File Error: {e}", pos);
            }
        });

        globals.AddFunction("file_append", ["path", "content"], (args, pos) =>
        {
            string filePath = args[0].ExpectString("Expected string file path", pos);
            Value content = args[0];
            try
            {
                File.AppendAllText(filePath, content.ToString());
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File Error: {e}", pos);
            }
        });

        globals.AddFunction("file_delete", ["path"], (args, pos) =>
        {
            string filePath = args[0].ExpectString("Expected string file path", pos);
            try
            {
                File.Delete(filePath);
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File Error: {e}", pos);
            }
        });

        globals.AddFunction("file_exist", ["path"], (args, pos) =>
        {
            string filePath = args[0].ExpectString("Expected string file path", pos);
            return new Value(File.Exists(filePath));
        });

        globals.AddFunction("file_copy", ["source", "dest"], (args, pos) =>
        {
            string source = args[0].ExpectString("Expected string file path", pos);
            string dest = args[1].ExpectString("Expected string file path", pos);
            try
            {
                File.Copy(source, dest);
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File Error: {e}", pos);
            }
        });

        globals.AddFunction("file_move", ["source", "dest"], (args, pos) =>
        {
            string source = args[0].ExpectString("Expected string file path", pos);
            string dest = args[1].ExpectString("Expected string file path", pos);
            try
            {
                File.Move(source, dest);
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File Error: {e}", pos);
            }
        });

        globals.AddFunction("file_replace", ["source", "dest", "backup"], (args, pos) =>
        {
            string source = args[0].ExpectString("Expected string file path", pos);
            string dest = args[1].ExpectString("Expected string file path", pos);
            string backup = args[2].ExpectString("Expected string file path", pos);
            try
            {
                File.Replace(source, dest, backup);
                return Value.Null;
            }
            catch (Exception e)
            {
                throw new Error($"File Error: {e}", pos);
            }
        });
    }
}