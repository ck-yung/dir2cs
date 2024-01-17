using System.Collections.Immutable;

namespace dir2;

static public partial class MyOptions
{

    /// <summary>
    /// Implicit boolean, default false
    /// </summary>
    internal class SwitchParser : ImplicitBool, IParse
    {
        public string Name { get; init; }

        public string Help { get; init; }
        public readonly Action Action;

        public SwitchParser(string name, string help = "")
        {
            Name = name;
            Help = help;
            Action = () => { };
        }

        public SwitchParser(string name, Action action, string help = "")
        {
            Name = name;
            Help = help;
            Action = action;
        }

        public IEnumerable<(bool, ArgType, string)> Parse(
            IEnumerable<(bool, ArgType, string)> args)
        {
            bool notYetActed = true;
            var it = args.GetEnumerator();
            while (it.MoveNext())
            {
                var current = it.Current;
                if (current.Item3 == Name)
                {
                    Flag = true;
                    if (notYetActed)
                    {
                        notYetActed = false;
                        Action();
                    }
                }
                else
                {
                    yield return current;
                }
            }
        }
    }

    internal abstract class Parser : IParse
    {
        public string Name { get; init; }

        public string Help { get; init; }
        public Action<Parser, IEnumerable<string>>
            Resolve
        { get; init; }

        public Parser(string name, string help,
            Action<Parser, IEnumerable<string>> resolve)
        {
            Name = name;
            Help = help;
            Resolve = resolve;
        }

        public IEnumerable<(bool, ArgType, string)> Parse(
            IEnumerable<(bool, ArgType, string)> args)
        {
            IEnumerable<(bool, ArgType, string)> ToFlagEnum()
            {
                var it = args.GetEnumerator();
                while (it.MoveNext())
                {
                    var current = it.Current;
                    if (current.Item3 != Name)
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
                        yield return
                            (true, it.Current.Item2, it.Current.Item3);
                    }
                }
            }

            var groupThe = ToFlagEnum()
                .GroupBy((it) => it.Item1)
                .ToImmutableDictionary((it) => it.Key, (it) => it.AsEnumerable());

            if (groupThe.TryGetValue(true, out var matches))
            {
                Resolve(this, matches.Select((it) => it.Item3));
            }

            if (groupThe.TryGetValue(false, out var notMatches))
            {
                return notMatches;
            }

            return Enumerable.Empty<(bool, ArgType, string)>();
        }
    }

    internal class SimpleParser : Parser
    {
        public SimpleParser(string name,
            Action<Parser, IEnumerable<string>> resolve,
            string help = "") : base(name, help, resolve)
        {
        }
    }

    internal class ParseInvoker<T, R> : Parser, IInovke<T, R>
    {
        protected Func<T, R> imp { get; private set; }
        public ParseInvoker(string name, Func<T, R> @init,
            Action<ParseInvoker<T, R>, IEnumerable<string>> resolve,
            string help = "") : base(name, help,
                resolve: (obj, args) =>
                resolve((ParseInvoker<T, R>)obj, args))
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

    internal class ExclFeauture<T, R> : ParseInvoker<T, R>
    {
        public ExclFeauture(string name
            , Func<T, R> init
            , Action<ParseInvoker<T, R>, IEnumerable<string>> resolve
            , string help = ""
            ) : base(name, init, resolve, help)
        {
        }
    }
}
