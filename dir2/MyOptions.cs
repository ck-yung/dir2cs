using System.Collections.Generic;
using static dir2.MyOptions;

namespace dir2;

static public partial class MyOptions
{
    static public string[] Parse(IEnumerable<string> args)
    {
        return Parsers.Aggregate(
            seed: args.Select((it) => (false, it)),
            func: (acc, it) => it.Parse(acc))
            .Select((it) => it.Item2).ToArray()!;
    }

    static public readonly ImplicitBool ScanSubDir = new SwitchParser(name:"--sub");

    static public readonly IInovke<string, int> PrintDirOption =
        new ParseInvoker<string, int>(
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
                    parser.SetImplementation(Helper.PrintDir);
                    break;
                case "off":
                    parser.SetImplementation(Helper.PrintFile);
                    break;
                default:
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
            }
        });

    static public IParse[] Parsers = new IParse[] 
    {
        (IParse) ScanSubDir,
        (IParse) PrintDirOption,
        SortOptions!,
    };

    /// <summary>
    /// Implicit boolean, default false
    /// </summary>
    private class SwitchParser : ImplicitBool, IParse
    {
        public string Name { get; init; }

        public string Help { get; init; }

        public SwitchParser(string name, string help ="")
        {
            Name = name;
            Help = help;
        }

        public IEnumerable<(bool, string)> Parse(IEnumerable<(bool, string)> args)
        {
            var it = args.GetEnumerator();
            while (it.MoveNext())
            {
                var current = it.Current;
                if (current.Item2 == Name)
                {
                    Flag = true;
                }
                else
                {
                    yield return current;
                }
            }
        }
    }

    internal abstract class Parser: IParse
    {
        public string Name { get; init; }

        public string Help { get; init; }
        public Action<Parser, IEnumerable<string>>
            Resolve { get; init; }

        public Parser(string name, string help,
            Action<Parser, IEnumerable<string>> resolve)
        {
            Name = name;
            Help = help;
            Resolve = resolve;
        }

        public IEnumerable<(bool, string)> Parse(
            IEnumerable<(bool, string)> args)
        {
            IEnumerable<(bool, string)> ToFlagEnum()
            {
                var it = args.GetEnumerator();
                while (it.MoveNext())
                {
                    var current = it.Current;
                    if (current.Item2 != Name)
                    {
                        yield return it.Current;
                    }
                    else
                    {
                        if (!it.MoveNext())
                        {
                            throw new ArgumentException(
                                $"Missing value to {Name}");
                        }
                        yield return (true, it.Current.Item2);
                    }
                }
            }

            var groupThe = ToFlagEnum()
                .GroupBy((it) => it.Item1)
                .ToDictionary((it) => it.Key, (it) => it.AsEnumerable());

            if (groupThe.ContainsKey(true))
            {
                var argsFound = groupThe[true].Select((it) => it.Item2);
                Resolve(this, argsFound);
            }

            if (groupThe.ContainsKey(false)) return groupThe[false];
            return Enumerable.Empty<(bool, string)>();
        }
    }

    private class SimpleParser : Parser
    {
        public SimpleParser(string name,
            Action<Parser, IEnumerable<string>> resolve,
            string help = "") : base(name, help, resolve)
        {
        }
    }

    private class ParseInvoker<T,R>: Parser, IInovke<T,R>
    {
        protected Func<T, R> imp { get; private set; }
        public ParseInvoker(string name, Func<T,R> @init,
            Action<ParseInvoker<T, R>, IEnumerable<string>> resolve,
            string help = ""): base(name, help,
                resolve: (obj, args) =>
                resolve((ParseInvoker<T, R>) obj, args))
        {
            imp = @init;
        }

        public R Invoke(T arg)
        {
            return imp(arg);
        }

        public bool SetImplementation(Func<T, R> impNew)
        {
            if (impNew != null)
            {
                imp = impNew;
                return true;
            }
            return false;
        }
    }
}
