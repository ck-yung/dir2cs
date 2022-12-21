using System.Net.Http.Headers;
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

    static internal InfoSum PrintDir(string path)
    {
        var cntDir = GetDirs(path)
            .Select((it) => System.ToInfoDir(it))
            .Invoke(Sort.Dirs)
            .Select((it) =>
            {
                Write("DIR ");
                Write(it.LastWriteTime.ToString("u"));
                Write(" ");
                WriteLine(it.Name);
                return it;
            })
            .Count();
        PrintDirCount(cntDir);
        return InfoSum.Fake;
    }

    static internal InfoSum PrintFile(string path)
    {
        return GetFiles(path)
            .Select((it) => System.ToInfoFile(it))
            .Invoke(Sort.Files)
            .Select((it) =>
            {
                Write($"{MyOptions.LengthFormat.Invoke(it.Length)} ");
                Write($"{MyOptions.DateFormat.Invoke(it.LastWriteTime)} ");
                WriteLine(it.Name);
                return it;
            })
            .Aggregate(seed: new InfoSum(Helper.GetLastDir(path)),
            func: (acc, it) => acc.Add(it));
    }

    static internal Action<int> impPrintDirCount { get; set; } = (cntDir) =>
    {
        if (cntDir>1) WriteLine($"{cntDir} dir are found.");
        WriteLine("");
    };

    static internal void PrintDirCount(int count)
    {
        impPrintDirCount(count);
    }

    static internal Action<InfoSum> impPrintSum { get; set; } = (arg) =>
    {
        switch (arg.Count)
        {
            case 0:
                WriteLine("No file is found.");
                break;
            case 1:
                break;
            default:
                Write($"{MyOptions.LengthFormat.Invoke(arg.Length)} ");
                Write($"{MyOptions.DateFormat.Invoke(arg.StartTime)} ");
                Write($"{MyOptions.DateFormat.Invoke(arg.EndTime)} ");
                Write($"{arg.Count,4} ");
                WriteLine(arg.Name);
                break;
        }
    };

    static internal void PrintInfoSum(InfoSum arg)
    {
        impPrintSum(arg);
    }

    static internal string GetLastDir(string arg)
    {
        return arg
            .TrimEnd(Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar)
            .AsEnumerable()
            .Last();
    }
}
