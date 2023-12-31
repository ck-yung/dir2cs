using System.Collections.Immutable;
using System.Globalization;
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

    static internal Func<string, InfoSum> PrintDir { get; set; }
        = (path) =>
        {
            var cntDir = ImpGetDirs(path)
            .Select((it) => ToInfoDir(it))
            .Where((it) => false==it.IsFake())
            .Where((it) => CheckDirLink(it))
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
                ItemWrite(Show.Date($"{DateFormatOpt.Invoke(Show.GetDate(it))} "));
                ItemWrite(Show.GetDirName(io.GetRelativeName(it.FullName)));
                ItemWrite(Show.Link.Invoke(it));
                ItemWriteLine(string.Empty);
                return it;
            })
            .Count();
        PrintDirCount(cntDir);
        return InfoSum.Fake;
    };

    static internal InfoSum GetFiles(string path)
    {
        return ImpGetFiles(path)
            .Select((it) => ToInfoFile(it))
            .Where((it) => it.IsNotFake())
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

    static internal Action<int> impPrintDirCount { get; set; } = (cntDir) =>
    {
        if (cntDir > 1) WriteLine($"{cntDir} dir are found.");
        if (cntDir > 0) WriteLine("");
    };

    static internal void PrintDirCount(int count)
    {
        impPrintDirCount(count);
    }

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
                    WriteLine(".");
                }
                break;
            case 1:
                if (printEvenCountOne)
                {
                    Write("One file is found: ");
                    Write(Show.Size(Show.LengthFormatOpt.Invoke(sum.Length)));
                    WriteLine(Show.Date(DateFormatOpt.Invoke(sum.StartTime)));
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
        if (arg.IsNotFake()) impPrintInfoTotal(path, wilds, arg);
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

    record DateParse(string pattern, Func<int, TimeSpan> toTimeSpan);

    static public string DefaultDateTimeFormatString
    { get; } = "yyyy-MM-dd HH:mm";

    static public readonly IInovke<DateTime, string> DateFormatOpt =
        new ParseInvoker<DateTime, string>(name: "--date-format",
            help: "DATE-FORMAT   e.g. short, unix, OR yy-MM-dd%20HH:mm:ss",
            init: (value) => value.ToString(DefaultDateTimeFormatString),
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
                if (aa.Length > 1)
                    throw new ArgumentException($"Too many values to {parser.Name}");
                var formatThe = aa[0];
                Func<DateTime, string> rtn = (_) => string.Empty;
                switch (formatThe)
                {
                    case "unix":
                        rtn = (value) =>
                        {
                            var dto = new DateTimeOffset(value.ToUniversalTime());
                            var unixTimestamp = dto.ToUnixTimeSeconds();
                            return $"{unixTimestamp,11}";
                        };
                        break;
                    case "short":
                        // length of key is 4 or 5
                        const string keyToday = "today";
                        const string keyYsDay = "ysday";
                        const string keyWeek = "week";
                        const string keyYear = "year";
                        const string keyElse = "else";

                        Func<string> fnJust = () => "Just".PadRight(8);
                        var formatMap = new Dictionary<string, Func<DateTime, string>>()
                        {
                            [keyToday] = (it) =>
                            it.ToString("hh:mmtt", CultureInfo.InvariantCulture).PadRight(8),

                            [keyYsDay] = (it) =>
                            it.ToString("\"Yd\" hhtt", CultureInfo.InvariantCulture).PadRight(8),

                            [keyWeek] = (it) =>
                            it.ToString("ddd hhtt", CultureInfo.InvariantCulture).PadRight(8),

                            [keyYear] = (it) =>
                            it.ToString("MMM dd", CultureInfo.InvariantCulture).PadRight(8),

                            [keyElse] = (it) =>
                            it.ToString("yyyy MMM", CultureInfo.InvariantCulture).PadRight(8),
                        };
                        var cfgFilename = Config.GetFilename()[..^4]
                        + ".date-short.opt";

                        if (File.Exists(cfgFilename))
                        {
                            var cultureThe = CultureInfo.InvariantCulture;
                            var regCulture = new Regex(
                                @"^culture=(?<Culture>\w.*)$",
                                RegexOptions.IgnoreCase);
                            var regFormat = new Regex(
                                @"^(?<Name>\w{4,5})(\s=\s|\s=|=\s|=)(?<Format>.*)$",
                                RegexOptions.IgnoreCase);

                            var buf2 = new byte[2048];
                            int readCnt = 0;
                            using (var inpFp = File.OpenRead(cfgFilename))
                            {
                                readCnt = inpFp.Read(buf2);
                            }
                            foreach (var line in Encoding.UTF8.GetString(buf2,0,readCnt)
                            .Split('\n','\r')
                            .Select((it) => it.Trim())
                            .Where((it) => it.Length > 0)
                            )
                            {
                                var check = regCulture.Match(line);
                                if (check.Success)
                                {
                                    var cultureFound = check.Groups["Culture"].Value;
                                    try
                                    {
                                        cultureThe = CultureInfo
                                        .CreateSpecificCulture(cultureFound);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.Error.WriteLine(
                                            $"Loading '{cfgFilename}', key=culture, value=[{cultureFound}]");
                                        Console.Error.WriteLine($"{ex.GetType()}: {ex.Message}");
                                    }
                                    continue;
                                } // regCulture.Match().Success

                                check = regFormat.Match(line);
                                if (!check.Success) continue;
                                var nameThe = check.Groups["Name"].Value.ToLower();
                                var dateFmtThe = check.Groups["Format"].Value?.Trim();
                                if (string.IsNullOrEmpty(dateFmtThe)) continue;
                                var dateFmtThe2 = System.Net.WebUtility.UrlDecode(dateFmtThe);
                                if (formatMap.ContainsKey(nameThe))
                                {

                                    Func<DateTime, string> decodingFormatThe =
                                    (it) => it.ToString(dateFmtThe2, cultureThe);
                                    try
                                    {
                                        _ = decodingFormatThe(DateTime.MinValue);
                                        formatMap[nameThe] = decodingFormatThe;
                                    }
                                    catch (Exception ee2)
                                    {
                                        Console.Error.WriteLine(
                                            $"Loading '{cfgFilename}', key='{nameThe}', format='{dateFmtThe}'");
                                        Console.Error.WriteLine($"\t{ee2.GetType()}: {ee2.Message}");
                                    }
                                }
                                else if (string.Compare(nameThe, "Just", ignoreCase: true) == 0)
                                {
                                    fnJust = () => dateFmtThe2;
                                }
                            }
                        }

                        var now = DateTime.Now;
                        var withinTwoMinutes = now.AddMinutes(-2);
                        var todayMidnight = new DateTime(now.Year, now.Month, now.Day);
                        var yesterday = todayMidnight.AddDays(-1);
                        var fnToday = formatMap[keyToday];
                        var fnYsDay = formatMap[keyYsDay];
                        var fnWeek = formatMap[keyWeek];
                        var fnYear = formatMap[keyYear];
                        var fnElse = formatMap[keyElse];
                        Func<DateTime, bool> checkDayOfWeek = (timeCheck) =>
                        {
                            return (now - timeCheck) < TimeSpan.FromDays(6)
                                && timeCheck.DayOfWeek < now.DayOfWeek;
                        };
                        var envirDir2 = Environment.GetEnvironmentVariable(nameof(dir2));
                        if (envirDir2?.Contains("--debug:CheckDayOfWeekOff") ?? false)
                        {
                            checkDayOfWeek = (timeCheck) =>
                            {
                                return (now - timeCheck) < TimeSpan.FromDays(6);
                            };
                        }
                        rtn = (timeThe) =>
                        {
                            if (timeThe > withinTwoMinutes) return fnJust();

                            if (timeThe > todayMidnight) return fnToday(timeThe);

                            if (timeThe > yesterday) return fnYsDay(timeThe);

                            if ((now.Year == timeThe.Year) && (now >= timeThe))
                            {
                                if (checkDayOfWeek(timeThe)) return fnWeek(timeThe);
                                return fnYear(timeThe);
                            }
                            return fnElse(timeThe);
                        };
                        break;
                    default:
                        var fmtThe = System.Net.WebUtility.UrlDecode(formatThe);
                        rtn = (value) => value.ToString(fmtThe);
                        break;
                };
                _ = rtn(DateTime.MinValue); // verify if the lambda is valid
                parser.SetImplementation(rtn);
            });

    static public readonly ImmutableArray<string> DateTimeFormats =
        ImmutableArray.Create(new string[] {
            "yyyy-MM-dd",
            "yyyyMMdd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd hh:mmtt",
            "yyyyMMdd HH:mm:ss",
            "yyyyMMdd HH:mm",
            "HH:mm:ss",
            "HH:mm",
            "hh:mmtt",
        });

    static public bool TryParseDateTime(string arg, out DateTime result)
    {
        result = DateTime.MinValue;
        var pattern3 = new Dictionary<string, DateParse>()
        {
            ["minute"] = new DateParse(@"^(?<minute>\d+)min$", (it) => TimeSpan.FromMinutes(it)),
            ["minutes"] = new DateParse(@"^(?<minutes>\d+)minutes$", (it) => TimeSpan.FromMinutes(it)),
            ["hour"] = new DateParse(@"^(?<hour>\d+)hour$", (it) => TimeSpan.FromHours(it)),
            ["hours"] = new DateParse(@"^(?<hours>\d+)hours$", (it) => TimeSpan.FromHours(it)),
            ["hr"] = new DateParse(@"^(?<hr>\d+)hr$", (it) => TimeSpan.FromHours(it)),
            ["day"] = new DateParse(@"^(?<day>\d+)day$", (it) => TimeSpan.FromDays(it)),
            ["days"] = new DateParse(@"^(?<days>\d+)days$", (it) => TimeSpan.FromDays(it)),
            ["year"] = new DateParse(@"^(?<year>\d+)year$", (it) => TimeSpan.FromDays(365 * it)),
            ["years"] = new DateParse(@"^(?<years>\d+)years$", (it) => TimeSpan.FromDays(365 * it)),
        };

        foreach (var (keyThe, parseThe) in pattern3)
        {
            foreach (Match match in Regex.Matches(arg, parseThe.pattern,
                RegexOptions.IgnoreCase))
            {
                var numThe = int.Parse(match.Groups[keyThe].ToString());
                result = DateTime.Now.Subtract(parseThe.toTimeSpan(numThe));
                return true;
            }
        }

        if (DateTime.TryParseExact(arg, DefaultDateTimeFormatString,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime goodValue))
        {
            result = goodValue;
            return true;
        }

        foreach (var fmtThe in DateTimeFormats)
        {
            if (DateTime.TryParseExact(arg, fmtThe,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime goodValue2))
            {
                result = goodValue2;
                return true;
            }
        }

        return false;
    }

    static public string ToKiloUnit(long arg)
    {
        var units = new char[] { 'T', 'G', 'M', 'K', ' ' };
        string toKilo(float arg2, int index)
        {
            if (arg2 < 10_000.0F) return $"{arg2,4:F0}{units[index - 1]} ";
            if (index == 1) return $"{arg2,4:F0}{units[0]} ";
            return toKilo((arg2 + 512) / 1024.0F, index - 1);
        }
        return toKilo(arg, units.Length);
    }

    static public string ToExtraShortKiloUnit(long arg)
    {
        var units = new (char UnitThe, float MaxThe)[]
        {
            ('T', 10_000.0F),
            ('G', 10_000.0F),
            ('M', 800.0F),
            ('K', 640.0F),
            (' ', 520.0F),
        };
        string toKilo(float arg2, int index)
        {
            if (arg2 < units[index - 1].MaxThe)
                return $"{arg2,4:F0}{units[index - 1].UnitThe} ";
            if (index == 1) return $"{arg2,4:F0}{units[0].UnitThe} ";
            return toKilo((arg2 + 512) / 1024.0F, index - 1);
        }
        return toKilo(arg, units.Length);
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
            new Regex(@"^(?<valueFound>\d{1,})(?<unitFound>[kmgt])b$",
            RegexOptions.IgnoreCase),
            new Regex(@"^(?<valueFound>\d{1,})(?<unitFound>[kmgt])$",
            RegexOptions.IgnoreCase),
            new Regex(@"^(?<valueFound>\d{1,})b$",
            RegexOptions.IgnoreCase),
            new Regex(@"^(?<valueFound>\d{1,})$",
            RegexOptions.IgnoreCase),
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
}
