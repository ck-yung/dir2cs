using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using static dir2.SummaryInfo;

namespace dir2;

static public partial class MyOptions
{
    const string ConfigPrefix = "Config.";
    public static string ThousandSeparator { get; private set; } = ",";

    [GeneratedRegex(
        @"^Config.Thousand.Separator(=|\s*=\s*)(?<Text>.)$",
        RegexOptions.IgnoreCase)]
    private static partial Regex RegThousandSeparator();

    [GeneratedRegex(
        @"^Config.Thousand.Separator(=|\s*=\s*)%20$",
        RegexOptions.IgnoreCase)]
    private static partial Regex RegThousandSeparatorSpace();

    static public IEnumerable<string> Init(IEnumerable<string> lines)
    {
        var prefixGroupBy = lines
            .GroupBy((it) => it.StartsWith(ConfigPrefix))
            .ToImmutableDictionary((grp) => grp.Key, (grp) => grp);
        if (prefixGroupBy.TryGetValue(true, out var matched))
        {
            var regThousandSep = RegThousandSeparator();
            var thousSepGroup = matched
                .Select((it) => new
                {
                    Text = it,
                    Match = regThousandSep.Match(it),
                })
                .GroupBy((it) => it.Match.Success)
                .ToImmutableDictionary((grp) => grp.Key, (grp) => grp);
            if (thousSepGroup.TryGetValue(true, out var thSeps))
            {
                var aa = thSeps.Select((it) => it.Match.Groups["Text"].Value)
                    .Distinct().Take(2).ToArray();
                if (aa.Length == 1)
                {
                    ThousandSeparator = aa[0];
                }
                else if (aa.Length > 1)
                {
                    // TODO: Only report too many different values
                }
            }
            else if (thousSepGroup.TryGetValue(false, out var notSeps))
            {
                var regThousSepSpace = RegThousandSeparatorSpace();
                if (notSeps
                    .Select((it) => it.Text)
                    .Select((it) => regThousSepSpace.Match(it))
                    .Where((it) => it.Success)
                    .Any())
                {
                    ThousandSeparator = " ";
                }
            }
        }
        if (prefixGroupBy.TryGetValue(false, out var notMatched))
        {
            return notMatched;
        }
        return Array.Empty<string>();
    }

    static public IEnumerable<(ArgType, string)> Resolve(this IParse[] parsers,
        IEnumerable<(ArgType, string)> args, bool isIncludeExclNameOptions)
    {
        var parsersThe = parsers;
        if (false == isIncludeExclNameOptions)
        {
            parsersThe = parsers
                .Where((it) => false == Wild.IsExclFeature(it))
                .ToArray();
        }

        return parsersThe.Aggregate(
            seed: args.Select((it) => (false, it.Item1, it.Item2)),
            func: (acc, it) => it.Parse(acc))
            .Select((it) => (it.Item2, it.Item3));
    }

    public enum EnumPrint
    {
        FileAndDir,
        OnlyDir,
        OnlyFile,
        DirTree,
    };

    static public EnumPrint PrintDir { get; private set; } = EnumPrint.FileAndDir;
    static public Func<InfoDir, bool> CheckDirLink { get; internal set; }
        = Always<InfoDir>.True;

    static InfoSum DefaultPrintDir(string path)
    {
        var infoDir = Helper.ToInfoDir(path);
        Helper.PrintDir(infoDir);
        return Helper.GetFiles(infoDir);
    }

    static internal Action<int> PrintDirCount { get; private set; } =
        (count) => PrintDirCountWithCheck(count);

    static public readonly IInovke<string, InfoSum> PrintDirOpt =
        new ParseInvoker<string, InfoSum>(
        name: "--dir", help: "both | off | only | only-link | tree",
        init: (path) => DefaultPrintDir(path),
        resolve: (parser, args) =>
        {
            var argThe = Helper.GetUnique(args, parser);
            switch (argThe.ToLower())
            {
                case "both":
                    PrintDir = EnumPrint.FileAndDir;
                    parser.SetImplementation((path) => DefaultPrintDir(path));
                    break;
                case "only":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    PrintDirCount = (count) => PrintDirCountWithCheck(count);
                    PrintDir = EnumPrint.OnlyDir;
                    parser.SetImplementation((path) => Helper.PrintDir(Helper.ToInfoDir(path)));
                    break;
                case "only-link":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    PrintDirCount = (count) => PrintDirCountWithCheck(count);
                    PrintDir = EnumPrint.OnlyDir;
                    CheckDirLink = (info) => info.IsLinked;
                    parser.SetImplementation((path) => Helper.PrintDir(Helper.ToInfoDir(path)));
                    break;
                case "off":
                    PrintDir = EnumPrint.OnlyFile;
                    parser.SetImplementation((path) => Helper.GetFiles(Helper.ToInfoDir(path)));
                    break;
                case "tree":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    PrintDir = EnumPrint.DirTree;
                    parser.SetImplementation((arg) =>
                    Helper.PrintDirTree(arg));
                    break;
                default:
                    throw new ConfigException(
                        "Valid option: " + parser.Help + Environment.NewLine +
                        $"Bad value '{argThe}' to {parser.Name}");
            }
        });

