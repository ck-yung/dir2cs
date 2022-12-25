using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace dir2;

public class Always<T>
{
    static public readonly Func<T, bool> True = (_) => true;
}

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

    static public void DoNothing<T>(T _) { }

    static public T itself<T>(T arg) => arg;

    /// <summary>
    /// Always return false
    /// </summary>
    static public bool Never<T>(T _) { return false; }

    static public IEnumerable<string> CommonSplit(IEnumerable<string> args) => args
        .Select((it) => it.Split(';', ':', ','))
        .SelectMany((it) => it)
        .Where((it) => it.Length > 0)
        .Distinct();

    static internal readonly string ExeName;
    static internal readonly string ExeVersion;
    static internal readonly string ExeCopyRight;

    static Helper()
    {
        var asm = Assembly.GetExecutingAssembly();
        var asmName = asm.GetName();
        ExeName = asmName.Name ?? "?";
        ExeVersion = asmName.Version?.ToString() ?? "?";
        var aa = asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute),
            inherit: false);
        if (aa.Length > 0)
        {
            ExeCopyRight = ((AssemblyCopyrightAttribute)aa[0]).Copyright;
        }
        else
        {
            ExeCopyRight = "?";
        }
    }

    static public string GetExeEnvr()
    {
        return Environment.GetEnvironmentVariable(ExeName) ?? string.Empty;
    }

    static public string GetVersion() =>
        $"{ExeName} v{ExeVersion} {ExeCopyRight}";

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

        foreach (var optThe in
        from parser in MyOptions.Parsers
        join shortcut in MyOptions.ShortcutOptions
        on parser.Name equals shortcut.Value into gj
        from found in gj.DefaultIfEmpty()
        select new
        {
            parser.Name, parser.Help,
            Shortcut = string.IsNullOrEmpty(found.Key) ? "  " : found.Key
        })
        {
            rtn.AppendLine($" {optThe.Name,16} {optThe.Shortcut}  {optThe.Help}");
        }
        rtn.AppendLine();
        foreach (var kvThe in MyOptions.ShortcutComplexOptions
            .OrderBy((it) => it.Key))
        {
            var textThe = new StringBuilder($" {kvThe.Value.Item1,-16} {kvThe.Key}");
            var text2The = string.Join(" ", kvThe.Value.Item2);
            textThe.Append($"  => {text2The,-12}");
            rtn.AppendLine(textThe.ToString());
        }
        return rtn.ToString();
    }

    static internal Func<string, InfoSum> PrintDir { get; set; } = (path) =>
    {
        var cntDir = ImpGetDirs(path)
            .Select((it) => io.ToInfoDir(it))
            .Where((it) => it.IsNotFake())
            .Where((it) => Wild.CheckIfDirNameMatched(it.Name))
            .Where((it) => (false == Wild.ExcludeDirName.Invoke(it.Name)))
            .Where((it) => Wild.IsMatchWithinDate(Show.GetDate(it)))
            .Where((it) => Wild.IsMatchNotWithinDate(Show.GetDate(it)))
            .Invoke(Sort.Dirs)
            .Invoke(Show.ReverseDir)
            .Invoke(Show.TakeDir)
            .Select((it) =>
            {
                ItemWrite(Show.Size("DIR "));
                ItemWrite(Show.Date($"{MyOptions.DateFormat.Invoke(Show.GetDate(it))} "));
                ItemWriteLine(Show.GetDirName(io.GetRelativeName(it.FullName)));
                return it;
            })
            .Count();
        PrintDirCount(cntDir);
        return InfoSum.Fake;
    };

    static internal InfoSum GetFiles(string path)
    {
        return ImpGetFiles(path)
            .Select((it) => io.ToInfoFile(it))
            .Where((it) => it.IsNotFake())
            .Where((it) => Wild.CheckIfFileNameMatched(it.Name))
            .Where((it) => (false == Wild.ExcludeFileName.Invoke(it.Name)))
            .Where((it) => Wild.IsMatchWithinSize(it.Length))
            .Where((it) => Wild.IsMatchWithinDate(Show.GetDate(it)))
            .Where((it) => Wild.IsMatchNotWithinSize(it.Length))
            .Where((it) => Wild.IsMatchNotWithinDate(Show.GetDate(it)))
            .Invoke((seq) => Sum.Func(seq));
    }

    static internal Action<string> ItemWrite { get; set; } = Write;
    static internal Action<string> ItemWriteLine { get; set; } = WriteLine;

    static internal Action<int> impPrintDirCount { get; set; } = (cntDir) =>
    {
        if (cntDir > 1) WriteLine($"{cntDir} dir are found.");
        if (cntDir > 0) WriteLine("");
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
        if (arg.IsNotFake()) impPrintInfoTotal(arg);
    }

    static internal string GetFirstDir(string path)
    {
        var rtn = path.Split(Path.DirectorySeparatorChar)
            .Take(2).FirstOrDefault();
        return string.IsNullOrEmpty(rtn) ? "." : rtn;
    }
    static public bool TryParseKiloNumber(string arg, out long result)
    {
        long unitValue(char unitThe)
        {
            return unitThe switch
            {
                'k' => 1024,
                'm' => 1024 * 1024,
                _ => 1024 * 1024 * 1024,// g
            };
        }

        if (long.TryParse(arg, out result))
        {
            return true;
        }

        if (Regex.Match(arg, @"^\d+[kmg]$").Success)
        {
            if (long.TryParse(arg.AsSpan(0, arg.Length - 1),
                out result))
            {
                if (result > 0)
                {
                    result *= unitValue(arg[^1]);
                    return true;
                }
            }
        }
        return false;
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
