﻿using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static dir2.MyOptions;
using static dir2.SummaryInfo;

namespace dir2;

public class Always<T>
{
    static public readonly Func<T, bool> True = (_) => true;
}

static public partial class Helper
{
    static public readonly Func<String, bool> AnyText = (_) => true;

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
    static public void DoNothing<T1, T2>(T1 _1, T2 _2) { }
    static public void DoNothing<T1, T2, T3>(T1 _1, T2 _2, T3 _3) { }

    static public string GetEmptyString() => string.Empty;
    static public string ReturnEmptyString(string _) => string.Empty;

    static public T itself<T>(T arg) => arg;

    static public string AppendSpace(string arg) => arg + " ";

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

    static public readonly string ShortSyntax = $"""
        Syntax: dir2 -??
        Syntax: dir2 [OPTION ..] [DIR] [WILD ..]
        Frequently used options:
            --size-format     short | +short | WIDTH
                              e.g --size-format +short
                   --excl -x  EXCL-WILD[,EXCL-WILD ..]
                              e.g. -x *.tmp;*.temp
               --excl-dir -X  EXCL-WILD[,EXCL-WILD ..]
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

    public record OptionHelp(string Name, string Help, string Shortcut);

    static public IEnumerable<OptionHelp> GetOptionHelps() =>
        from parser in MyOptions.Parsers
        join shortcut in MyOptions.ShortcutOptions
        on parser.Name equals shortcut.Value into gj
        from found in gj.DefaultIfEmpty()
        select new OptionHelp(
            parser.Name,
            parser.Help,
            Shortcut: string.IsNullOrEmpty(found.Key) ? "  " : found.Key);

    static public string GetSyntax()
    {
        var rtn = new StringBuilder($"""
        Syntax: {ExeName} --version
        Syntax: {ExeName} [OPTION ..] [DIR] [WILD ..]
        HELP:
          --dir2 -?
          --dir2 -? cfg
          --dir2 -? -
          --dir2 -? +
        OPTION:
        """);
        rtn.AppendLine();

        rtn.AppendLine($" {Program.CfgOffOpt,16}     [CFG INFO: {ExeName} -? cfg]");

        foreach (var optThe in GetOptionHelps())
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
        foreach (var kvThe in MyOptions.ShortcutExpandOptions
            .OrderBy ((it) => it.Value.Item1))
        {
            rtn.AppendLine($" {kvThe.Value.Item1,-16} {kvThe.Key}     {kvThe.Value.Item3}");
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
        .Zip(Show.ColorOpt.Invoke(3))
        .Select((itm) =>
        {
            itm.Second.Invoke();
            var it = itm.First;
            ItemWrite(Show.Color.SwitchFore(Show.Attributes(it)));
            ItemWrite(Show.Color.SwitchFore(Show.Owner(it)));
            ItemWrite(Show.Color.SwitchFore(DirPrefixText("DIR ")));
            ItemWrite(Show.Color.SwitchFore(Show.Date(DateFormatOpt.Invoke(Show.GetDate(it)))));
            ItemWrite(Show.Color.SwitchFore(Show.GetDirName(Io.GetRelativeName(it.FullName))));
            ItemWrite(Show.Color.SwitchFore(Show.Link.Invoke(it)));
            ItemWriteLine(Show.Color.TotallyResetFore(""));
            return it;
        })
        .Count();
        PrintDirCount(cntDir);
        Show.Color.Reset();
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

    static internal Func<string, string> ItemWrite
    { get; set; } = Write;
    static internal Func<string, string> ItemWriteLine
    { get; set; } = WriteLine;

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
        if (sum.IsFake) return;
        if ((sum.AddCount == 1) && (false == printEvenCountOne)) return;
        var txt = new StringBuilder();
        switch (sum.Count)
        {
            case 0:
                if (string.IsNullOrEmpty(path))
                {
                    if ((wilds.Length > 0) && !string.IsNullOrEmpty(wilds[0]))
                    {
                        txt.Append(Format(StringFormat.FileZeroWithWild, wilds[0]));
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
                        if ((wilds.Length > 0) && !string.IsNullOrEmpty(wilds[0]))
                        {
                            txt.Append(NoFileWithWildOnDir(wilds[0], path));
                        }
                        else
                        {
                            txt.Append(Format(StringFormat.FileZeroOnDir, path));
                        }
                        DumpArgsAction(path, wilds);
                    }
                    else
                    {
                        txt.Append(Format(StringFormat.DirNotFound, path));
                    }
                }
                txt.Append(Show.EndTime.Invoke(true));
                break;

            case 1:
                if (printEvenCountOne)
                {
                    txt.Append(Text(FixedText.OneFile));
                    txt.Append(Show.Size(Show.LengthFormatOpt.Invoke(sum.Length)));
                    txt.Append(Show.Date(DateFormatOpt.Invoke(sum.StartTime)));
                    txt.Append(Show.EndTime.Invoke(true));
                }
                break;
            default:
                txt.Append(sum.ToString());
                break;
        }
        WriteTotalLine(txt.ToString());
    }

    static internal Action<string, string[], InfoSum> impPrintInfoTotal { get; set; }
        = (path, wilds, arg) => PrintIntoTotalWithFlag(
            path, wilds, arg, printEvenCountOne: false);

    static internal void PrintInfoTotal(string path, string[] wilds, InfoSum arg)
    {
        impPrintInfoTotal(path, wilds, arg);
    }

    static internal string GetFirstDir(string path)
    {
        var rtn = path.Split(Path.DirectorySeparatorChar)
            .Take(2).FirstOrDefault();
        return string.IsNullOrEmpty(rtn) ? "." : rtn;
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
        RegexOptions.IgnoreCase)]
    private static partial Regex RegexSizeWithUnitB();

    [GeneratedRegex(@"^(?<valueFound>\d{1,})(?<unitFound>[kmgt])$",
        RegexOptions.IgnoreCase)]
    private static partial Regex RegexSizeWithUnit();

    [GeneratedRegex(@"^(?<valueFound>\d{1,})b$",
        RegexOptions.IgnoreCase)]
    private static partial Regex RegexSizeB();

    [GeneratedRegex(@"^(?<valueFound>\d{1,})$",
        RegexOptions.IgnoreCase)]
    private static partial Regex RegexSize();
}
