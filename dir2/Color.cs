using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static dir2.MyOptions;

namespace dir2;

static internal partial class Show
{
    static internal class Color
    {
        static int CountLineMax { get; set; } = 10;
        static int CountLine { get; set; } = 1;
        static ConsoleColor BackgroundColorPerLineCount = ConsoleColor.Black;
        static ConsoleColor BackgroundColorTotalLine = ConsoleColor.Black;

        static internal string[] GetColorNames() =>
            Enum.GetNames<ConsoleColor>().ToArray();

        static internal int Init(IParse parser,
            int countToChangeBackground,
            string colorNamePerCount,
            string colorNameTotalLine)
        {
            if (2 > countToChangeBackground)
            {
                Console.Error.WriteLine(
                    $"LineCountToChangeBackgroundColor {countToChangeBackground} is too small. The min is 2.");
                throw new ShowSyntaxException(parser);
            }
            CountLineMax = countToChangeBackground;
            System.Diagnostics.Debug.WriteLine($"dbg: Color.CountLineMax={CountLineMax}");

            if (Enum.TryParse(typeof(ConsoleColor), colorNamePerCount, ignoreCase: true, out var result2))
            {
                System.Diagnostics.Debug.WriteLine($"dbg: Color.NamePerCount {colorNamePerCount}");
                BackgroundColorPerLineCount = (ConsoleColor)result2;
            }
            else
            {
                Console.Error.WriteLine($"'{colorNamePerCount}' is NOT a color name");
                throw new ShowSyntaxException(parser);
            }

            if (Enum.TryParse(typeof(ConsoleColor), colorNameTotalLine, ignoreCase: true, out var result3))
            {
                System.Diagnostics.Debug.WriteLine($"dbg: Color.totalLine {colorNameTotalLine}");
                BackgroundColorTotalLine = (ConsoleColor)result3;
            }
            else
            {
                Console.Error.WriteLine($"'{colorNamePerCount}' is NOT a color name");
                throw new ShowSyntaxException(parser);
            }

            Reset = () =>
            {
                return "[###]";
            };

            TotalLine = () =>
            {
                return "[***]";
            };

            return CountLineMax;
        }

        static void DoNothing() { }

        static string ReturnBlank() => string.Empty;

        static internal IEnumerable<Func<string>> GetBlanks()
        {
            while (true) yield return ReturnBlank;
        }

        static internal IEnumerable<Func<string>> GetNumberTexts(int dbgMarker, int cntMax)
        {
            var cntThis = 0;
            while (true)
            {
                cntThis += 1;
                if (cntThis >= cntMax) cntThis = 0;
                yield return () => $"[{dbgMarker}:{cntThis}] ";
            }
        }

        static internal Func<string> Reset { get; private set; } = ReturnBlank;
        static internal Func<string> TotalLine { get; private set; } = ReturnBlank;
    }

    static internal readonly IInovke<int, IEnumerable<Func<string>>> ColorOpt =
        new ParseInvoker<int, IEnumerable<Func<string>>>("--color",
            help: "INTEGER,COLOR,COLOR  (Check dir2 --color +?)",
            init: (_) => Color.GetBlanks(), resolve: (parser, args) =>
            {
                var aa = args
                .Select((it) => it.Split(';', ','))
                .SelectMany((it) => it)
                .Where((it) => it.Length > 0)
                .Take(4)
                .ToArray();

                if (aa.Any((it) => it == Helper.ExtraHelp))
                {
                    Console.WriteLine("Color name:");
                    foreach (var a2 in Color.GetColorNames())
                    {
                        Console.WriteLine($"\t{a2}");
                    }
                    throw new ShowSyntaxException(parser);
                }

                if (aa.Length != 3)
                {
                    throw new ShowSyntaxException(parser);
                }

                if (int.TryParse(aa[0], out var lineCountToChangeBackgroundColor))
                {
                    var cntMax = Color.Init(parser, lineCountToChangeBackgroundColor, aa[1], aa[2]);
                    parser.SetImplementation((arg) => Color.GetNumberTexts(arg, cntMax));
                }
                else
                {
                    Console.Error.WriteLine($"'{aa[0]}' is NOT a number.");
                    throw new ShowSyntaxException(parser);
                }
            });
}
