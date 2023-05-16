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
        if (PrintDir != EnumPrintDir.Tree)
        {
            CheckIfDirNameMatched = (it) => matchFuncs.Any((chk) => chk(it));
        }
    }

    static internal bool IsExclFeature(IParse arg) => arg is ExclFeauture<string, bool>;

    static public IEnumerable<TypedArg> SelectExclFeatures(IParse[] options,
        IEnumerable<TypedArg> args)
    {
        var optNames = options.Select((it) => it.Name).ToArray();
        var it = args.GetEnumerator();
        while (it.MoveNext())
        {
            var current = it.Current;
            if (optNames.Contains(current.arg))
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
                var checkFuncs = Helper.CommonSplit(args)
                .Select((it) => ToWildMatch(it))
                .ToArray();
                parser.SetImplementation((arg) => checkFuncs.Any((chk) => chk(arg)));
            });

    static internal readonly IInovke<string, bool> ExclDirNameOpt =
        new ExclFeauture<string, bool>("--excl-dir", help: "EXCL-WILD[;EXCL-WILD ..]",
            init: Helper.Never,
            resolve: (parser, args) =>
            {
                var checkFuncs = Helper.CommonSplit(args)
                .Select((it) => ToWildMatch(it))
                .ToArray();
                parser.SetImplementation((arg) => checkFuncs.Any((chk) => chk(arg)));
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

    record WithData(bool IsSize, long Size, DateTime Date)
    {
        public WithData(long size)
            : this(IsSize: true, Size: size, Date: DateTime.MinValue)
        { }

        public WithData(DateTime date)
            : this(IsSize: false, Size: 0, Date: date)
        { }

        static public WithData Parse(string name, string arg)
        {
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

            if (hintSizeDate.Contains(arg.ToUpper()))
            {
                Console.Error.WriteLine($"""
                                Command line option could be
                                  {name} 123
                                  {name} 789k
                                  {name}  59min
                                  {name}   7day
                                  {name} 2019-06-12T15:20
                                """);
            }

            throw new ArgumentException($"'{arg}' is bad to {name}");
        }
    }

    static readonly string[] hintSizeDate = new string[] { "SIZE", "DATE" };

    static internal Func<long, bool> IsMatchWithinSize
    { get; private set; } = Always<long>.True;

    static internal Func<DateTime, bool> IsMatchWithinDate
    { get; private set; } = Always<DateTime>.True;

    static internal readonly IParse WithinOpt = new SimpleParser(name: "--within",
            help: "SIZE | DATE   where SIZE ends with k, m, or g; DATE ends with min, hour, or day",
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct()
                .Select((it) => (restrict: WithData.Parse(parser.Name, it), arg: it))
                .GroupBy((it) => it.restrict.IsSize)
                .ToImmutableDictionary(
                    (grp) => grp.Key, (grp) => grp.AsEnumerable());

                if (aa.ContainsKey(true))
                {
                    var sizeWith = aa[true].Take(2).ToArray();
                    if (sizeWith.Length > 1)
                    {
                        throw new ArgumentException(
                            $"Too many Size '{sizeWith[0].arg}', '{sizeWith[1].arg}' to {parser.Name}");
                    }
                    var sizeMax = sizeWith[0].restrict.Size;
                    IsMatchWithinSize = (size) => (size <= sizeMax);
                }

                if (aa.ContainsKey(false))
                {
                    var dateWithin = aa[false].Take(2).ToArray();
                    if (dateWithin.Length > 1)
                    {
                        throw new ArgumentException(
                            $"Too many DateTime '{dateWithin[0].arg}', '{dateWithin[1].arg}' to {parser.Name}");
                    }
                    var dateMax = dateWithin[0].restrict.Date;
                    IsMatchWithinDate = (date) => (date >= dateMax);
                }
            });

    static internal Func<long, bool> IsMatchNotWithinSize
    { get; private set; } = Always<long>.True;

    static internal Func<DateTime, bool> IsMatchNotWithinDate
    { get; private set; } = Always<DateTime>.True;

    static internal readonly IParse NotWithinOpt = new SimpleParser(name: "--not-within",
            help: "SIZE | DATE   where SIZE ends with k, m, or g; DATE ends with min, hour, or day",
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct()
                .Select((it) => (restrict: WithData.Parse(parser.Name, it), arg: it))
                .GroupBy((it) => it.restrict.IsSize)
                .ToImmutableDictionary(
                    (grp) => grp.Key, (grp) => grp.AsEnumerable());

                if (aa.ContainsKey(true))
                {
                    var sizeNotWith = aa[true].Take(2).ToArray();
                    if (sizeNotWith.Length > 1)
                    {
                        throw new ArgumentException(
                            $"Too many Size '{sizeNotWith[0].arg}', '{sizeNotWith[1].arg}' to {parser.Name}");
                    }
                    var sizeMin = sizeNotWith[0].restrict.Size;
                    IsMatchNotWithinSize = (size) => (size > sizeMin);
                }

                if (aa.ContainsKey(false))
                {
                    var dateNotWithin = aa[false].Take(2).ToArray();
                    if (dateNotWithin.Length > 1)
                    {
                        throw new ArgumentException(
                            $"Too many DateTime '{dateNotWithin[0].arg}', '{dateNotWithin[1].arg}' to {parser.Name}");
                    }
                    var dateMin = dateNotWithin[0].restrict.Date;
                    IsMatchNotWithinDate = (date) => (date < dateMin);
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
