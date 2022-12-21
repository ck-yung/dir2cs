using System.Reflection;
using System.Text;

namespace dir2;

static public partial class Helper
{
    static public IEnumerable<T> Invoke<T>(this IEnumerable<T> seq,
        Func<IEnumerable<T>, IEnumerable<T>> func)
    {
        return func(seq);
    }

    static internal readonly string ExeName;
    static internal readonly string ExeVersion;

    static Helper()
    {
        var asm = Assembly.GetExecutingAssembly().GetName();
        ExeName = asm.Name ?? "?";
        ExeVersion = asm.Version?.ToString() ?? "?";
    }

    static public string GetExeEnvr()
    {
        return Environment.GetEnvironmentVariable(ExeName) ?? string.Empty;
    }

    static public string GetVersion() => $"{ExeName} v{ExeVersion}";

    static public string GetHelpSyntax() => $"""
        Get help by
          {ExeName} -?
        """;

    static public string GetSyntax()
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
        var cntDir = GetDirs(path)
            .Select((it) => System.ToInfoDir(it))
            .Invoke(MyOptions.SortDirs)
            .Select((it) =>
            {
                Write("DIR ");
                Write(it.LastWriteTime.ToString("u"));
                Write(" ");
                WriteLine(it.Name);
                return it;
            })
            .Count();
        if (cntDir > 1)
        {
            WriteLine($"{cntDir} directories are found.");
            WriteLine("");
        }
        return 0;
    }

    static internal int PrintFile(string path)
    {
        var cntFile = GetFiles(path)
            .Select((it) => System.ToInfoFile(it))
            .Invoke(MyOptions.SortFiles)
            .Select((it) =>
            {
                Write($"{it.Length,8} ");
                Write(it.LastWriteTime.ToString("u"));
                Write(" ");
                WriteLine(it.Name);
                return it;
            })
            .Count();
        if (cntFile > 1)
        {
            WriteLine($"{cntFile} files are found.");
        }
        return cntFile;
    }
}
