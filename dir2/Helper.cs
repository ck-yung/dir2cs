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

    static public R Invoke<T, R>(this IEnumerable<T> seq,
        Func<IEnumerable<T>, R> func)
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
                rtn.Append($"  {parser.Name,28}");
            }
            else
            {
                rtn.Append($"  {parser.Name,28}   {parser.Help}");
            }
            rtn.AppendLine();
        }
        return rtn.ToString();
    }

    static internal Func<string, InfoSum> PrintDir { get; set; } = (path) =>
    {
        var cntDir = GetDirs(path)
            .Select((it) => System.ToInfoDir(it))
            .Invoke(Sort.Dirs)
            .Select((it) =>
            {
                ItemWrite("DIR ");
                ItemWrite(Show.Date($"{MyOptions.DateFormat.Invoke(it.LastWriteTime)} "));
                ItemWriteLine(it.Name);
                return it;
            })
            .Count();
        PrintDirCount(cntDir);
        return InfoSum.Fake;
    };

    static internal InfoSum PrintFile(string path)
    {
        return GetFiles(path)
            .Select((it) => System.ToInfoFile(it))
            .Where((it) => it.IsNotFake())
            .Invoke(Sort.Files)
            .Invoke((seq) => Sum.Func(seq, path));
    }

    static internal Action<string> ItemWrite { get; set; } = Write;
    static internal Action<string> ItemWriteLine { get; set; } = WriteLine;

    static internal Action<int> impPrintDirCount { get; set; } = (cntDir) =>
    {
        if (cntDir>1) WriteLine($"{cntDir} dir are found.");
        WriteLine("");
    };

    static internal void PrintDirCount(int count)
    {
        impPrintDirCount(count);
    }

    static internal void PrintIntoTotalWithFlag(InfoSum sum, bool printEvenCountOne)
    {
        switch (sum.Count)
        {
            case 0:
                WriteLine("No file is found.");
                break;
            case 1:
                if (printEvenCountOne)
                {
                    Write("One file is found: ");
                    Write(Show.Size($"{MyOptions.LengthFormat.Invoke(sum.Length)} "));
                    WriteLine(Show.Date($"{MyOptions.DateFormat.Invoke(sum.StartTime)}"));
                }
                break;
            default:
                sum.Print(Write, WriteLine);
                break;
        }
    }

    static internal Action<InfoSum> impPrintInfoTotal { get; set; }
        = (arg) => PrintIntoTotalWithFlag(arg, printEvenCountOne: false);

    static internal void PrintInfoTotal(InfoSum arg)
    {
        impPrintInfoTotal(arg);
    }

    static internal string GetFirstDir(string path)
    {
        var rtn = path.Split(Path.DirectorySeparatorChar)
            .Take(2).FirstOrDefault();
        return string.IsNullOrEmpty(rtn) ? "." : rtn;
    }

    static internal string GetLastDir(string path)
    {
        return path
            .TrimEnd(Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar)
            .AsEnumerable()
            .Last();
    }
}
