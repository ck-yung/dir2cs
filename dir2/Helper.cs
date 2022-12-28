using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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

    static public T itself<T>(T arg) => arg;

    /// <summary>
    /// Always return false
    /// </summary>
    static public bool Never<T>(T _) { return false; }

    static public IEnumerable<string> CommonSplit(IEnumerable<string> args)
        => args
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
        Syntax: {ExeName} [OPTION ..] WILD [WILD ..]
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
        rtn.AppendLine();
        foreach (var kvThe in MyOptions.ShortcutComplexOptions
            .OrderBy((it) => it.Key))
        {
            var textThe = new StringBuilder($" {kvThe.Value.Item1,-16} {kvThe.Key}");
            var text2The = string.Join(" ", kvThe.Value.Item2);
            textThe.Append($"  => {text2The,-12}");
            rtn.AppendLine(textThe.ToString());
        }
        rtn.AppendLine($"Envir '{nameof(dir2)}' will be parsed before config file is involved.");
        return rtn.ToString();
    }

    static internal Func<string, InfoSum> PrintDir { get; set; } = (path) =>
    {
        var cntDir = ImpGetDirs(path)
            .Select((it) => io.ToInfoDir(it))
            .Where((it) => it.IsNotFake())
            .Where((it) => Wild.CheckIfDirNameMatched(it.Name))
            .Where((it) => (false == Wild.ExclDirNameOpt.Invoke(it.Name)))
            .Where((it) => Wild.IsMatchWithinDate(Show.GetDate(it)))
            .Where((it) => Wild.IsMatchNotWithinDate(Show.GetDate(it)))
            .Invoke(Sort.Dirs)
            .Invoke(Show.ReverseDir)
            .Invoke(Show.TakeDir)
            .Select((it) =>
            {
                ItemWrite(Show.Size("DIR "));
                ItemWrite(Show.Date($"{DateFormatOpt.Invoke(Show.GetDate(it))} "));
                ItemWrite(Show.GetDirName(io.GetRelativeName(it.FullName)));
                ItemWrite(LinkOpt.Invoke(it));
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
            .Select((it) => io.ToInfoFile(it))
            .Where((it) => it.IsNotFake())
            .Where((it) => Wild.CheckIfFileNameMatched(it.Name))
            .Where((it) => (false == Wild.ExclFileNameOpt.Invoke(it.Name)))
            .Where((it) => Wild.IsMatchWithinSize(it.Length))
            .Where((it) => Wild.IsMatchWithinDate(Show.GetDate(it)))
            .Where((it) => Wild.IsMatchNotWithinSize(it.Length))
            .Where((it) => Wild.IsMatchNotWithinDate(Show.GetDate(it)))
            .Where((it) => Wild.ExtensionOpt.Invoke(it))
            .Where((it) => IsHiddenOpt.Invoke(it))
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
                    Write(Show.Size(Show.LengthFormatOpt.Invoke(sum.Length)));
                    WriteLine(Show.Date($"{DateFormatOpt.Invoke(sum.StartTime)}"));
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

    record DateParse(string pattern, Func<int, TimeSpan> toTimeSpan);

    static public string DefaultDateTimeFormatString
    { get; private set; } = "yyyy-MM-dd HH:mm";

    static public readonly IInovke<DateTime, string> DateFormatOpt =
        new ParseInvoker<DateTime, string>(name: "--date-format",
            help: "DATE-FORMAT   e.g. yyyy-MMM-dd HH:mm:ss",
            init: (value) => value.ToString(DefaultDateTimeFormatString),
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
                if (aa.Length > 1)
                    throw new ArgumentException($"Too many values to {parser.Name}");
                DefaultDateTimeFormatString = aa[0];
                Func<DateTime, string> rtn = (value) => value.ToString(aa[0]);
                _ = rtn(DateTime.MinValue);
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
        });

    static public bool TryParseDateTime(string arg, out DateTime result)
    {
        result = DateTime.MinValue;
        var pattern3 = new Dictionary<string, DateParse>()
        {
            ["minute"] = new DateParse(@"^(?<minute>\d+)min$", (it) => TimeSpan.FromMinutes(it)),
            ["hour"] = new DateParse(@"^(?<hour>\d+)hour$", (it) => TimeSpan.FromHours(it)),
            ["hour2"] = new DateParse(@"^(?<hour2>\d+)hr$", (it) => TimeSpan.FromHours(it)),
            ["day"] = new DateParse(@"^(?<day>\d+)day$", (it) => TimeSpan.FromDays(it)),
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
}
