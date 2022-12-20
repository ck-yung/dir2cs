using System.Reflection;
using System.Text;

namespace dir2;

public static partial class Helper
{
    static internal readonly string ExeName;
    static internal readonly string ExeVersion;
    static Helper()
    {
        var asm = Assembly.GetExecutingAssembly().GetName();
        ExeName = asm.Name ?? "?";
        ExeVersion = asm.Version?.ToString() ?? "?";
    }

    public static string GetVersion() => $"{ExeName} v{ExeVersion}";

    public static string GetHelpSyntax() => $"""
        Get help by
          {ExeName} -?
        """;

    public static string GetSyntax()
    {
        var rtn = new StringBuilder($"""
        Syntax: {ExeName} --version
        Syntax: {ExeName} [OPTION ..] [DIRNAME]
        OPTION:
        """);
        rtn.AppendLine();
        foreach (var parser in MyOptions.Parsers)
        {
            if (string.IsNullOrEmpty(parser.Name))
            {
                rtn.Append($"  {parser.Name}");
            }
            else
            {
                rtn.Append($"  {parser.Name}   {parser.Help}");
            }
            rtn.AppendLine();
        }
        return rtn.ToString();
    }

    static internal int PrintDir(string path)
    {
        var cntDir = Helper.GetDirs(path)
            .Select((it) => new DirectoryInfo(it))
            .Select((it) =>
            {
                Console.Write("DIR ");
                Console.Write(it.LastWriteTime.ToString("u"));
                Console.Write(" ");
                Console.WriteLine(it.Name);
                return it;
            })
            .Count();
        if (cntDir > 1)
        {
            Console.WriteLine($"{cntDir} directories are found.");
            Console.WriteLine();
        }
        return 0;
    }

    static internal int PrintFile(string path)
    {
        var cntFile = Helper.GetFiles(path)
            .Select((it) => new FileInfo(it))
            .Select((it) =>
            {
                Console.Write($"{it.Length,8} ");
                Console.Write(it.LastWriteTime.ToString("u"));
                Console.Write(" ");
                Console.WriteLine(it.Name);
                return it;
            })
            .Count();
        if (cntFile > 1)
        {
            Console.WriteLine($"{cntFile} files are found.");
        }
        return cntFile;
    }
}
