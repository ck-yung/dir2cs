using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static dir2.MyOptions;


namespace dir2;

static public partial class Helper
{
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
                        #region
                        // 'keyXXX' must be lower case
                        // length of key is 4 ~ 8
                        const string keyToday = "today";
                        const string keyYsDay = "ysday";
                        const string keyTodayAM = "todayam";
                        const string keyYsDayAM = "ysdayam";
                        const string keyTodayPM = "todaypm";
                        const string keyYsDayPM = "ysdaypm";
                        const string keyWeek = "week";
                        const string keyYear = "year";
                        const string keyElse = "else";
                        // length of key is 8 ~ 11
                        const string keyTodayAmPm = "todayampm";
                        const string keyYsDayAmPm = "ysdayampm";
                        #endregion

                        Func<string> fnJust = () => "Just".PadRight(8);
                        var formatMap = new Dictionary<string, Func<DateTime, string>>()
                        {
                            [keyToday] = (it) =>
                            it.ToString("hh:mmtt", CultureInfo.InvariantCulture).PadRight(8),

                            [keyYsDay] = (it) =>
                            it.ToString("\"Yd\" hhtt", CultureInfo.InvariantCulture).PadRight(8),

                            [keyTodayAM] = (it) =>
                            it.ToString("hh:mmtt", CultureInfo.InvariantCulture).PadRight(8),

                            [keyYsDayAM] = (it) =>
                            it.ToString("\"Yd\" hhtt", CultureInfo.InvariantCulture).PadRight(8),

                            [keyTodayPM] = (it) =>
                            it.ToString("hh:mmtt", CultureInfo.InvariantCulture).PadRight(8),

                            [keyYsDayPM] = (it) =>
                            it.ToString("\"Yd\" hhtt", CultureInfo.InvariantCulture).PadRight(8),

                            [keyWeek] = (it) =>
                            it.ToString("ddd hhtt", CultureInfo.InvariantCulture).PadRight(8),

                            [keyYear] = (it) =>
                            it.ToString("MMM dd", CultureInfo.InvariantCulture).PadRight(8),

                            [keyElse] = (it) =>
                            it.ToString("yyyy MMM", CultureInfo.InvariantCulture).PadRight(8),
                        };
                        var ampmFlags = new Dictionary<string, bool>()
                        {
                            [keyTodayAmPm] = false,
                            [keyYsDayAmPm] = false,
                        };

                        var cfgFilename = Config.GetFilename()[..^4]
                        + ".date-short.opt";

