using System.Text;
using Microsoft.CodeAnalysis.CSharp;

class SourceGenerator
{
    const string _rumtimePath = "Templates/Runtime";
    const string _nativesPath = "Templates/Natives";

    public string Combine(string generated, List<(string Path, Position position)> uses)
    {
        string exePath = AppContext.BaseDirectory;
        string runtimePath = Path.Join(exePath, _rumtimePath);
        string nativesPath = Path.Join(exePath, _nativesPath);

        if (!Directory.Exists(runtimePath))
            throw new InvalidOperationException("Failed to find runtime folder");

        if (!Directory.Exists(nativesPath))
            throw new InvalidOperationException("Failed to find natives folder");

        StringBuilder runtimeAndUsesCombined = new StringBuilder();

        foreach (string file in Directory.GetFiles(runtimePath))
        {
            runtimeAndUsesCombined.AppendLine(File.ReadAllText(file));
        }

        foreach (var use in uses)
        {
            string usePath = Path.Join(nativesPath, use.Path);

            if (!File.Exists(usePath))
                throw new Error($"'{usePath}' does not exist", use.position);

            string contents = File.ReadAllText(usePath);

            var tree = CSharpSyntaxTree.ParseText(contents);

            var diagnostics = tree.GetDiagnostics().ToList();

            if (diagnostics.Count > 0)
            {
                string message = string.Join(
                    Environment.NewLine,
                    diagnostics.Select(x => x.ToString())
                );

                throw new Error(message, use.position);
            }

            runtimeAndUsesCombined.AppendLine(contents);
        }

        StringBuilder strippedResult = new StringBuilder();
        HashSet<string> usings = new HashSet<string>();
        HashSet<string> packages = new HashSet<string>();

        using StringReader stringReader = new StringReader(runtimeAndUsesCombined.ToString());

        string? line;

        while ((line = stringReader.ReadLine()) != null)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith("global using"))
            {
                usings.Add(trimmed["global ".Length..]);
            }
            else if (trimmed.StartsWith("using "))
            {
                usings.Add(trimmed);
            }
            else if (trimmed.StartsWith("// #:package "))
            {
                packages.Add(trimmed["// ".Length..]);
            }
            else
            {
                strippedResult.AppendLine(line);
            }
        }

        StringBuilder result = new StringBuilder();

        foreach (string package in packages)
            result.AppendLine(package);

        result.AppendLine();

        foreach (string @using in usings)
            result.AppendLine(@using);

        result.AppendLine();

        result.AppendLine(generated);

        result.AppendLine(strippedResult.ToString());

        return result.ToString();
    }
}