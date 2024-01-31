using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static dir2.MyOptions;

namespace dir2;

static public partial class Helper
{
    record DateParse(string Pattern, Func<int, TimeSpan> ToTimeSpan);

    static public readonly string DefaultDateTimeFormatString
        = "yyyy-MM-dd HH:mm";

    [GeneratedRegex(
        @"^utc(?<timespan>[-+][01]?([0-9])(:[0-5][0-9])?)?$",
        RegexOptions.IgnoreCase)]
    private static partial Regex utcTimespan();

    enum HelpType { Both, ContainingDate, ContainingTime };

    static public readonly IInovke<DateTimeOffset, string> DateFormatOpt =
        new ParseInvoker<DateTimeOffset, string>(name: "--date-format",
            help: "DATE-FORMAT | ZONE  (short, yy-MM-dd%20HH:mm:ss, unix, unix+, UTC+hh:mm)",
            init: (value) => value.ToString(DefaultDateTimeFormatString),
            resolve: (parser, args) =>
            {
                void ShowFormatCode()
                {
                    Console.Write("""
                        Code | Output
                        ---- | ------
                        yy   | 2-digit year
                        yyyy | 4-digit year
                        MM   | 2-digit month
                        MMM  | 3-char month name (in en-US)
                        dd   | 2-digit day of month
                        ddd  | 3-char weekday name (in en-US)
                        tt   | AM or PM
                        HH   | 2-digit 24-hour
                        hh   | 2-digit 12-hour
                        mm   | 2-digit minute
                        ss   | 2-digit second
                        zz   | 3-digit time-zone (eg. +08)
                        zzz  | 6-digit time-zone (eg. +08:00)

                        Char Code | Format
                        --------- | ------
                        g         | Format customed by the computer
                        r         | ddd, dd MMM yyyy HH:mm:ss "GMT"
                        s         | yyyy-MM-ddTHH:mm:ss
                        z         | yyyy-MM-ddTHH:mm:sszz
                        """);
                }
                void ShowFormatHelp(HelpType type)
                {
                    IEnumerable<string> infoLines = DateTimeFormats;
                    string extra = "Run 'dir2 -D time' show time formats.";
                    switch (type)
                    {
                        case HelpType.ContainingDate:
                            infoLines = DateTimeFormats
                            .Where((it) => it.Contains('M'))
                            .Union(new string[]
                            {
                                "(ddd) MMM dd, yyyy",
                            });
                            break;
                        case HelpType.ContainingTime:
                            infoLines = DateTimeFormats
                            .Where((it) => it.Contains('m'));
                            extra = "Run 'dir2 -D date' show time formats.";
                            break;
                        default:
                            break;
                    }
                    Console.WriteLine("Format example:");
                    foreach (var line in infoLines)
                    {
                        Console.WriteLine($"    --date-format \"{line}\"");
                    }
                    Console.WriteLine(extra);
                    Console.Write("Run 'dir2 -D code' for the meaning of format code.");
                }

                void ShowTimeZoneHelp()
                {
                    Console.Write("""
                            Format of time zone setting:
                                --date-format UTC+hh:mm
                            For example,
                                -D +8
                                -D -7
                                --date-format UTC+08:00
                                --date-format UTC-07:30
                                --date-format UTC+06
                                --date-format UTC-5
                            """);
                }

                var timespanFound = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
                var regUtc = utcTimespan();
                bool IsTimeSpan(string arg)
                {
                    var matchUtc = regUtc.Match(arg);
                    if (matchUtc.Success)
                    {
                        var textFound = matchUtc.Groups["timespan"].Value;
                        var a3 = textFound.StartsWith('+') ? textFound.Substring(1) : textFound;
                        if (false == a3.Contains(':'))
                        {
                            a3 += ":00";
                        }
                        if (TimeSpan.TryParse(a3, out var timespanThe))
                        {
                            if (timespanThe < TimeSpan.FromHours(-12))
                            {
                                throw new ConfigException(
                                    $"{parser.Name} cannot be less than (UTC) -12:00 but '{textFound}' is found.");
                            }

                            if (timespanThe > TimeSpan.FromHours(12))
                            {
                                throw new ConfigException(
                                    $"{parser.Name} cannot be greater than (UTC) +12:00 but '{textFound}' is found.");
                            }

                            var a4 = timespanThe.ToString()[..^3];
                            if (timespanThe > TimeSpan.Zero)
                            {
                                ReportTimeZone = " +" + a4;
                            }
                            else
                            {
                                ReportTimeZone = " "+a4;
                            }

                            timespanFound = timespanThe;
                            return true;
                        }
                    }
                    return false;
                }

                var formatFound = string.Empty;
                var theArgs = GetUniqueTexts(args, 2, parser);

                if (theArgs.Length == 1)
                {
                    if (IsTimeSpan(theArgs[0]))
                    {
                        FromUtcToReportTimeZone = (arg) => arg.ToOffset(timespanFound);
                        return;
                    }
                    formatFound = theArgs[0];
                }
                else
                {
                    switch (IsTimeSpan(theArgs[0]), IsTimeSpan(theArgs[1]))
                    {
                        case (false, true):
                            formatFound = theArgs[0];
                            FromUtcToReportTimeZone = (arg) =>
                                arg.ToOffset(timespanFound);
                            break;
                        case (true, false):
                            formatFound = theArgs[1];
                            FromUtcToReportTimeZone = (arg) =>
                                arg.ToOffset(timespanFound);
                            break;
                        default:
                            throw new ConfigException(
                                $"{parser.Name} : '{theArgs[0]}' and '{theArgs[1]}' are same type!");
                    }
                }

                Func<DateTimeOffset, string> rtn = (_) => string.Empty;
                var formatLc = formatFound.ToLower();
                switch (formatLc)
                {
                    case "unix":
                        if (Show.IsOututCsv)
                        {
                            rtn = (value) => value.ToUnixTimeSeconds().ToString();
                        }
                        else
                        {
                            rtn = (value) => $"{value.ToUnixTimeSeconds(),11}";
                        }
                        break;
                    case "unix+":
                        if (Show.IsOututCsv)
                        {
                            rtn = (value) => $"{value.ToUnixTimeSeconds()}.{value.Millisecond:D3}";
                        }
                        else
                        {
                            rtn = (value) => $"{value.ToUnixTimeSeconds(),11}.{value.Millisecond:D3}";
                        }
                        break;
                    case "short":
                        rtn = ParseToDateShortFormat(timespanFound);
                        break;
                    case "z":
                        rtn = (value) => value.ToString("yyyy-MM-ddTHH:mm:sszz");
                        break;
                    case "zone":
                        ShowTimeZoneHelp();
                        throw new ConfigException(string.Empty);
                    case "date-format":
                        ShowFormatHelp(HelpType.Both);
                        throw new ConfigException(string.Empty);
                    case "date":
                        ShowFormatHelp(HelpType.ContainingDate);
                        throw new ConfigException(string.Empty);
                    case "time":
                        ShowFormatHelp(HelpType.ContainingTime);
                        throw new ConfigException(string.Empty);
                    case "code":
                        ShowFormatCode();
                        throw new ConfigException(string.Empty);
                    default:
                        if (formatLc.StartsWith("utc"))
                        {
                            ShowTimeZoneHelp();
                            throw new ConfigException(string.Empty);
                        }
                        var fmtThe = System.Net.WebUtility.UrlDecode(formatFound);
                        rtn = (value) => value.ToString(fmtThe);
                        try
                        {
                            _ = rtn(DateTimeOffset.Now); // verify if the lambda is valid
                        }
                        catch (FormatException)
                        {
                            throw new ConfigException(
                                $"Format '{formatFound}' is invalid to {parser.Name}");
                        }
                        break;
                };
                parser.SetImplementation(rtn);
            });