                        if (File.Exists(cfgFilename))
                        {
                            IEnumerable<string> MyRegxPrase(IEnumerable<string> lines,
                                Regex regx, string keyName, string valueName,
                                Action<IEnumerable<KeyValuePair<string, string>>> apply)
                            {
                                var matchResult = lines
                                .Select((it) => new
                                {
                                    Text = it,
                                    MatchResult = regx.Match(it)
                                })
                                .GroupBy((it) => it.MatchResult.Success)
                                .ToImmutableDictionary((grp) => grp.Key,
                                elementSelector: (grp) => grp);
                                if (matchResult.TryGetValue(true, out var matchingThe))
                                {
                                    apply(matchingThe
                                        .Select((it) => new KeyValuePair<string, string>(
                                            it.MatchResult.Groups[keyName].Value.ToLower(),
                                            it.MatchResult.Groups[valueName].Value.Trim()))
                                        .Where((it) => it.Value.Length > 0));
                                }
                                if (matchResult.TryGetValue(false, out var notMatching))
                                {
                                    return notMatching.Select((it) => it.Text);
                                }
                                return Enumerable.Empty<string>();
                            }

                            var cultureThe = CultureInfo.InvariantCulture;
                            var regCulture = new Regex(
                                @"^culture(\s=\s|\s=|=\s|=)(?<Culture>\w.*)$",
                                RegexOptions.IgnoreCase);

                            var regJust = new Regex(
                                @"^Just(\s=\s|\s=|=\s|=)(?<Format>\w.*)$",
                                RegexOptions.IgnoreCase);

                            var regFormat = new Regex(
                                @"^(?<Name>\w{4,11})(\s=\s|\s=|=\s|=)(?<Format>.*)$",
                                RegexOptions.IgnoreCase);

                            var regAmPmFormat = new Regex(
                                @"^(?<Name>\w{8,11})(\s:\s|\s:|:\s|:)(?<Format>.*)$",
                                RegexOptions.IgnoreCase);

                            var buf2 = new byte[1024];
                            int readCnt = 0;
                            using (var inpFp = File.OpenRead(cfgFilename))
                            {
                                readCnt = inpFp.Read(buf2);
                            }

                            var qryPhase1 = MyRegxPrase(
                                Encoding.UTF8.GetString(buf2, 0, readCnt)
                                .Split('\n', '\r')
                                .Select((it) => it.Trim())
                                .Where((it) => it.Length > 0),
                                regCulture,
                                keyName: "none", valueName: "Culture",
                                apply: (seq) =>
                                {
                                    var founds = seq
                                    .Select((it) => it.Value)
                                    .Distinct()
                                    .Take(2).ToImmutableArray();
                                    if (founds.Length == 1)
                                    {
                                        cultureThe = CultureInfo
                                        .CreateSpecificCulture(founds[0]);
                                    }
                                    else if (founds.Length > 1)
                                    {
                                        Console.Error.WriteLine($"""
                                        '{cfgFilename}' contains too many cultures
                                        '{founds[0]}', '{founds[1]}', ...
                                        """);
                                    }
                                });

                            var qryPhase2 = MyRegxPrase(qryPhase1, regJust,
                                keyName: "none", valueName: "Format",
                                apply: (seq) =>
                                {
                                    var founds = seq
                                    .Select((it) => it.Value)
                                    .Distinct()
                                    .Take(2)
                                    .ToImmutableArray();
                                    switch (founds.Length)
                                    {
                                        case 0:
                                            // do nothing ..
                                            break;
                                        case 1:
                                            var textJust = System.Net.WebUtility
                                            .UrlDecode(founds[0]);
                                            fnJust = () => textJust;
                                            break;
                                        default:
                                            Console.Error.WriteLine(
                                                $"'{cfgFilename}' contains too many 'Just'" +
                                                $" '{founds[0]}', '{founds[1]}', ...");
                                            break;
                                    };
                                });

                            var qryPhase3 = MyRegxPrase(qryPhase2, regFormat,
                                keyName: "Name", valueName: "Format",
                                apply: (seq) =>
                                {
                                    var founds = seq
                                    .GroupBy((it) => it.Key)
                                    .ToImmutableDictionary(
                                        (grp) => grp.Key, (grp) => grp);
                                    foreach (var keyName in formatMap.Keys)
                                    {
                                        if (founds.TryGetValue(keyName, out var seqMatching))
                                        {
                                            var matchings = seqMatching
                                            .Select((it) => it.Value)
                                            .Distinct()
                                            .Take(2)
                                            .ToImmutableArray();
                                            switch (matchings.Length)
                                            {
                                                case 0:
                                                    // do nothing ..
                                                    break;
                                                case 1:
                                                    var dateFmtThe2 = System.Net.WebUtility.UrlDecode(
                                                        matchings[0]);
                                                    Func<DateTime, string> decodingFormatThe =
                                                    (it) => it.ToString(dateFmtThe2, cultureThe);
                                                    try
                                                    {
                                                        _ = decodingFormatThe(DateTime.MinValue);
                                                        formatMap[keyName] = decodingFormatThe;
                                                    }
                                                    catch (Exception ee2)
                                                    {
                                                        Console.Error.WriteLine($"""
                                                        Loading '{cfgFilename}', key='{keyName}', format='{dateFmtThe2}'
                                                        {ee2.GetType()}: {ee2.Message}
                                                        """);
                                                    }
                                                    break;
                                                default:
                                                    Console.Error.WriteLine($"""
                                        '{cfgFilename}' contains too many '{keyName}'
                                        '{matchings[0]}', '{matchings[1]}', ...
                                        """);
                                                    break;
                                            }
                                        }
                                    }
                                });

                            _ = MyRegxPrase(qryPhase3, regAmPmFormat,
                                keyName: "Name", valueName: "Format",
                                apply: (seq) =>
                                {
                                    var founds = seq
                                    .GroupBy((it) => it.Key)
                                    .ToImmutableDictionary(
                                        (grp) => grp.Key,
                                        elementSelector: (grp) => grp);
                                    foreach (var keyName in ampmFlags.Keys)
                                    {
                                        if (founds.TryGetValue(keyName, out var seqMatching))
                                        {
                                            var matchings = seqMatching
                                            .Select((it) => it.Value.ToLower())
                                            .Distinct()
                                            .Take(2)
                                            .ToImmutableArray();
                                            switch (matchings.Length)
                                            {
                                                case 0:
                                                    // do nothing ..
                                                    break;
                                                case 1:
                                                    var valueFound = matchings[0];
                                                    ampmFlags[keyName] = (valueFound == "yes")
                                                    || (valueFound == "true");
                                                    break;
                                                default:
                                                    Console.Error.WriteLine($"""
                                        '{cfgFilename}' contains too many '{keyName}'
                                        '{matchings[0]}', '{matchings[1]}', ...
                                        """);
                                                    break;
                                            }
                                        }
                                    }
                                })
                            .Count();
                        }

                        var now = DateTime.Now;
                        var withinTwoMinutes = now.AddMinutes(-2);
                        var todayMidnight = new DateTime(now.Year, now.Month, now.Day);
                        var todayNoon = todayMidnight.AddHours(12);
                        var yesterday = todayMidnight.AddDays(-1);
                        var yesterdayNoon = todayMidnight.AddDays(-1);
                        #region the formats ..
                        var fnToday = formatMap[keyToday];
                        var fnYsDay = formatMap[keyYsDay];
                        var fnTodayAM = formatMap[keyTodayAM];
                        var fnYsDayAM = formatMap[keyYsDayAM];
                        var fnTodayPM = formatMap[keyTodayPM];
                        var fnYsDayPM = formatMap[keyYsDayPM];
                        var fnWeek = formatMap[keyWeek];
                        var fnYear = formatMap[keyYear];
                        var fnElse = formatMap[keyElse];
                        #endregion

                        if (ampmFlags[keyTodayAmPm])
                        {
                            fnToday = (date) =>
                            {
                                if (date < todayNoon) return fnTodayAM(date);
                                return fnTodayPM(date);
                            };
                        }

                        if (ampmFlags[keyYsDayAmPm])
                        {
                            fnYsDay = (date) =>
                            {
                                if (date < yesterdayNoon) return fnYsDayAM(date);
                                return fnYsDayPM(date);
                            };
                        }

                        Func<DateTime, bool> checkDayOfWeek = (date) =>
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

}
