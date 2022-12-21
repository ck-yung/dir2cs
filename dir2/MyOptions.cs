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

    static public IFlag ScanSubDir = new SwitchParser(name:"--sub");

    static public IInovke<string, int> PrintDirOption =
        new ParseInvoker<string,int>(
        name: "--dir", help: "both | only | off",
        init: (path) =>
        {
            Helper.PrintDir(path);
            return Helper.PrintFile(path);
        }, parse: (parser, args) =>
        {
            var aa = args.Where((it)=>it.Length>0).ToHashSet().ToArray();
            if (aa.Length > 1)
                throw new ArgumentException($"Too many values to {parser.Name}");
            switch (aa[0])
            {
                case "both": // default value
                    break;
                case "only":
                    parser.SetImplementation((path) => Helper.PrintDir(path));
                    break;
                case "off":
                    parser.SetImplementation((path) => Helper.PrintFile(path));
                    break;
                default:
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
            }
        });

    static public IParse[] Parsers = new IParse[] 
    {
        (IParse) ScanSubDir,
        (IParse) PrintDirOption,
    };

    static MyOptions()
    {
    }

    private class SwitchParser : IParse, IFlag
    {
        public string Name { get; init; }

        public string Help { get; init; }
        public bool Flag { get; private set; } = false;

        public SwitchParser(string name, string help ="")
        {
            Name = name;
            Help = help;
        }
        public IEnumerable<(bool, string)> Parse(IEnumerable<(bool, string)> args)
        {
            IEnumerable<(bool, string)> ToFlagEnum()
            {
                var it = args.GetEnumerator();
                while (it.MoveNext())
                {
                    var current = it.Current;
                    if (current.Item2 == Name)
                    {
                        yield return (true, Name);
                    }
                    else
                    {
                        yield return it.Current;
                    }
                }
            }

            var groupThe = ToFlagEnum()
                .GroupBy((it) => it.Item1)
                .ToDictionary((it) => it.Key, (it) => it.AsEnumerable());

            if (groupThe.ContainsKey(true))
            {
                Flag = true;
            }

            if (groupThe.ContainsKey(false)) return groupThe[false];
            return Enumerable.Empty<(bool, string)>();
        }
    }

    internal abstract class Parser: IParse
    {
        public string Name { get; init; }

        public string Help { get; init; }
        public Action<Parser, IEnumerable<string>> ParseAction { get; init; }

        public Parser(string name, string help,
            Action<Parser, IEnumerable<string>> parse)
        {
            Name = name;
            Help = help;
            ParseAction = parse;
        }

        public IEnumerable<(bool, string)> Parse(IEnumerable<(bool, string)> args)
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
                ParseAction(this, argsFound);
            }

            if (groupThe.ContainsKey(false)) return groupThe[false];
            return Enumerable.Empty<(bool, string)>();
        }
    }

    internal class Invoker<T,R>: Parser
    {
        public Func<T, R> imp { get; private set; }

        public Invoker(string name, Func<T,R> @init,
            Action<Parser,IEnumerable<string>> parse,
            string help = ""
            ) : base(name, help, parse)
        {
            imp = @init;
        }
    }

    private class ParseInvoker<T,R>: Parser, IInovke<T,R>
    {
        protected Func<T, R> imp { get; private set; }
        public ParseInvoker(string name, Func<T,R> @init,
            Action<ParseInvoker<T, R>, IEnumerable<string>> parse,
            string help = ""): base(name, help,
                parse: (obj, args) => parse( (ParseInvoker<T, R>) obj, args))
        {
            imp = @init;
        }

        public R Invoke(T arg)
        {
            return imp(arg);
        }

        public void SetImplementation(Func<T, R> impNew)
        {
            imp = impNew;
        }
    }
}
