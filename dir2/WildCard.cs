using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static dir2.MyOptions;

namespace dir2;

static public class Wild
{
    static internal Func<string, Regex> MakeRegex { get; private set; }
        = (it) => new Regex(it, RegexOptions.IgnoreCase);
    static internal readonly IParse CaseSensitiveOpt = new SwitchParser(
        name: "--case-sensitive", action: () =>
        {
            MakeRegex = (it) => new Regex(it, RegexOptions.None);
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
        var a2 = PrintDirOpt != PrintDir.Only;
        var matchFuncs = names
            .Where((it) => it.Length > 0)
            .Distinct()
            .Select((it) => ToWildMatch(it))
            .ToArray();
        if (matchFuncs.Length == 0) return;
        if (PrintDirOpt != PrintDir.Only)
        {
            CheckIfFileNameMatched = (it) => matchFuncs.Any((chk) => chk(it));
        }
        else
        {
            CheckIfDirNameMatched = (it) => matchFuncs.Any((chk) => chk(it));
        }
    }

    static internal readonly IInovke<string, bool> ExcludeFileName =
        new ParseInvoker<string, bool>("--excl", help: "FILE[;FILE ..]",
            init: Helper.Never,
            resolve: (parser, args) =>
            {
                var checkFuncs = Helper.CommonSplit(args)
                .Select((it) => ToWildMatch(it))
                .ToArray();
                parser.SetImplementation((arg) => checkFuncs.Any((chk) => chk(arg)));
            });

    static internal readonly IInovke<string, bool> ExcludeDirName =
        new ParseInvoker<string, bool>("--excl-dir", help: "DIR[;DIR ..]",
            init: Helper.Never,
            resolve: (parser, args) =>
            {
                var checkFuncs = Helper.CommonSplit(args)
                .Select((it) => ToWildMatch(it))
                .ToArray();
                parser.SetImplementation((arg) => checkFuncs.Any((chk) => chk(arg)));
            });

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

            // TODO: Various date-formats should be supported.
            if (DateTime.TryParse(arg, out DateTime valueDate))
            {
                return new WithData(valueDate);
            }

            throw new ArgumentException($"'{arg}' is bad to {name}");
        }
    }

    static internal Func<long, bool> IsMatchWithinSize
    { get; private set; } = Always<long>.True;

    static internal Func<DateTime, bool> IsMatchWithinDate
    { get; private set; } = Always<DateTime>.True;

    static internal readonly IParse Within = new SimpleParser(name: "--within",
            help: "SIZE | DATE",
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct()
                .Select((it) => (WithData.Parse(parser.Name, it),it))
                .GroupBy((it) => it.Item1.IsSize)
                .ToImmutableDictionary(
                    (grp) => grp.Key, (grp) => grp.AsEnumerable());

                if (aa.ContainsKey(true))
                {
                    var sizeWith = aa[true].Take(2).ToArray();
                    if (sizeWith.Length > 1)
                    {
                        throw new ArgumentException(
                            $"Too many Size '{sizeWith[0].Item2}', '{sizeWith[1].Item2}' to {parser.Name}");
                    }
                    var sizeMax = sizeWith[0].Item1.Size;
                    IsMatchWithinSize = (size) => (size <= sizeMax);
                }

                if (aa.ContainsKey(false))
                {
                    var dateWithin = aa[false].Take(2).ToArray();
                    if (dateWithin.Length > 1)
                    {
                        throw new ArgumentException(
                            $"Too many DateTime '{dateWithin[0].Item2}', '{dateWithin[1].Item2}' to {parser.Name}");
                    }
                    var dateMax = dateWithin[0].Item1.Date;
                    IsMatchWithinDate = (date) => (date <= dateMax);
                }
            });

    static internal Func<long, bool> IsMatchNotWithinSize
    { get; private set; } = Always<long>.True;

    static internal Func<DateTime, bool> IsMatchNotWithinDate
    { get; private set; } = Always<DateTime>.True;

    static internal readonly IParse NotWithin = new SimpleParser(name: "--not-within",
            help: "SIZE | DATE",
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct()
                .Select((it) => (WithData.Parse(parser.Name, it), it))
                .GroupBy((it) => it.Item1.IsSize)
                .ToImmutableDictionary(
                    (grp) => grp.Key, (grp) => grp.AsEnumerable());

                if (aa.ContainsKey(true))
                {
                    var sizeNotWith = aa[true].Take(2).ToArray();
                    if (sizeNotWith.Length > 1)
                    {
                        throw new ArgumentException(
                            $"Too many Size '{sizeNotWith[0].Item2}', '{sizeNotWith[1].Item2}' to {parser.Name}");
                    }
                    var sizeMin = sizeNotWith[0].Item1.Size;
                    IsMatchNotWithinSize = (size) => (size > sizeMin);
                }

                if (aa.ContainsKey(false))
                {
                    var dateNotWithin = aa[false].Take(2).ToArray();
                    if (dateNotWithin.Length > 1)
                    {
                        throw new ArgumentException(
                            $"Too many DateTime '{dateNotWithin[0].Item2}', '{dateNotWithin[1].Item2}' to {parser.Name}");
                    }
                    var dateMin = dateNotWithin[0].Item1.Date;
                    IsMatchNotWithinDate = (date) => (date > dateMin);
                }
            });
}
