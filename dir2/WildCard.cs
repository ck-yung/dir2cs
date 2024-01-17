using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static dir2.MyOptions;

namespace dir2;

static public partial class Wild
{
    static internal StringComparer StringComparer
    { get; private set; } = StringComparer.OrdinalIgnoreCase;

    static internal Func<string, Regex> MakeRegex { get; private set; }
        = (it) => new Regex(it, RegexOptions.IgnoreCase);
    static internal Func<string, string> GetRawText { get; private set; }
        = (it) => it.ToLower();

    static internal readonly IParse CaseSensitiveOpt = new SwitchParser(
        name: "--case-sensitive", action: () =>
        {
            MakeRegex = (it) => new Regex(it, RegexOptions.None);
            StringComparer = StringComparer.Ordinal;
            GetRawText = (it) => it;
        });

    static internal Func<string, string> ToRegexText { get; private set; } = (it) =>
    {
        var regText = new System.Text.StringBuilder("^");
        regText.Append(it
            .Replace(@"\", @"\\")
            .Replace("^", @"\^")
            .Replace("$", @"\$")
            .Replace(".", @"\.")
            .Replace("?", ".")
            .Replace("*", ".*")
            .Replace("(", @"\(")
            .Replace(")", @"\)")
            .Replace("[", @"\[")
            .Replace("]", @"\]")
            .Replace("{", @"\{")
            .Replace("}", @"\}")
            ).Append('$');
        return regText.ToString();
    };
    static internal readonly IParse RegexOpt = new SwitchParser(
        name: "--regex", action: () =>
        {
            ToRegexText = Helper.itself;
        });

    static internal Func<string, bool> ToWildMatch(string arg)
    {
        var regThe = MakeRegex(ToRegexText(arg));
        return (it) => regThe.Match(it).Success;
    }

    static internal Func<string, bool> CheckIfFileNameMatched
    { get; private set; } = Always<string>.True;
    static internal Func<string, bool> CheckIfDirNameMatched
    { get; private set; } = Always<string>.True;

    static internal void InitMatchingNames(IEnumerable<string> names)
    {
        var matchFuncs = names
            .Where((it) => it.Length > 0)
            .Distinct()
            .Select((it) => ToWildMatch(it))
            .ToArray();
        if (matchFuncs.Length == 0) return;
        CheckIfFileNameMatched = (it) => matchFuncs.Any((chk) => chk(it));
        if (PrintDir != EnumPrint.DirTree)
        {
            CheckIfDirNameMatched = (it) => matchFuncs.Any((chk) => chk(it));
        }
    }

    static internal bool IsExclFeature(IParse arg) => arg is ExclFeauture<string, bool>;

    static public IEnumerable<(ArgType, string)> SelectExclFeatures(IParse[] options,
        IEnumerable<(ArgType, string)> args)
    {
        var optNames = options.Select((it) => it.Name).ToArray();
        var it = args.GetEnumerator();
        while (it.MoveNext())
        {
            var current = it.Current;
            if (optNames.Contains(current.Item2))
            {
                if (it.MoveNext())
                {
                    var valueThe = it.Current;
                    yield return current;
                    yield return valueThe;
                }
                else
                {
                    throw new ArgumentException($"Missing value to {current}");
                }
            }
        }
    }

    static internal readonly IInovke<string, bool> ExclFileNameOpt =
        new ExclFeauture<string, bool>("--excl", help: "EXCL-WILD[;EXCL-WILD ..]",
            init: Helper.Never,
            resolve: (parser, args) =>
            {
                var aa = Helper.CommonSplit(args)
                .GroupBy((it) => it.Equals(":link"))
                .ToImmutableDictionary((grp) => grp.Key, (grp) => grp);
                if (aa?.ContainsKey(true) ?? false)
                {
                    var a2 = (ParseInvoker<InfoFile, bool>)Helper.IsLinkFileOpt;
                    a2.SetImplementation((it) => it.IsNotLinked);
                }
                if (aa?.TryGetValue(false, out var aa2) ?? false)
                {
                    var aa3 = aa2.ToArray();
                    if (aa3.Any((it) => it == Helper.ExtraHelp))
                    {
                        throw new ArgumentException($"Syntax: {parser.Name} {parser.Help}");
                    }
                    var checkFuncs = aa3
                    .Select((it) => ToWildMatch(it))
                    .ToArray();
                    parser.SetImplementation((arg) => checkFuncs.Any((chk) => chk(arg)));
                }
            });

    static internal readonly IInovke<string, bool> ExclDirNameOpt =
        new ExclFeauture<string, bool>("--excl-dir", help: "EXCL-WILD[;EXCL-WILD ..]",
            init: Helper.Never,
            resolve: (parser, args) =>
            {
                var aa = Helper.CommonSplit(args)
                .GroupBy((it) => it.Equals(":link"))
                .ToImmutableDictionary((grp) => grp.Key, (grp) => grp);
                if (aa?.ContainsKey(true) ?? false)
                {
                    CheckDirLink = (info) => info.IsNotLinked;
                }

                if (aa?.TryGetValue(false, out var aa2) ?? false)
                {
                    var aa3 = aa2.ToArray();
                    if (aa3.Any((it) => it == Helper.ExtraHelp))
                    {
                        throw new ArgumentException($"Syntax: {parser.Name} {parser.Help}");
                    }

                    var checkFuncs = aa3
                    .Select((it) => ToWildMatch(it))
                    .ToArray();
                    parser.SetImplementation((arg) => checkFuncs.Any((chk) => chk(arg)));
                }
            });

    static internal readonly string ExclNone = "--excl-none";
    static internal string[] Parse_ExclNone(IEnumerable<string> args)
    {
        var chkThe = args
            .GroupBy((it) => it == ExclNone)
            .ToImmutableDictionary(
            (grp) => grp.Key,
            (grp) => grp.AsEnumerable());

        if (chkThe.ContainsKey(true))
        {
            ((ExclFeauture<string, bool>)ExclFileNameOpt).SetImplementation(Helper.Never);
            ((ExclFeauture<string, bool>)ExclDirNameOpt).SetImplementation(Helper.Never);
        }

        if (chkThe.TryGetValue(false, out var others))
        {
            return others.ToArray();
        }
        return Array.Empty<string>();
    }

    enum DataType
    {
        Invalid,
        Size,
        DateTime,
        DeltaDate,
    };

    partial record WithData(DataType Type, long Size, DateTimeOffset Date, TimeSpan DateDelta)
    {
        public WithData(long size)
            : this(DataType.Size, Size: size, Date: DateTimeOffset.MinValue
                  , DateDelta: TimeSpan.Zero)
        { }

        public WithData(DateTimeOffset date)
            : this(DataType.DateTime, Size: 0, Date: date, DateDelta: TimeSpan.Zero)
        { }
        public WithData(TimeSpan dateDelta)
            : this(DataType.DeltaDate, Size: 0, Date: DateTimeOffset.MinValue
                  , DateDelta: dateDelta)
        { }

        static ImmutableDictionary<string, (Regex, Func<int, TimeSpan>)> TryParseTimeSpans
            = new Dictionary<string, (Regex, Func<int, TimeSpan>)>
            {
                ["year"] = (RegexYear(), (it) => TimeSpan.FromDays(365 * it)),
                ["years"] = (RgexeYears(), (it) => TimeSpan.FromDays(365 * it)),
                ["day"] = (RegexDay(), (it) => TimeSpan.FromDays(it)),
                ["days"] = (RegexDays(), (it) => TimeSpan.FromDays(it)),
                ["hour"] = (RegexHours(), (it) => TimeSpan.FromHours(it)),
                ["hours"] = (RegexHour(), (it) => TimeSpan.FromHours(it)),
                ["hr"] = (RegexHourShort(), (it) => TimeSpan.FromHours(it)),
                ["min"] = (RegMinuteShort(),
                (it) => TimeSpan.FromMinutes(it)),
                ["minute"] = (RegexMinute(),
                (it) => TimeSpan.FromMinutes(it)),
                ["minutes"] = (RegexMinutes(),
                (it) => TimeSpan.FromMinutes(it)),
            }.ToImmutableDictionary();

        static public WithData Parse(string name, string arg, bool hasDateDelta)
        {
            ImmutableDictionary<string, string[]> hintSizeDate =
                new Dictionary<string, string[]>
                {
                    ["size"] =
                    [
                        "123",
                        "123b",
                        "34k",
                        "45Kb",
                        "67m",
                        "23Gb",
                    ],

                    ["date"] =
                    [
                        "3day",
                        "4days",
                        "5year",
                        "6years",
                        "2019-06-12",
                        "20140928",
                    ],

                    ["time"] =
                    [
                        "2min",
                        "3minute",
                        "4minutes",
                        "5hr",
                        "6hour",
                        "7hours",
                        "2019-06-12T07:46",
                        "2019-06-30T21:21:32",
                        "03:47pm",
                        "15:47",
                        "15:47:31",
                        "2019-06-12T07:46 +8",
                        "2019-06-30T21:21:32 +08",
                        "03:47pm +08:00",
                    ],
                }.ToImmutableDictionary();

            string[] hintNotWithinDate =
            [
                "+2min",
                "+3minute",
                "+4minutes",
                "+5hr",
                "+6hour",
                "+8hours",
                "+9year",
                "+2day",
                "+3days",
                "+4years",
            ];

            long valueThe;
            if (long.TryParse(arg, out valueThe))
            {
                return new WithData(valueThe);
            }

            if (Helper.TryParseKiloNumber(arg, out valueThe))
            {
                return new WithData(valueThe);
            }

            if (Helper.TryParseDateTime(arg, out var valueDate))
            {
                return new WithData(valueDate);
            }

            if (hasDateDelta)
            {
                foreach (var nameThe in TryParseTimeSpans.Keys)
                {
                    (var checkThe, var toDelta) = TryParseTimeSpans[nameThe];
                    var checkResult = checkThe.Match(arg);
                    if (false == checkResult.Success) continue;
                    var intThe = int.Parse(checkResult.Groups[nameThe].Value);
                    return new WithData(toDelta(intThe));
                }
            }

            if (hintSizeDate.TryGetValue(arg.ToLower(), out var hints))
            {
                Console.Error.WriteLine("Command line option could be");
                foreach (var hintThe in hints)
                {
                    Console.Error.WriteLine($"  {name} {hintThe}");
                }
                if (hasDateDelta && 0==string.Compare(arg,"date",ignoreCase:true))
                {
                    foreach (var hintThe in hintNotWithinDate)
                    {
                        Console.Error.WriteLine($"  {name} {hintThe}");
                    }
                }
                throw new ArgumentException(string.Empty);
            }

            throw new ArgumentException($"'{arg}' is bad to {name}");
        }

        [GeneratedRegex(@"^\+(?<year>\d+)year$", RegexOptions.Compiled)]
        private static partial Regex RegexYear();
        [GeneratedRegex(@"^\+(?<years>\d+)years$", RegexOptions.Compiled)]
        private static partial Regex RgexeYears();
        [GeneratedRegex(@"^\+(?<day>\d+)day$", RegexOptions.Compiled)]
        private static partial Regex RegexDay();
        [GeneratedRegex(@"^\+(?<days>\d+)days$", RegexOptions.Compiled)]
        private static partial Regex RegexDays();
        [GeneratedRegex(@"^\+(?<hour>\d+)hour$", RegexOptions.Compiled)]
        private static partial Regex RegexHours();
        [GeneratedRegex(@"^\+(?<hours>\d+)hours$", RegexOptions.Compiled)]
        private static partial Regex RegexHour();
        [GeneratedRegex(@"^\+(?<hr>\d+)hr$", RegexOptions.Compiled)]
        private static partial Regex RegexHourShort();
        [GeneratedRegex(@"^\+(?<min>\d+)min$", RegexOptions.Compiled)]
        private static partial Regex RegMinuteShort();
        [GeneratedRegex(@"^\+(?<minute>\d+)minute$", RegexOptions.Compiled)]
        private static partial Regex RegexMinute();
        [GeneratedRegex(@"^\+(?<minutes>\d+)minutes$", RegexOptions.Compiled)]
        private static partial Regex RegexMinutes();
    }

    static Func<TimeSpan, string, DateTimeOffset> AssignNotWithinDate { get; set; } = (_, deltaText) =>
    {
        throw new ArgumentException($"Option '--within' is required for '--not-wihtin {deltaText}' !");
    };

    static internal Func<long, bool> IsMatchWithinSize
    { get; private set; } = Always<long>.True;

    static internal Func<DateTimeOffset, bool> IsMatchWithinDate
    { get; private set; } = Always<DateTimeOffset>.True;

    static internal readonly IParse WithinOpt = new SimpleParser(name: "--within",
            help: "SIZE | DATE | TIME",
            resolve: (parser, args) =>
            {
                var aa = Helper.GetUniqueTexts(args, 3, parser)
                .Select((it) => (WithData.Parse(
                    parser.Name, it,
                    hasDateDelta:false), it))
                .GroupBy((it) => it.Item1.Type)
                .ToImmutableDictionary(
                    (grp) => grp.Key, (grp) => grp.AsEnumerable());

                foreach (var typeThe in aa.Keys)
                {
                    switch (typeThe)
                    {
                        case DataType.Size:
                            var sizeWith = aa[typeThe].Take(2).ToArray();
                            if (sizeWith.Length > 1)
                            {
                                throw new ArgumentException(
                                    $"Too many Size '{sizeWith[0].Item2}', '{sizeWith[1].Item2}' to {parser.Name}");
                            }
                            var sizeMax = sizeWith[0].Item1.Size;
                            IsMatchWithinSize = (size) => (size <= sizeMax);
                            break;
                        case DataType.DateTime:
                            var dateWithin = aa[typeThe].Take(2).ToArray();
                            if (dateWithin.Length > 1)
                            {
                                throw new ArgumentException(
                                    $"Too many DateTime '{dateWithin[0].Item2}', '{dateWithin[1].Item2}' to {parser.Name}");
                            }
                            var dateMax = dateWithin[0].Item1.Date;
                            IsMatchWithinDate = (date) => (date >= dateMax);
                            AssignNotWithinDate = (delta, _) => dateMax.Add(delta);
                            break;
                        default:
                            var valueThe = aa[typeThe].FirstOrDefault().Item2;
                            throw new ArgumentException($"{valueThe} is bad to {parser.Name}");
                    }
                }
            });

    static internal Func<long, bool> IsMatchNotWithinSize
    { get; private set; } = Always<long>.True;

    static internal Func<DateTimeOffset, bool> IsMatchNotWithinDate
    { get; private set; } = Always<DateTimeOffset>.True;

    static internal readonly IParse NotWithinOpt = new SimpleParser(name: "--not-within",
            help: "SIZE | DATE | TIME",
            resolve: (parser, args) =>
            {
                var aa = Helper.GetUniqueTexts(args, 3, parser)
                .Select((it) => (WithData.Parse(
                    parser.Name, it,
                    hasDateDelta: false), it))
                .GroupBy((it) => it.Item1.Type)
                .ToImmutableDictionary(
                    (grp) => grp.Key, (grp) => grp.AsEnumerable());

                if (aa.TryGetValue(DataType.Size, out var sizes))
                {
                    var sizeNotWith = sizes.Take(2).ToArray();
                    if (sizeNotWith.Length > 1)
                    {
                        throw new ArgumentException(
                            $"Too many Size '{sizeNotWith[0].Item2}', '{sizeNotWith[1].Item2}' to {parser.Name}");
                    }
                    var sizeMin = sizeNotWith[0].Item1.Size;
                    IsMatchNotWithinSize = (size) => (size > sizeMin);
                }

                switch (aa.TryGetValue(DataType.DateTime, out var dateFounds),
                aa.TryGetValue(DataType.DeltaDate, out var deltaDateFounds))
                {
                    case (false, false):
                        break;
                    case (true, false):
                        var dateNotWithin = dateFounds.Take(2).ToArray();
                        if (dateNotWithin.Length > 1)
                        {
                            throw new ArgumentException(
                                $"Too many DateTime '{dateNotWithin[0].Item2}', '{dateNotWithin[1].Item2}' to {parser.Name}");
                        }
                        var dateMin = dateNotWithin[0].Item1.Date;
                        IsMatchNotWithinDate = (date) => (date < dateMin);
                        break;
                    case (false, true):
                        var deltaFound = deltaDateFounds.Take(2).ToArray();
                        if (deltaFound.Length > 1)
                        {
                            throw new ArgumentException(
                                $"Too many DateTime '{deltaFound[0].Item2}', '{deltaFound[1].Item2}' to {parser.Name}");
                        }
                        var deltaThe = deltaFound[0].Item1.DateDelta;
                        var dateMin2 = AssignNotWithinDate(deltaThe, deltaFound[0].Item2);
                        IsMatchNotWithinDate = (date) => (date < dateMin2);
                        break;
                    default:
                        throw new ArgumentException(
                            $"Too many values to {parser.Name}");
                }
            });

    static internal readonly IInovke<InfoFile, bool> ExtensionOpt =
        new ParseInvoker<InfoFile, bool>("--no-ext", help: "incl | excl | only",
            init: Always<InfoFile>.True, resolve: (parser, args) =>
            {
                var argThe = Helper.GetUnique(args, parser);
                switch (argThe)
                {
                    case "incl":
                        parser.SetImplementation(Always<InfoFile>.True);
                        break;
                    case "excl":
                        parser.SetImplementation((it) => false == string.IsNullOrEmpty(it.Extension));
                        break;
                    case "only":
                        parser.SetImplementation((it) => string.IsNullOrEmpty(it.Extension));
                        break;
                    default:
                        throw new ArgumentException($"'{argThe}' is bad value to {parser.Name}");
                }
            });
}
