using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dir2
{
    static public partial class MyOptions
    {
        internal abstract class ComplexParserBase : IParse
        {
            public string Name { get; init; }

            public string Help { get; init; }
            public Action<ComplexParserBase, IEnumerable<string>
                , ImmutableArray<string>> Resolve
            { get; init; }

            public ComplexParserBase(string name, string help,
                Action<ComplexParserBase, IEnumerable<string>
                    , ImmutableArray<string>> resolve)
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

                ImmutableArray<(bool, ArgType, string)> aa = groupThe.ContainsKey(false)
                    ? groupThe[false].ToImmutableArray()
                    : Enumerable.Empty<(bool, ArgType, string)>().ToImmutableArray();

                if (groupThe.ContainsKey(true))
                {
                    Resolve(this, groupThe[true].Select((it) => it.Item3)
                        , aa.Select((it) => it.Item3).ToImmutableArray<string>());
                }

                return aa.AsEnumerable();
            }
        }

        internal class ComplexParser : ComplexParserBase
        {
            public ComplexParser(string name,
                Action<ComplexParserBase, IEnumerable<string>
                    , ImmutableArray<string>> resolve,
                string help = "") : base(name, help, resolve)
            {
            }
        }
    }
}
