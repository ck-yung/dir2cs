using System.Collections.Immutable;

namespace dir2;

static public partial class MyOptions
{
    static public string[] Parse(IEnumerable<string> args)
    {
        var rtn = Parsers.Aggregate(
            seed: args.Select((it) => (false, it)),
            func: (acc, it) => it.Parse(acc))
            .Select((it) => it.Item2);
        if (rtn.Any()) return rtn.Where((it) => it.Length>0).ToArray();
        return Array.Empty<string>();
    }

    static public readonly ImplicitBool ScanSubDir = new SwitchParser(name:"--sub");

    public enum PrintDir
    {
        Both,
        Only,
        Off,
    };

    static public PrintDir PrintDirOpt { get; private set; } = PrintDir.Both;
    static public void PrintDirOptBothOff()
    {
        if (PrintDirOpt == PrintDir.Both)
        {
            ((ParseInvoker<string, InfoSum>)PrintDirOption).SetImplementation(Helper.GetFiles);
        }
    }

    static public readonly IInovke<string, InfoSum> PrintDirOption =
        new ParseInvoker<string, InfoSum>(
        name: "--dir", help: "both | only | off",
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
                    PrintDirOpt = PrintDir.Both;
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
                    PrintDirOpt = PrintDir.Only;
                    parser.SetImplementation(Helper.PrintDir);
                    break;
                case "off":
                    PrintDirOpt = PrintDir.Off;
                    parser.SetImplementation(Helper.GetFiles);
                    break;
                default:
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
            }
        });

    static public string ToKiloUnit(long arg)
    {
        var units = new char[] { 'T', 'G', 'M', 'K', ' ' };
        string toKilo(float arg2, int index)
        {
            if (arg2 < 10_000.0F) return $"{arg2,4:F0}{units[index - 1]}";
            if (index == 1) return $"{arg2,4:F0}{units[0]}";
            return toKilo((arg2 + 512) / 1024.0F, index - 1);
        }
        return toKilo(arg, units.Length);
    }

    static public readonly IInovke<long, string> LengthFormat =
        new ParseInvoker<long, string>(name: "--size-format",
            help: "short | long | comma | eight",
            init: (value) => $"{value,8}", resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
                if (aa.Length > 1)
                    throw new ArgumentException($"Too many values to {parser.Name}");
                switch (aa[0])
                {
                    case "short":
                        parser.SetImplementation((value) => ToKiloUnit(value));
                        break;
                    case "long":
                        parser.SetImplementation((value) => $"{value,12}");
                        break;
                    case "comma":
                        parser.SetImplementation((value) => $"{value,19:N0}");
                        break;
                    case "eight":
                        parser.SetImplementation((value) => $"{value,8}");
                        break;
                    default:
                        throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
                }
            });

    static public readonly IInovke<DateTime, string> DateFormat =
        new ParseInvoker<DateTime, string>(name: "--date-format",
            help: "DATE-FORMAT",
            init: (value) => $"{value:yyyy-MM-dd HH:mm:ss}",
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
                if (aa.Length > 1)
                    throw new ArgumentException($"Too many values to {parser.Name}");
                Func<DateTime, string> rtn = (value) => value.ToString(aa[0]);
                _ = rtn(DateTime.MinValue);
                parser.SetImplementation(rtn);
            });

    static public readonly IParse TotalOption = new SimpleParser(name: "--total",
        help: "off | only", resolve: (parser, args) =>
        {
            var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
            if (aa.Length > 1)
                throw new ArgumentException($"Too many values to {parser.Name}");
            switch (aa[0])
            {
                case "off":
                    Helper.impPrintInfoTotal = Helper.DoNothing;
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

                    if (InfoSum.IsNothing(Helper.impPrintInfoTotal))
                    {
                        Helper.impPrintInfoTotal =
                            (arg) => Helper.PrintIntoTotalWithFlag(arg, printEvenCountOne: true);
                    }
                    break;
                default:
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
            }
        });

    static public IParse[] Parsers = new IParse[]
    {
        (IParse) ScanSubDir,
        (IParse) PrintDirOption,
        (IParse) LengthFormat,
        (IParse) DateFormat,
        (IParse) Wild.ExcludeFileName,
        (IParse) Wild.ExcludeDirName,
        Sort.Options,
        Show.Options,
        Sum.Options,
        (IParse) Helper.io.KeepDirOpt,
        Wild.Within,
        Wild.NotWithin,
        TotalOption,
    };

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
            if (ShortcutOptions.TryGetValue(current, out (string, string[]) found))
            {
                foreach (var opt in found.Item2)
                    yield return opt;
            }
            else
            {
                yield return current;
            }
        }
    }

    static internal ImmutableDictionary<string, (string, string[])> ShortcutOptions
        = new Dictionary<string, (string, string[])>
        {
            ["-o"] = ("", new[] { "--sort" }),
            ["-s"] = ("Scan sub dir", new[] { "--sub" }),
            ["-f"] = ("File only", new[] { "--dir", "off" }),
            ["-d"] = ("Dir only", new[] { "--dir", "only" }),
            ["-k"] = ("", new[] { "--keep-dir" }),
            ["-t"] = ("", new[] { "--total", "only" }),
            ["-b"] = ("Brief path name", new[] { "--total", "off", "--hide", "date,size,count" }),
            ["-w"] = ("Select size, date-time", new[] { "--within" }),
            ["-W"] = ("Select size, date-time", new[] { "--not-within" }),
            ["-x"] = ("Excluding file name", new[] { "--excl" }),
            ["-X"] = ("Excluding dir name", new[] { "--excl-dir" }),
        }.ToImmutableDictionary();
}
