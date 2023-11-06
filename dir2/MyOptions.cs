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

    public enum EnumPrintDir
    {
        Both,
        Only,
        Off,
        Tree,
    };

    static public EnumPrintDir PrintDir { get; private set; } = EnumPrintDir.Both;
    static public void PrintDirOptBothOff()
    {
        if (PrintDir == EnumPrintDir.Both)
        {
            ((ParseInvoker<string, InfoSum>)PrintDirOpt).SetImplementation(Helper.GetFiles);
        }
    }

    static public readonly IInovke<string, InfoSum> PrintDirOpt =
        new ParseInvoker<string, InfoSum>(
        name: "--dir", help: "all | only | off | tree",
        init: (path) =>
        {
            Helper.PrintDir(path);
            return Helper.GetFiles(path);
        }, resolve: (parser, args) =>
        {
            var aa = args.Where((it)=>it.Length>0).Distinct().Take(2).ToArray();
            if (aa.Length > 1)
                throw new ArgumentException($"Too many values to {parser.Name}");
            switch (aa[0])
            {
                case "both":
                    PrintDir = EnumPrintDir.Both;
                    parser.SetImplementation((path) =>
                    {
                        Helper.PrintDir(path);
                        return Helper.GetFiles(path);
                    });
                    break;
                case "only":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    Helper.impPrintDirCount = (cnt) =>
                    {
                        if (cnt==0)
                        {
                            Helper.WriteLine("No dir is found.");
                        }
                        else if (cnt>1)
                        {
                            Helper.WriteLine($"{cnt} dir are found.");
                        }
                    };
                    PrintDir = EnumPrintDir.Only;
                    parser.SetImplementation(Helper.PrintDir);
                    break;
                case "off":
                    PrintDir = EnumPrintDir.Off;
                    parser.SetImplementation(Helper.GetFiles);
                    break;
                case "tree":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    PrintDir = EnumPrintDir.Tree;
                    parser.SetImplementation(Helper.PrintDirTree);
                    break;
                default:
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
            }
        });

    static public readonly IInovke<string, InfoSum> SubDirOpt =
        new ParseInvoker<string, InfoSum>(name: "--sub",
            help: "off | all | only-link | excl-link",
            init: (path) => PrintDirOpt.Invoke(path),
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
                if (aa.Length > 1)
                    throw new ArgumentException($"Too many values to {parser.Name}");

                switch (aa[0])
                {
                    case "off":
                        parser.SetImplementation((path) => PrintDirOpt.Invoke(path));
                        break;
                    case "all":
                        IsFakeDirOrLinked = Helper.Never; ;
                        parser.SetImplementation((path) => impSubDir(path));
                        break;
                    case "only-link":
                        IsFakeDirOrLinked = (path) =>
                        {
                            var info = Helper.ToInfoDir(path);
                            if (false == info.IsNotFake()) return true;
                            return string.IsNullOrEmpty(info.LinkTarget);
                        };
                        parser.SetImplementation((path) => impSubDir(path));
                        break;
                    case "excl-link":
                        IsFakeDirOrLinked = (path) =>
                        {
                            var info = Helper.ToInfoDir(path);
                            if (false == info.IsNotFake()) return true;
                            return false == string.IsNullOrEmpty(info.LinkTarget);
                        };
                        parser.SetImplementation((path) => impSubDir(path));
                        break;
                    default:
                        throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
                }
            });

    static public readonly IParse TotalOpt = new SimpleParser(name: "--total",
        help: "off | only", resolve: (parser, args) =>
        {
            var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
            if (aa.Length > 1)
                throw new ArgumentException($"Too many values to {parser.Name}");
            switch (aa[0])
            {
                case "off":
                    Helper.impPrintInfoTotal = InfoSum.DoNothing;
                    Helper.impPrintDirCount = Helper.DoNothing;
                    break;
                case "only":
                    Helper.ItemWrite = Helper.DoNothing;
                    Helper.ItemWriteLine = Helper.DoNothing;

                    Helper.impPrintDirCount = (cntDir) =>
                    {
                        switch (cntDir)
                        {
                            case 0:
                                Helper.WriteLine("No dir is found.");
                                break;
                            case 1:
                                Helper.WriteLine("One dir is found.");
                                break;
                            default:
                                Helper.WriteLine($"{cntDir} dirs are found.");
                                break;
                        }
                    };

                    if (PrintDir != EnumPrintDir.Only)
                    {
                        Helper.impPrintInfoTotal =
                            (arg) => Helper.PrintIntoTotalWithFlag(arg, printEvenCountOne: true);
                    }
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
        (IParse) Wild.ExclFileNameOpt,
        (IParse) Wild.ExclDirNameOpt,
        (IParse) Wild.ExtensionOpt,
        (IParse) Helper.IsHiddenFileOpt,
        (IParse) Helper.IsLinkFileOpt,
        Sort.Opt,
        Show.Opt,
        Show.HideOpt,
        Sum.Opt,
        (IParse) Helper.io.KeepDirOpt,
        Wild.NotWithinOpt,
        Wild.WithinOpt,
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
        (IParse) Helper.IsLinkFileOpt,
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
            ["-H"] = ("", new[] { "--excl-none", "--hidden", "only"}),
            ["-s"] = ("Scan all sub dir", new[] { "--sub", "all" }),
            ["-f"] = ("File only", new[] { "--dir", "off" }),
            ["-d"] = ("Dir only", new[] { "--dir", "only" }),
            ["-b"] = ("Brief path name", new[] {
                "--total", "off", "--hide", "date,size,count,mode,owner,link" }),
        }.ToImmutableDictionary();
}
