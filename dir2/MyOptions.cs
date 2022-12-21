using System.Collections.Generic;
using static dir2.MyOptions;

namespace dir2;

static public partial class MyOptions
{
    static public string[] Parse(IEnumerable<string> args)
    {
        var rtn = Parsers.Aggregate(
            seed: args.Select((it) => (false, it)),
            func: (acc, it) => it.Parse(acc))
            .Select((it) => it.Item2);
        if (rtn.Any()) return rtn.ToArray();
        return Array.Empty<string>();
    }

    static public readonly ImplicitBool ScanSubDir = new SwitchParser(name:"--sub");

    static public readonly IInovke<string, InfoSum> PrintDirOption =
        new ParseInvoker<string, InfoSum>(
        name: "--dir", help: "both | only | off",
        init: (path) =>
        {
            Helper.PrintDir(path);
            return Helper.PrintFile(path);
        }, resolve: (parser, args) =>
        {
            var aa = args.Where((it)=>it.Length>0).ToHashSet().ToArray();
            if (aa.Length > 1)
                throw new ArgumentException($"Too many values to {parser.Name}");
            switch (aa[0])
            {
                case "both": // default value
                    break;
                case "only":
                    Helper.impPrintSum = (_) => { };
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
                    parser.SetImplementation(Helper.PrintDir);
                    break;
                case "off":
                    parser.SetImplementation(Helper.PrintFile);
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
            return toKilo(arg2 / 1024.0F, index - 1);
        }
        return toKilo((float)arg, units.Length);
    }

    static public readonly IInovke<long, string> LengthFormat =
        new ParseInvoker<long, string>(name: "--size-format",
            help: "short | long | comma | eight",
            init: (value) => $"{value,8}", resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).ToHashSet().ToArray();
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
                var aa = args.Where((it) => it.Length > 0).ToHashSet().ToArray();
                if (aa.Length > 1)
                    throw new ArgumentException($"Too many values to {parser.Name}");
                Func<DateTime, string> rtn = (value) => value.ToString(aa[0]);
                _ = rtn(DateTime.MinValue);
                parser.SetImplementation(rtn);
            });

    static public IParse[] Parsers = new IParse[]
    {
        (IParse) ScanSubDir,
        (IParse) PrintDirOption,
        (IParse) LengthFormat,
        (IParse) DateFormat,
        Sort.Options,
    };
}
