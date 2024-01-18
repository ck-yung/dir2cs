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

        static internal void Init(IParse parser,
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
            CountLineMax = countToChangeBackground - 1;

            if (Enum.TryParse(typeof(ConsoleColor), colorNamePerCount, ignoreCase: true, out var result2))
            {
                Console.Error.WriteLine($"dbg: NamePerCount {colorNamePerCount}");
                BackgroundColorPerLineCount = (ConsoleColor)result2;
            }
            else
            {
                Console.Error.WriteLine($"'{colorNamePerCount}' is NOT a color name");
                throw new ShowSyntaxException(parser);
            }

            if (Enum.TryParse(typeof(ConsoleColor), colorNameTotalLine, ignoreCase: true, out var result3))
            {
                Console.Error.WriteLine($"dbg: totalLine {colorNameTotalLine}");
                BackgroundColorTotalLine = (ConsoleColor)result3;
            }
            else
            {
                Console.Error.WriteLine($"'{colorNamePerCount}' is NOT a color name");
                throw new ShowSyntaxException(parser);
            }
        }

        static void DoNothing() { }

        static void ChangePerLineCountBackground()
        {
            Console.BackgroundColor = BackgroundColorPerLineCount;
            NextUpdate = Restore;
        }

        static void Restore()
        {
            Console.ResetColor();
            NextUpdate = DoNothing;
        }

        static Action NextUpdate { get; set; } = DoNothing;

        static internal void UpdateBackground()
        {
            NextUpdate();
            CountLine += 1;
            if (CountLine > CountLineMax)
            {
                NextUpdate = ChangePerLineCountBackground;
                CountLine = -1;
            }
        }

        static internal void Reset()
        {
            Restore();
            CountLine = 1;
        }

        static internal void ChangeTotalLineBackgroundColor()
        {
            Console.ResetColor();
            Console.BackgroundColor = BackgroundColorTotalLine;
        }
    }

    static internal readonly IInovke<string, string> ColorOpt =
        new ParseInvoker<string, string>("--color",
            help: "INTEGER,COLOR,COLOR  (Check dir2 --color +?)",
            init: (it) => it, resolve: (parser, args) =>
            {
                var aa = Helper.GetUniqueTexts(args, 4, parser,
                    ignoreExtraHelp: true);

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
                    Color.Init(parser, lineCountToChangeBackgroundColor, aa[1], aa[2]);
                    parser.SetImplementation((Func<string, string>)((it) =>
                    {
                        Show.Color.UpdateBackground();
                        return it;
                    }));
                }
                else
                {
                    Console.Error.WriteLine($"'{aa[0]}' is NOT a number.");
                    throw new ShowSyntaxException(parser);
                }
            });
}
