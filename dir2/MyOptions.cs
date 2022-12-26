using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace dir2;

static public partial class MyOptions
{
    static public string[] Resolve(this IParse[] parsers, IEnumerable<string> args)
    {
        var rtn = parsers.Aggregate(
            seed: args.Select((it) => (false, it)),
            func: (acc, it) => it.Parse(acc))
            .Select((it) => it.Item2);
        if (rtn.Any()) return rtn.Where((it) => it.Length>0).ToArray();
        return Array.Empty<string>();
    }

    static public readonly ImplicitBool SubDirOpt = new SwitchParser(name:"--sub");

    public enum EnumPrintDir
    {
        Both,
        Only,
        Off,
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
        name: "--dir", help: "both | only | off | tree",
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
                    PrintDir = EnumPrintDir.Only;
                    parser.SetImplementation(Helper.PrintDirTree);
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
            if (arg2 < 10_000.0F) return $"{arg2,4:F0}{units[index - 1]} ";
            if (index == 1) return $"{arg2,4:F0}{units[0]} ";
            return toKilo((arg2 + 512) / 1024.0F, index - 1);
        }
        return toKilo(arg, units.Length);
    }

    static public readonly IInovke<long, string> LengthFormatOpt =
        new ParseInvoker<long, string>(name: "--size-format", help: "short | comma;WIDTH",
            init: (it) => $"{it,8} ", resolve: (parser, args) =>
            {
                var pattern = @"\d+|comma|short";
                var aa = Helper.CommonSplit(args).OrderBy((it) => it).Take(4).ToArray();
                foreach (var a2 in aa)
                {
                    if (false == Regex.Match(a2, pattern, RegexOptions.None).Success)
                    {
                        throw new ArgumentException($"'{a2}' is bad to {parser.Name}");
                    }
                }

                if (aa.Contains("short"))
                {
                    if (aa.Length > 1) throw new ArgumentException($"Too many values to {parser.Name}");
                    parser.SetImplementation((val) => ToKiloUnit(val));
                    return;
                }

                if (aa.Length == 1)
                {
                    if (aa[0] == "comma")
                    {
                        parser.SetImplementation((it) => $"{it,8:N0} ");
                        return;
                    }
                    else
                    {
                        if (int.TryParse(aa[0], out int width))
                        {
                            if (width > 30)
                                throw new ArgumentException($"'{aa[0]}' is too largth width to {parser.Name}");
                            var fmtThe = $"{{0,{width}}} ";
                            parser.SetImplementation((it) => string.Format(fmtThe, it));
                            return;
                        }
                        throw new ArgumentException($"'{aa[0]}' is NOT width to {parser.Name}");
                    }
                }
                else if (2 == aa.Length && aa[1] == "comma")
                {
                    if (int.TryParse(aa[0], out int width))
                    {
                        if (width > 30)
                            throw new ArgumentException($"'{aa[0]}' is too largth width to {parser.Name}");
                        var fmtThe = $"{{0,{width}:N0}} ";
                        parser.SetImplementation((it) => string.Format(fmtThe, it));
                        return;
                    }
                    throw new ArgumentException($"'{aa[0]}' is NOT width to {parser.Name}");
                }
                else
                {
                    throw new ArgumentException($"Bad values is found to {parser.Name}");
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
        (IParse) SubDirOpt,
        (IParse) PrintDirOpt,
        Show.EncodeConsoleOpt,
        Wild.CaseSensitiveOpt,
        Wild.RegexOpt,
        (IParse) LengthFormatOpt,
        (IParse) Show.CountFormat,
        (IParse) Show.DateFormatOpt,
        (IParse) Wild.ExclFileNameOpt,
        (IParse) Wild.ExclDirNameOpt,
        (IParse) Wild.ExtensionOpt,
        (IParse) Helper.IsHiddenOpt,
        Sort.Opt,
        Show.Opt,
        Sum.Opt,
        (IParse) Helper.io.KeepDirOpt,
        Wild.WithinOpt,
        Wild.NotWithinOpt,
        Show.CreationDateOpt,
        Show.ReverseOpt,
        Show.TakeOpt,
        TotalOpt,
    };

    static public readonly IParse[] ConfigParsers = new IParse[]
    {
        Show.EncodeConsoleOpt,
        Wild.RegexOpt,
        Wild.CaseSensitiveOpt,
        (IParse) LengthFormatOpt,
        (IParse) Show.CountFormat,
        (IParse) Show.DateFormatOpt,
        (IParse) Helper.IsHiddenOpt,
        Sort.Opt,
    };

    static public readonly IParse[] ExclFileDirParsers = new IParse[]
    {
        (IParse) Wild.ExclFileNameOpt,
        (IParse) Wild.ExclDirNameOpt,
    };

    static internal ImmutableDictionary<string, string> ShortcutOptions
        = new Dictionary<string, string>()
        {
            ["-c"] = "--case-sensitive",
            ["-o"] = "--sort",
            ["-s"] = "--sub",
            ["-k"] = "--keep-dir",
            ["-r"] = "--reverse",
            ["-w"] = "--within",
            ["-W"] = "--not-within",
            ["-x"] = "--excl",
            ["-X"] = "--excl-dir",
        }.ToImmutableDictionary();

    static internal ImmutableDictionary<string, (string, string[])>
        ShortcutComplexOptions
        = new Dictionary<string, (string, string[])>
        {
            ["-f"] = ("File only", new[] { "--dir", "off" }),
            ["-d"] = ("Dir only", new[] { "--dir", "only" }),
            ["-t"] = ("", new[] { "--total", "only" }),
            ["-b"] = ("Brief path name", new[] {
                "--total", "off", "--hide", "date,size,count" }),
        }.ToImmutableDictionary();
}
