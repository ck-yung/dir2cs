using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static dir2.MyOptions;

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
    static public void DoNothing<T1,T2>(T1 _1, T2 _2) { }
    static public void DoNothing<T1, T2, T3>(T1 _1, T2 _2, T3 _3) { }

    static public T itself<T>(T arg) => arg;

    /// <summary>
    /// Always return false
    /// </summary>
    static public bool Never<T>(T _) { return false; }

    static public IEnumerable<string> CommonSplit(IEnumerable<string> args)
        => args
        .Select((it) => it.Split(';', ','))
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

        var defaultTimeSpan = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
        FromUtcToReportTimeZone = (arg) => arg.ToOffset(defaultTimeSpan);
    }

    /// <summary>
    /// From Utc time zone to Reporting time zone
    /// </summary>
    public static Func<DateTimeOffset, DateTimeOffset> FromUtcToReportTimeZone
    { get; private set; }
    public static string ReportTimeZone { get; private set; } = "";

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

    static public string ShortSyntax = $"""
        Syntax: dir2 -??
        Syntax: dir2 [OPTION ..] [DIR] [WILD ..]
        Frequently used options:
            --size-format     short | +short | WIDTH
                              e.g --size-format +short
                   --excl -x  EXCL-WILD[;EXCL-WILD ..]
                              e.g. -x *.tmp;*.temp
               --excl-dir -X  EXCL-WILD[;EXCL-WILD ..]
                   --sort -o  off | name | size | date | ext | count | last
                              e.g. -o size
                    --sum     ext | dir | +dir | year
                 --within -w  SIZE | DATE
                              e.g. -w 100m -w 30day
             --not-within -W  SIZE | DATE
                              e.g. -W 10k -W 3day
                              e.g. -w 14day -W +7day
         Frequently used shortcuts:
                          -R  => --dir tree
         Scan all sub dir -s  => --sub all
         Brief path name  -b  => --total off --hide date,size,count,mode,owner,link
         Dir only         -d  => --dir only
         File only        -f  => --dir off

        https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md

        """;

    static public string GetSyntax()
    {
        var rtn = new StringBuilder($"""
        Syntax: {ExeName} --version
        Syntax: {ExeName} [OPTION ..] [DIR] [WILD ..]
        OPTION:
        """);
        rtn.AppendLine();

        rtn.AppendLine($" {Program.CfgOffOpt,16}     [CFG INFO: {ExeName} -? cfg]");

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
        rtn.AppendLine($" {Wild.ExclNone,16}     clear all '--excl' and '--excl-dir'");
        rtn.AppendLine("SHORTCUT:");
        foreach (var kvThe in MyOptions.ShortcutComplexOptions
            .OrderBy((it) => it.Value.Item1))
        {
            var textThe = new StringBuilder($" {kvThe.Value.Item1,-16} {kvThe.Key}");
            var text2The = string.Join(" ", kvThe.Value.Item2);
            textThe.Append($"  => {text2The,-12}");
            rtn.AppendLine(textThe.ToString());
        }
        rtn.AppendLine("""

            https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md
            """);
        return rtn.ToString();
    }

    static internal Func<string, string> DirPrefixText { get; set; } = (msg) => msg;

    static internal Func<InfoDir, InfoSum> PrintDir { get; set; } = (dir) =>
    {
        var cntDir = dir.GetDirectories()
        .Where((it) => Wild.CheckIfDirNameMatched(it.Name))
        .Where((it) => (false == Wild.ExclDirNameOpt.Invoke(it.Name)))
        .Where((it) => Wild.IsMatchWithinDate(Show.GetDate(it)))
        .Where((it) => Wild.IsMatchNotWithinDate(Show.GetDate(it)))
        .Invoke(Sort.Dirs)
        .Invoke(Sort.ReverseDir)
        .Invoke(Sort.TakeDir)
        .Select((it) =>
        {
            ItemWrite(Show.Attributes(it));
            ItemWrite(Show.Owner(it));
            ItemWrite(DirPrefixText("DIR "));
            ItemWrite(Show.Date(DateFormatOpt.Invoke(Show.GetDate(it))));
            ItemWrite(Show.GetDirName(io.GetRelativeName(it.FullName)));
            ItemWrite(Show.Link.Invoke(it));
            ItemWriteLine(string.Empty);
            return it;
        })
        .Count();
        PrintDirCount(cntDir);
        return InfoSum.Fake;
    };

    static internal InfoSum GetFiles(InfoDir dir)
    {
        return dir.GetFiles()
            .Select((it) => ToInfoFile(it))
            .Where((it) => it.IsNotFake)
            .Where((it) => Wild.CheckIfFileNameMatched(it.Name))
            .Where((it) => (false == Wild.ExclFileNameOpt.Invoke(it.Name)))
            .Where((it) => Wild.IsMatchWithinSize(it.Length))
            .Where((it) => Wild.IsMatchWithinDate(Show.GetDate(it)))
            .Where((it) => Wild.IsMatchNotWithinSize(it.Length))
            .Where((it) => Wild.IsMatchNotWithinDate(Show.GetDate(it)))
            .Where((it) => Wild.ExtensionOpt.Invoke(it))
            .Where((it) => IsHiddenFileOpt.Invoke(it))
            .Where((it) => IsLinkFileOpt.Invoke(it))
            .Invoke(Sum.Reduce);
    }

    static internal Action<string> ItemWrite { get; set; } = Write;
    static internal Action<string> ItemWriteLine { get; set; } = WriteLine;

    static internal Action<string, string[]> DumpArgsAction { get; set; }
        = (path, wilds) =>
        {
            Program.MyDebugWrite("");
            Program.MyDebugWrite($"path='{path}'");
            Program.MyDebugWrite($"#wilds={wilds.Length}");
            foreach (var wild in wilds)
            {
                Program.MyDebugWrite($"  '{wild}'");
            }
        };

    static internal void PrintIntoTotalWithFlag(string path, string[] wilds,
        InfoSum sum, bool printEvenCountOne)
    {
        switch (sum.Count)
        {
            case 0:
                if (string.IsNullOrEmpty(path))
                {
                    if ((wilds.Length > 0) && !string.IsNullOrEmpty(wilds[0]))
                    {
                        Write($"No file is found for '{wilds[0]}'");
                    }
                    else
                    {
                        DumpArgsAction(path, wilds);
                    }
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        Write("No file is found");
                        if ((wilds.Length > 0) && !string.IsNullOrEmpty(wilds[0]))
                        {
                            Write($" for '{wilds[0]}'");
                        }
                        Write($" on '{path}'");
                        DumpArgsAction(path, wilds);
                    }
                    else
                    {
                        Write($"Dir '{path}' is not found");
                    }
                    Write(".");
                }
                WriteLine(Show.EndTime.Invoke(true));
                break;
            case 1:
                if (printEvenCountOne)
                {
                    Write("One file is found: ");
                    Write(Show.Size(Show.LengthFormatOpt.Invoke(sum.Length)));
                    Write(Show.Date(DateFormatOpt.Invoke(sum.StartTime)));
                    WriteLine(Show.EndTime.Invoke(true));
                }
                break;
            default:
                sum.Print(Write, WriteLine);
                break;
        }
    }

    static internal Action<string, string[], InfoSum> impPrintInfoTotal { get; set; }
        = (path, wilds, arg) => PrintIntoTotalWithFlag(
            path, wilds, arg, printEvenCountOne: false);

    static internal void PrintInfoTotal(string path, string[] wilds, InfoSum arg)
    {
        if (arg.IsNotFake) impPrintInfoTotal(path, wilds, arg);
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

    static public string ToKiloUnit(long arg)
    {
        if (arg <= 9_999)
        {
            return arg.ToString().PadLeft(4) + "  ";
        }
        if (arg <= 10_239_487) // 9999.499K
        {
            arg = Convert.ToInt64(arg / 1024.0);
            return arg.ToString().PadLeft(4) + "K ";
        }
        if (arg <= 10_485_235_711) // 9999.499999M
        {
            arg = Convert.ToInt64(arg / 1024.0 / 1024.0);
            return arg.ToString().PadLeft(4) + "M ";
        }
        if (arg <= 10_736_344_498_175) // 9998.999999999G
        {
            arg = Convert.ToInt64(arg / 1024.0 / 1024.0 / 1024.0);
            return arg.ToString().PadLeft(4) + "G ";
        }
        arg = Convert.ToInt64(arg / 1024.0 / 1024.0 / 1024.0 / 1024.0);
        return arg.ToString().PadLeft(4) + "T ";
    }

    static public string ToExtraShortKiloUnit(long arg)
    {
        if (arg <= 519)
        {
            return arg.ToString().PadLeft(4) + "  ";
        }
        if (arg <= 655_872) // 640K
        {
            arg = Convert.ToInt64(arg / 1024.0);
            return arg.ToString().PadLeft(4) + "K ";
        }
        if (arg <= 839_385_089) // 800M
        {
            arg = Convert.ToInt64(arg / 1024.0 / 1024.0);
            return arg.ToString().PadLeft(4) + "M ";
        }
        if (arg <= 10_736_344_498_175) // 9998.999999999G
        {
            arg = Convert.ToInt64(arg / 1024.0 / 1024.0 / 1024.0);
            return arg.ToString().PadLeft(4) + "G ";
        }
        arg = Convert.ToInt64(arg / 1024.0 / 1024.0 / 1024.0 / 1024.0);
        return arg.ToString().PadLeft(4) + "T ";
    }

    static public bool TryParseKiloNumber(string arg, out long rtn)
    {
        rtn = 0;
        var toLong = new Dictionary<string, Func<string, long>>
        {
            ["k"] = (it) => long.Parse(it) * 1024,
            ["m"] = (it) => long.Parse(it) * 1024 * 1024,
            ["g"] = (it) => long.Parse(it) * 1024 * 1024 * 1024,
            ["t"] = (it) => long.Parse(it) * 1024 * 1024 * 1024 * 1024,
            [""] = (it) => long.Parse(it),
        };
        var regs = new Regex[]
        {
            RegexSizeWithUnitB(),
            RegexSizeWithUnit(),
            RegexSizeB(),
            RegexSize(),
        };
        foreach (var regThe in regs)
        {
            var rslt = regThe.Match(arg);
            if (true != rslt.Success) continue;
            var valueFound = rslt.Groups["valueFound"].Value;
            var unitFound = rslt.Groups["unitFound"].Value;
            if (toLong.TryGetValue(unitFound.ToLower(), out var parseThe))
            {
                rtn = parseThe(valueFound);
                return true;
            }
            return false;
        }
        return false;
    }

    [GeneratedRegex(@"^(?<valueFound>\d{1,})(?<unitFound>[kmgt])b$",
        RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RegexSizeWithUnitB();

    [GeneratedRegex(@"^(?<valueFound>\d{1,})(?<unitFound>[kmgt])$",
        RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RegexSizeWithUnit();

    [GeneratedRegex(@"^(?<valueFound>\d{1,})b$",
        RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RegexSizeB();

    [GeneratedRegex(@"^(?<valueFound>\d{1,})$",
        RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RegexSize();
}