    static public readonly IInovke<string, InfoSum> SubDirOpt =
        new ParseInvoker<string, InfoSum>(name: "--sub",
            help: "off | all",
            init: (path) => PrintDirOpt.Invoke(path),
            resolve: (parser, args) =>
            {
                var argThe = Helper.GetUnique(args, parser);
                switch (argThe.ToLower())
                {
                    case "off":
                        parser.SetImplementation((path) => PrintDirOpt.Invoke(path));
                        break;
                    case "all":
                        parser.SetImplementation((path) => impSubDir(path));
                        break;
                    default:
                        throw new ConfigException(
                            "Valid option: "+ parser.Help + Environment.NewLine +
                            $"Bad value '{argThe}' to {parser.Name}");
                }
            });

    static void PrintDirCountWithCheck(int count, bool addNewLine = true,
        bool skipLessTwo = true)
    {
        switch (count, skipLessTwo)
        {
            case (<2, true):
                return;
            case (1, _):
                Helper.WriteTotalLine(Text(FixedText.OneDir), addNewLine);
                break;
            case (0, _):
                Helper.WriteTotalLine(Text(FixedText.ZeroDir), addNewLine);
                break;
            default:
                Helper.WriteTotalLine(Format(StringFormat.DirOther, count),
                    addNewLine); ;
                break;
        }
    }

    static public readonly IParse TotalOpt = new SimpleParser(name: "--total",
        help: "off | only | always", resolve: (parser, args) =>
        {
            var argThe = Helper.GetUnique(args, parser);
            switch (argThe.ToLower())
            {
                case "off":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    PrintDirCount = Helper.DoNothing;
                    break;

                case "only":
                    Helper.ItemWrite = Helper.ReturnEmptyString;
                    Helper.ItemWriteLine = Helper.ReturnEmptyString;

                    PrintDirCount = (cntDir) => PrintDirCountWithCheck(
                        cntDir, addNewLine: false, skipLessTwo: false);

                    if (PrintDir != EnumPrint.OnlyDir)
                    {
                        Helper.impPrintInfoTotal =
                            (path, wilds, arg) => Helper.PrintIntoTotalWithFlag(
                                path, wilds, arg, printEvenCountOne: true);
                    }
                    break;

                case "always":
                    PrintDirCount = (cntDir) => PrintDirCountWithCheck(
                        cntDir, skipLessTwo: false);

                    Helper.impPrintInfoTotal =
                        (path, wilds, arg) => Helper.PrintIntoTotalWithFlag(
                           path, wilds, arg, printEvenCountOne: true);
                    break;

                default:
                    throw new ConfigException($"Bad value '{argThe}' to {parser.Name}");
            }
        });

    static public IEnumerable<string> ExpandFromShortCut(IEnumerable<string> args)
    {
        IEnumerable<string> ExpandCombiningShortCut()
        {
            var it = args.GetEnumerator();
            while (it.MoveNext())
            {
                var current = it.Current;
                if (current.StartsWith("--"))
                {
                    yield return current;
                }
                else if (current.Length < 3 || current[0] != '-')
                {
                    yield return current;
                }
                else
                {
                    if (current.Length > 2 &&
                        current[1] != ',' && current[1] != ';' &&
                        (current[1] < '0' || current[1] > '9'))
                    {
                        foreach (var chThe in current.Substring(1))
                        {
                            yield return $"-{chThe}";
                        }
                    }
                    else
                    {
                        yield return current;
                    }
                }
            }
        }

        var it2 = ExpandCombiningShortCut().GetEnumerator();
        while (it2.MoveNext())
        {
            var current = it2.Current;
            if (ShortcutOptions.TryGetValue(current, out string found))
            {
                yield return found;
            }
            else if (ShortcutExpandOptions.TryGetValue(current,
                out var a2))
            {
                if (it2.MoveNext())
                {
                    current = it2.Current;
                    foreach (var opt in a2.Item2(current))
                        yield return opt;
                }
                else
                {
                    throw ConfigException.MissingValue(current);
                }
            }
            else if (ShortcutComplexOptions.TryGetValue(current,
                out (string, string[]) founds))
            {
                foreach (var opt in founds.Item2)
                    yield return opt;
            }
            else
            {
                yield return current;
            }
        }
    }

