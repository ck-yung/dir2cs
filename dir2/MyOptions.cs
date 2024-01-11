using System.Collections.Immutable;

namespace dir2;

static public partial class MyOptions
{
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
            var aa = args.Where((it) => it.Length > 0)
            .Distinct().Take(2).ToArray();
            if (aa.Length > 1)
            {
                throw new ArgumentException(
                    "Valid option: " + parser.Help + Environment.NewLine +
                    $"Too many values to {parser.Name}");
            }
            switch (aa[0])
            {
                case "both":
                    PrintDir = EnumPrint.FileAndDir;
                    parser.SetImplementation((path) => DefaultPrintDir(path));
                    break;
                case "only":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    PrintDirCount = (
                        count) => PrintDirCountWithCheck(count);
                    PrintDir = EnumPrint.OnlyDir;
                    parser.SetImplementation(
                        (path) => Helper.PrintDir(Helper.ToInfoDir(path)));
                    break;
                case "only-link":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    PrintDirCount = (
                        count) => PrintDirCountWithCheck(count);
                    PrintDir = EnumPrint.OnlyDir;
                    CheckDirLink = (info) => info.IsLinked;
                    parser.SetImplementation(
                        (path) => Helper.PrintDir(Helper.ToInfoDir(path)));
                    break;
                case "off":
                    PrintDir = EnumPrint.OnlyFile;
                    parser.SetImplementation(
                        (path) => Helper.GetFiles(Helper.ToInfoDir(path)));
                    break;
                case "tree":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    PrintDir = EnumPrint.DirTree;
                    parser.SetImplementation((arg) =>
                    Helper.PrintDirTree(arg));
                    break;
                default:
                    throw new ArgumentException(
                        "Valid option: " + parser.Help + Environment.NewLine +
                        $"Bad value '{aa[0]}' to {parser.Name}");
            }
        });

    static public readonly IInovke<string, InfoSum> SubDirOpt =
        new ParseInvoker<string, InfoSum>(name: "--sub",
            help: "off | all",
            init: (path) => PrintDirOpt.Invoke(path),
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
                if (aa.Length > 1)
                {
                    throw new ArgumentException(
                        "Valid option: " + parser.Help + Environment.NewLine +
                        $"Too many values to {parser.Name}");
                }

                switch (aa[0])
                {
                    case "off":
                        parser.SetImplementation((path) => PrintDirOpt.Invoke(path));
                        break;
                    case "all":
                        parser.SetImplementation((path) => impSubDir(path));
                        break;
                    default:
                        throw new ArgumentException(
                            "Valid option: "+ parser.Help + Environment.NewLine +
                            $"Bad value '{aa[0]}' to {parser.Name}");
                }
            });

    static void PrintDirCountWithCheck(int count, bool addNewLine = true,
        bool skipLessTwo = true)
    {
        switch (count, skipLessTwo)
        {
            case (<2, true):
                break;
            case (1, _):
                Helper.WriteLine("One dir is found.");
                if (addNewLine) Helper.WriteLine("");
                break;
            case (0, _):
                Helper.WriteLine("No dir is found.");
                if (addNewLine) Helper.WriteLine("");
                break;
            default:
                Helper.WriteLine($"{count} dirs are found.");
                if (addNewLine) Helper.WriteLine("");
                break;
        }
    }

    static public readonly IParse TotalOpt = new SimpleParser(name: "--total",
        help: "off | only | always", resolve: (parser, args) =>
        {
            var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
            if (aa.Length > 1)
                throw new ArgumentException($"Too many values to {parser.Name}");
            switch (aa[0])
            {
                case "off":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    PrintDirCount = Helper.DoNothing;
                    break;
                case "only":
                    Helper.ItemWrite = Helper.DoNothing;
                    Helper.ItemWriteLine = Helper.DoNothing;

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
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
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
                    foreach (var chThe in current.Substring(1))
                    {
                        yield return $"-{chThe}";
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
        Show.Opt,
        Show.HideOpt,
        Sum.Opt,
        (IParse) Helper.io.KeepDirOpt,
        Wild.WithinOpt,
        Wild.NotWithinOpt,
        Show.CreationDateOpt,
        Sort.ReverseOpt,
        Sort.TakeOpt,
        TotalOpt,
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
    };

    static public readonly IParse[] ExclFileDirParsers = Parsers
        .Where((it) => Wild.IsExclFeature(it))
        .ToArray();

    static internal ImmutableDictionary<string, string> ShortcutOptions
        = new Dictionary<string, string>()
        {
            ["-c"] = "--case-sensitive",
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
            ["-b"] = ("Brief path name", new[] {
                "--total", "off", "--hide", "date,size,count,mode,owner,link" }),
        }.ToImmutableDictionary();
}