    #region "Format for Parsing Date/Time value"
    static public readonly ImmutableArray<string> TimeZoneFormats =
        ["", " zz", " zzz", " z", "zz", "zzz", "z"];

    static public readonly ImmutableArray<string> DateTimeFormats =
        [
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
        ];
    #endregion

    static public bool TryParseDateTime(string arg, out DateTimeOffset outputDate)
    {
        ImmutableDictionary<string, DateParse> pattern3 = new Dictionary<string, DateParse>()
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
        }.ToImmutableDictionary();

        foreach (var (keyThe, parseThe) in pattern3)
        {
            foreach (Match match in Regex.Matches(arg, parseThe.Pattern,
                RegexOptions.IgnoreCase))
            {
                var numThe = int.Parse(match.Groups[keyThe].ToString());
                outputDate = DateTimeOffset.UtcNow.Subtract(parseThe.ToTimeSpan(numThe));
                return true;
            }
        }

        string[] qq2 = [arg, System.Net.WebUtility.UrlDecode(arg)];
        qq2 = new string[] { string.Empty, ReportTimeZone }
            .SelectMany((it) => qq2.Select((it2) => it2 + it))
            .Distinct()
            .ToArray();

        foreach (var tz in TimeZoneFormats)
        {
            var formatsThe = DateTimeFormats
                .Select((it) => it + tz)
                .ToArray();
            var parseResult = qq2
                .Select((it) =>
                {
                    if (DateTimeOffset.TryParseExact(it,
                    formats: formatsThe,
                    formatProvider: CultureInfo.InvariantCulture,
                    styles: DateTimeStyles.None, out var result))
                    {
                        return (Flag: true, Date: result);
                    }
                    else
                    {
                        return (Flag: false, Date: DateTimeOffset.MinValue);
                    }
                })
                .FirstOrDefault((it) => it.Flag);
            if (parseResult.Flag)
            {
                outputDate = parseResult.Date;
                return true;
            }
        }
        outputDate = DateTimeOffset.MinValue;
        return false;
    }

    [GeneratedRegex(
        @"^culture(=|\s*=\s*)(?<Culture>\w.*})$",
        RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RegCulture();

    [GeneratedRegex(
        @"^Just(=|\s*=\s*)(?<Format>\w.*)$",
        RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RegJustText();

    [GeneratedRegex(
        @"^(?<Name>[a-zA-Z0-9~]{1,21})(=|\s*=\s*)(?<Format>\w.*)$",
        RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RegNameFormat();

    private record FormatFunc(bool NeedConverted, Func<DateTimeOffset, string> Func);

    private static Func<DateTimeOffset, string> ParseToDateShortFormat(
        TimeSpan timespanFound)
    {
        #region KeyNames in 'dir2-date-short.txt'
        const string keyCulture = "culture";
        const string keyJust = "just";
        const string keyToday = "today";
        const string keyTodayHours = "todayhours";
        const string keyToday06 = "today00~06";
        const string keyToday12 = "today06~12";
        const string keyToday18 = "today12~18";
        const string keyToday24 = "today18~24";
        const string keyYsday = "ysday";
        const string keyYsdayHours = "ysdayhours";
        const string keyYsday06 = "ysday00~06";
        const string keyYsday12 = "ysday06~12";
        const string keyYsday18 = "ysday12~18";
        const string keyYsday24 = "ysday18~24";
        const string keyWkdayHours = "wkdayhours";
        const string keyWkday = "wkday";
        const string keyWkday06 = "wkday00~06";
        const string keyWkday12 = "wkday06~12";
        const string keyWkday18 = "wkday12~18";
        const string keyWkday24 = "wkday18~24";

        const string keyMonth = "month";
        const string keyYear = "year";
        const string keyElse = "else";
        #endregion

        #region timestamp markers
        var now = FromUtcToReportTimeZone(DateTimeOffset.UtcNow);
        var notWithinTenMinutes = now.AddMinutes(10);
        var withinTwoMinutes = now.AddMinutes(-2);
        var todayTimeZero = new DateTimeOffset(
            now.Year, now.Month, now.Day,
            0, 0, 0, timespanFound);
        var today00 = todayTimeZero.AddHours(1);
        var today06 = todayTimeZero.AddHours(6);
        var today12 = todayTimeZero.AddHours(12);
        var today18 = todayTimeZero.AddHours(18);
        var yesterday = todayTimeZero.AddDays(-1);
        var yesterday00 = today00.AddDays(-1);
        var yesterday06 = today06.AddDays(-1);
        var yesterday12 = today12.AddDays(-1);
        var yesterday18 = today18.AddDays(-1);
        #endregion

        #region Default Format Functions
        // ..........................................."12345678"
        string DefaultJustFormat(DateTimeOffset _) => "Just    ";
        string DefaultFlag(DateTimeOffset _) => "No"; // ............."12345678"
        string DefaultTodayFormat(DateTimeOffset arg) => arg.ToString("hh:mmtt ");
        // .................................................................12345678"
        string DefaultYsdayFormat(DateTimeOffset arg) => arg.ToString("\"Yd\" hhtt ");
        // ............................................................12345678"
        string DefaultWkdayFormat(DateTimeOffset arg) => arg.ToString("ddd hhtt");
        // ............................................................12345678"
        string DefaultMonthFormat(DateTimeOffset arg) => arg.ToString("MMM dd  ");
        // ...........................................................12345678"
        string DefaultYearFormat(DateTimeOffset arg) => arg.ToString("MMM dd  ");
        // ............................................................12345678"
        string DefaultElseFormat(DateTimeOffset arg) => arg.ToString("yyyy MMM");
        #endregion

        #region Dictionary of Key to Default Formating Functions and Flags
        var formatMap = new Dictionary<string, FormatFunc>()
        {
            [keyCulture] = new FormatFunc(false, DefaultFlag),
            [keyTodayHours] = new FormatFunc(false, DefaultFlag),
            [keyYsdayHours] = new FormatFunc(false, DefaultFlag),
            [keyWkdayHours] = new FormatFunc(false, DefaultFlag),

            [keyJust] = new FormatFunc(true, DefaultJustFormat),
            [keyMonth] = new FormatFunc(true, DefaultMonthFormat),
            [keyYear] = new FormatFunc(true, DefaultYearFormat),
            [keyElse] = new FormatFunc(true, DefaultElseFormat),

            [keyToday] = new FormatFunc(true, DefaultTodayFormat),
            [keyToday06] = new FormatFunc(true, DefaultTodayFormat),
            [keyToday12] = new FormatFunc(true, DefaultTodayFormat),
            [keyToday18] = new FormatFunc(true, DefaultTodayFormat),
            [keyToday24] = new FormatFunc(true, DefaultTodayFormat),

            [keyYsday] = new FormatFunc(true, DefaultYsdayFormat),
            [keyYsday06] = new FormatFunc(true, DefaultYsdayFormat),
            [keyYsday12] = new FormatFunc(true, DefaultYsdayFormat),
            [keyYsday18] = new FormatFunc(true, DefaultYsdayFormat),
            [keyYsday24] = new FormatFunc(true, DefaultYsdayFormat),

            [keyWkday] = new FormatFunc(true, DefaultWkdayFormat),
            [keyWkday06] = new FormatFunc(true, DefaultWkdayFormat),
            [keyWkday12] = new FormatFunc(true, DefaultWkdayFormat),
            [keyWkday18] = new FormatFunc(true, DefaultWkdayFormat),
            [keyWkday24] = new FormatFunc(true, DefaultWkdayFormat),
        };
        var flagHours = new Dictionary<string, bool>()
        {
            [keyTodayHours] = false,
            [keyYsdayHours] = false,
            [keyWkdayHours] = false,
        };
        #endregion

        var cfgFilename = Config.GetFilename(Config.ShortDateCfg);
        if (File.Exists(cfgFilename))
        {
            #region Read the Date-Short Format Config File
            using var inpFp = File.OpenRead(cfgFilename);
            int cntRead = 0;
            var buf2 = new byte[1024];
            cntRead = inpFp.Read(buf2);
            #endregion

            var convertTypeGroups = Encoding.UTF8.GetString(
                buf2, 0, cntRead).Split('\n', '\r')
                .Select((it) => it.Split('=', count: 2))
                .Where((it) => it.Length == 2)
                .Select((it) => new
                {
                    Key = it[0].Trim().ToLower(),
                    Text = it[1].Trim()
                })
                .Where((it) => it.Key.Length > 0 && it.Text.Length > 0)

                .Join(formatMap,
                outerKeySelector: (it) => it.Key,
                innerKeySelector: (it) => it.Key,
                resultSelector: (foundThe, matchThe) =>
                new
                {
                    foundThe.Key,
                    matchThe.Value.NeedConverted,
                    foundThe.Text,
                })

                .GroupBy((it) => it.NeedConverted)
                .ToImmutableDictionary((grp) => grp.Key,
                elementSelector: (grp) => grp
                    .GroupBy((it2) => it2.Key)
                    .ToImmutableDictionary((grp2) => grp2.Key,
                    elementSelector: (grp2) => grp2
                        .Select((it2) => it2.Text)
                        .Take(2)
                        .ToArray())
                );

            var cultureThe = CultureInfo.InvariantCulture;

            string[] flags = ["yes", "true", "enabled", "on"];
            ImmutableDictionary<string, string[]> keyValues;
            string[] founds;

            if (convertTypeGroups.TryGetValue(false, out keyValues))
            {
                if (keyValues.TryGetValue(keyCulture, out founds))
                {
                    if (founds.Length > 1)
                    {
                        throw new ConfigException(
                            $"In {cfgFilename}, '{keyCulture}' has more than one assignment!");
                    }
                    cultureThe = CultureInfo.CreateSpecificCulture(founds[0]);
                }

                foreach (var keyThe in flagHours.Keys)
                {
                    if (keyValues.TryGetValue(keyThe, out founds))
                    {
                        if (founds.Length > 1)
                        {
                            throw new ConfigException(
                                $"In {cfgFilename}, '{keyThe}' has more than one assignment!");
                        }
                        flagHours[keyThe] = flags
                            .Any((it) =>
                            0 == string.Compare(it, founds[0], ignoreCase: true));
                    }
                }
            }

            if (convertTypeGroups.TryGetValue(true, out keyValues))
            {
                foreach (var itm in keyValues)
                {
                    if (itm.Value.Length > 1)
                    {
                        throw new ConfigException(
                            $"In {cfgFilename}, '{itm.Key}' has more than one assignment!");
                    }
                    var textFormat = System.Net.WebUtility.UrlDecode(itm.Value[0]);
                    formatMap[itm.Key] = new FormatFunc(true,
                    (arg) => arg.ToString(textFormat, cultureThe) + " ");
                }
            }
        }

        #region format functions of default value and config value
        var fnJust = formatMap[keyJust].Func;
        var fnToday = formatMap[keyToday].Func;
        var fnToday06 = formatMap[keyToday06].Func;
        var fnToday12 = formatMap[keyToday12].Func;
        var fnToday18 = formatMap[keyToday18].Func;
        var fnToday24 = formatMap[keyToday24].Func;
        var fnYsday = formatMap[keyYsday].Func;
        var fnYsday06 = formatMap[keyYsday06].Func;
        var fnYsday12 = formatMap[keyYsday12].Func;
        var fnYsday18 = formatMap[keyYsday18].Func;
        var fnYsday24 = formatMap[keyYsday24].Func;
        var fnWkday = formatMap[keyWkday].Func;
        var fnWkday06 = formatMap[keyWkday06].Func;
        var fnWkday12 = formatMap[keyWkday12].Func;
        var fnWkday18 = formatMap[keyWkday18].Func;
        var fnWkday24 = formatMap[keyWkday24].Func;
        var fnMonth = formatMap[keyMonth].Func;
        var fnYear = formatMap[keyYear].Func;
        var fnElse = formatMap[keyElse].Func;
        #endregion

        #region Hour-Format for Today, Yeserday, and Weekday
        if (flagHours[keyTodayHours])
        {
            fnToday = (arg) =>
            {
                if (arg < today06) return fnToday06(arg);
                if (arg < today12) return fnToday12(arg);
                if (arg < today18) return fnToday18(arg);
                return fnToday24(arg);
            };
        }

        if (flagHours[keyYsdayHours])
        {
            fnYsday = (arg) =>
            {
                if (arg < yesterday06) return fnYsday06(arg);
                if (arg < yesterday12) return fnYsday12(arg);
                if (arg < yesterday18) return fnYsday18(arg);
                return fnYsday24(arg);
            };
        }

        if (flagHours[keyWkdayHours])
        {
            fnWkday = (arg) =>
            {
                if (arg.Hour <= 6) return fnWkday06(arg);
                if (arg.Hour <= 12) return fnWkday12(arg);
                if (arg.Hour <= 18) return fnWkday18(arg);
                return fnWkday24(arg);
            };
        }
        #endregion

        #region Check if a DateTime value is within the week.
        Func<DateTimeOffset, bool> checkDayOfWeek = (date) =>
        {
            return (now - date) < TimeSpan.FromDays(6)
                && date.DayOfWeek < now.DayOfWeek;
        };

        var envirDir2 = Environment.GetEnvironmentVariable(nameof(dir2));
        if (envirDir2?.Contains("--debug:CheckDayOfWeekOff") ?? false)
        {
            checkDayOfWeek = (date) =>
            {
                return (now - date) < TimeSpan.FromDays(6);
            };
        }
        #endregion

        Func<DateTimeOffset, string> rtn = (timeThe) =>
        {
            if (timeThe > withinTwoMinutes)
            {
                if (timeThe  < notWithinTenMinutes) return fnJust(timeThe);
                return fnElse(timeThe);
            }

            if (timeThe > todayTimeZero) return fnToday(timeThe);

            if (timeThe > yesterday) return fnYsday(timeThe);

            if (now.Year == timeThe.Year)
            {
                if (checkDayOfWeek(timeThe)) return fnWkday(timeThe);
                if (now.Month == timeThe.Month) return fnMonth(timeThe);
                return fnYear(timeThe);
            }
            return fnElse(timeThe);
        };
        return rtn;
    }
}
