using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static dir2.MyOptions;

namespace dir2;

static public class Wild
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
                    a2.SetImplementation((it) => string.IsNullOrEmpty(it.LinkTarget));
                }
                if (aa?.ContainsKey(false) ?? false)
                {
                    var checkFuncs = Helper.CommonSplit(aa[false])
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
                    MyOptions.IsFakeDirOrLinked = (path) =>
                    {
                        var info = Helper.ToInfoDir(path);
                        if (info.IsFake()) return true;
                        return false == string.IsNullOrEmpty(info.LinkTarget);
                    };
                    MyOptions.IsFakeInfoDirOrLinked = (info) =>
                    {
                        if (info.IsFake()) return true;
                        return false == string.IsNullOrEmpty(info.LinkTarget);
                    };
                    CheckDirLink = (info) => true == string.IsNullOrEmpty(info.LinkTarget);
                }
                if (aa?.ContainsKey(false) ?? false)
                {
                    var checkFuncs = Helper.CommonSplit(aa[false])
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

        if (chkThe.ContainsKey(false))
        {
            return chkThe[false].ToArray();
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

    record WithData(DataType Type, long Size, DateTime Date, TimeSpan DateDelta)
    {
        public WithData(long size)
            : this(DataType.Size, Size: size, Date: DateTime.MinValue
                  , DateDelta: TimeSpan.Zero)
        { }

        public WithData(DateTime date)
            : this(DataType.DateTime, Size: 0, Date: date, DateDelta: TimeSpan.Zero)
        { }
        public WithData(TimeSpan dateDelta)
            : this(DataType.DeltaDate, Size: 0, Date: DateTime.MinValue
                  , DateDelta: dateDelta)
        { }

        static ImmutableDictionary<string, (Regex, Func<int, TimeSpan>)> TryParseTimeSpans
            = new Dictionary<string, (Regex, Func<int, TimeSpan>)>
            {
                ["year"] = (new Regex(@"^\+(?<year>\d+)year$", RegexOptions.Compiled),
                (it) => TimeSpan.FromDays(365 * it)),
                ["years"] = (new Regex(@"^\+(?<years>\d+)years$", RegexOptions.Compiled),
                (it) => TimeSpan.FromDays(365 * it)),
                ["day"] = (new Regex(@"^\+(?<day>\d+)day$", RegexOptions.Compiled),
                (it) => TimeSpan.FromDays(it)),
                ["days"] = (new Regex(@"^\+(?<days>\d+)days$", RegexOptions.Compiled),
                (it) => TimeSpan.FromDays(it)),
                ["hour"] = (new Regex(@"^\+(?<hour>\d+)hour$", RegexOptions.Compiled),
                (it) => TimeSpan.FromHours(it)),
                ["hours"] = (new Regex(@"^\+(?<hours>\d+)hours$", RegexOptions.Compiled),
                (it) => TimeSpan.FromHours(it)),
                ["hr"] = (new Regex(@"^\+(?<hr>\d+)hr$", RegexOptions.Compiled),
                (it) => TimeSpan.FromHours(it)),
                ["min"] = (new Regex(@"^\+(?<min>\d+)min$", RegexOptions.Compiled),
                (it) => TimeSpan.FromMinutes(it)),
                ["minute"] = (new Regex(@"^\+(?<minute>\d+)minute$", RegexOptions.Compiled),
                (it) => TimeSpan.FromMinutes(it)),
                ["minutes"] = (new Regex(@"^\+(?<minutes>\d+)minutes$", RegexOptions.Compiled),
                (it) => TimeSpan.FromMinutes(it)),
            }.ToImmutableDictionary();

        static public WithData Parse(string name, string arg, bool hasDateDelta)
        {
            ImmutableDictionary<string, string[]> hintSizeDate =
                new Dictionary<string, string[]>
                {
                    ["size"] = new string[]
                    {
                        "123",
                        "123b",
                        "34k",
                        "45Kb",
                        "67m",
                        "23Gb",
                    },
                    ["date"] = new string[]
                    {
                        "2min",
                        "3hr",
                        "1minute",
                        "2hour",
                        "3day",
                        "4year",
                        "2minutes",
                        "3hours",
                        "4days",
                        "5years",
                        "2019-03-31",
                        "20140928",
                        "2019-06-12T07:46",
                        "2019-06-30T21:21:32",
                        "13:58:59",
                        "13:58",
                        "11:58am",
                    },
                }.ToImmutableDictionary();

            string[] hintNotWithinDate = new string[]
            {
                "+9min",
                "+8hr",
                "+1year",
                "+2day",
                "+3hour",
                "+4minute",
                "+2years",
                "+3days",
                "+4hours",
                "+5minutes",
            };

            long valueThe;
            if (long.TryParse(arg, out valueThe))
            {
                return new WithData(valueThe);
            }

            if (Helper.TryParseKiloNumber( arg, out valueThe))
            {
                return new WithData(valueThe);
            }

            if (Helper.TryParseDateTime(arg, out DateTime valueDate))
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

            if (hintSizeDate.ContainsKey(arg.ToLower()))
            {
                Console.Error.WriteLine("Command line option could be");
                foreach (var hintThe in hintSizeDate[arg.ToLower()])
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
    }

    static Func<TimeSpan, string, DateTime> AssignNotWithinDate { get; set; } = (_, deltaText) =>
    {
        throw new ArgumentException($"Option '--within' is required for '--not-wihtin {deltaText}' !");
    };

    static internal Func<long, bool> IsMatchWithinSize
    { get; private set; } = Always<long>.True;

    static internal Func<DateTime, bool> IsMatchWithinDate
    { get; private set; } = Always<DateTime>.True;

    static internal readonly IParse WithinOpt = new SimpleParser(name: "--within",
            help: "SIZE | DATE",
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct()
                .Select((it) => (WithData.Parse(parser.Name, it, hasDateDelta:false), it))
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

    static internal Func<DateTime, bool> IsMatchNotWithinDate
    { get; private set; } = Always<DateTime>.True;

    static internal readonly IParse NotWithinOpt = new SimpleParser(name: "--not-within",
            help: "SIZE | DATE",
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct()
                .Select((it) => (WithData.Parse(parser.Name, it, hasDateDelta: true), it))
                .GroupBy((it) => it.Item1.Type)
                .ToImmutableDictionary(
                    (grp) => grp.Key, (grp) => grp.AsEnumerable());

                if (aa.ContainsKey(DataType.Size))
                {
                    var sizeNotWith = aa[DataType.Size].Take(2).ToArray();
                    if (sizeNotWith.Length > 1)
                    {
                        throw new ArgumentException(
                            $"Too many Size '{sizeNotWith[0].Item2}', '{sizeNotWith[1].Item2}' to {parser.Name}");
                    }
                    var sizeMin = sizeNotWith[0].Item1.Size;
                    IsMatchNotWithinSize = (size) => (size > sizeMin);
                }

                switch (aa.ContainsKey(DataType.DateTime), aa.ContainsKey(DataType.DeltaDate))
                {
                    case (false, false):
                        break;
                    case (true, false):
                        var dateNotWithin = aa[DataType.DateTime].Take(2).ToArray();
                        if (dateNotWithin.Length > 1)
                        {
                            throw new ArgumentException(
                                $"Too many DateTime '{dateNotWithin[0].Item2}', '{dateNotWithin[1].Item2}' to {parser.Name}");
                        }
                        var dateMin = dateNotWithin[0].Item1.Date;
                        IsMatchNotWithinDate = (date) => (date < dateMin);
                        break;
                    case (false, true):
                        var deltaFound = aa[DataType.DeltaDate].Take(2).ToArray();
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
                var aa = args.Where((it) => it.Length>0).Distinct().Take(2).ToArray();
                if (aa.Length > 1)
                    throw new ArgumentException($"Too many values to {parser.Name}");
                switch (aa[0])
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
                        throw new ArgumentException($"'{aa[0]}' is bad value to {parser.Name}");
                }
            });
}