    static public readonly IParse[] Parsers = new IParse[]
    {
        (IParse) Show.OutputOpt,
        (IParse) PrintDirOpt,
        (IParse) SubDirOpt,
        Show.EncodeConsoleOpt,
        Wild.CaseSensitiveOpt,
        Wild.RegexOpt,
        (IParse) Show.LengthFormatOpt,
        (IParse) Show.CountFormat,
        (IParse) Helper.DateFormatOpt,
        (IParse) Helper.IsLinkFileOpt,
        (IParse) Wild.ExclFileNameOpt,
        (IParse) Wild.ExclDirNameOpt,
        (IParse) Wild.ExtensionOpt,
        (IParse) Helper.IsHiddenFileOpt,
        Sort.Opt,
        Sum.Opt,
        Show.Opt,
        Show.HideOpt,
        (IParse) Helper.Io.KeepDirOpt,
        Wild.WithinOpt,
        Wild.NotWithinOpt,
        Show.CreationDateOpt,
        Sort.ReverseOpt,
        Sort.TakeOpt,
        TotalOpt,
        (IParse) Show.EndTime,
        (IParse) Show.ColorOpt,
        (IParse) Show.PauseOpt,
    };

    static public readonly IParse[] ConfigParsers = new IParse[]
    {
        Sort.Opt,
        Show.Opt,
        Show.HideOpt,
        Show.EncodeConsoleOpt,
        Wild.RegexOpt,
        Wild.CaseSensitiveOpt,
        (IParse) Show.LengthFormatOpt,
        (IParse) Show.CountFormat,
        (IParse) Helper.DateFormatOpt,
        (IParse) Helper.IsHiddenFileOpt,
        Sort.ReverseOpt,
        (IParse) Show.EndTime,
        (IParse) Show.ColorOpt,
        (IParse) Show.PauseOpt,
    };

    static public readonly IParse[] ExclFileDirParsers = Parsers
        .Where((it) => Wild.IsExclFeature(it))
        .ToArray();

    static internal ImmutableDictionary<string, string> ShortcutOptions
        = new Dictionary<string, string>()
        {
            ["-c"] = "--case-sensitive",
            ["-D"] = "--date-format",
            ["-o"] = "--sort",
            ["-k"] = "--keep-dir",
            ["-w"] = "--within",
            ["-W"] = "--not-within",
            ["-x"] = "--excl",
            ["-X"] = "--excl-dir",
        }.ToImmutableDictionary();

    static internal ImmutableDictionary<string, (string, string[])>
        ShortcutComplexOptions
        = new Dictionary<string, (string, string[])>
        {
            ["-r"] = ("", new[] { "--reverse", "on" }),
            ["-t"] = ("", new[] { "--total", "only" }),
            ["-R"] = ("", new[] { "--dir", "tree" }),
            ["-H"] = ("", new[] { "--hidden", "only", "--excl-none", "--excl", ":link", "--excl-dir", ":link"}),
            ["-s"] = ("Scan all sub dir", new[] { "--sub", "all" }),
            ["-f"] = ("File only", new[] { "--dir", "off" }),
            ["-d"] = ("Dir only", new[] { "--dir", "only" }),
            ["-p"] = ("", new[] { "--pause", "on" }),
            ["-b"] = ("Brief path name", new[] {
                "--total", "off", "--hide", "date,size,count,mode,owner,link" }),
        }.ToImmutableDictionary();

    static internal ImmutableDictionary<string, (string, Func<string, string[]>, string)>
        ShortcutExpandOptions
        = new Dictionary<string, (string, Func<string, string[]>, string)>
        {
            ["-Z"] = ("Time Zone", (arg) =>
            {
                if (false == string.IsNullOrEmpty(arg)
                && arg[0]!='+' && arg[0]!='-')
                {
                    return ["--date-format", "utc+" + arg];
                }
                return ["--date-format", "utc" + arg];
            }, "For example, -Z +08 => --date-format utc+08"),
        }.ToImmutableDictionary();

    static internal string GetShortCut(string name)
    {
        var finding = ShortcutOptions
            .Select((it) => new
            {
                Found = (it.Value == name),
                it.Key
            })
            .FirstOrDefault((it) => it.Found);
        if (finding?.Found ?? false)
        {
            return finding.Key;
        }
        return string.Empty;
    }

    static internal string GetHelpText(IParse parser)
    {
        var rtn = new StringBuilder();
        rtn.Append($"Syntax: {nameof(dir2)} {parser.Name,-16} {parser.Help}");
        var shortcut = GetShortCut(parser.Name);
        if (false == string.IsNullOrEmpty(shortcut))
        {
            rtn.AppendLine();
            // ....... $"Syntax: {nameof(
            rtn.Append($"Or:     {nameof(dir2)} {shortcut,-16} {parser.Help}");
        }
        return rtn.ToString();
    }
}
