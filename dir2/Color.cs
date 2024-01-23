﻿using System;
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
        static Color()
        {
            ForegroundColorDefault = Console.ForegroundColor;
            ForegroundColorSwitch = Console.ForegroundColor;
        }

        static internal string[] GetColorNames() =>
            Enum.GetNames<ConsoleColor>().ToArray();

        static internal int Init(IParse parser,
            int countToChangeBackground,
            string colorNamePerCount,
            string colorNameTotalLine)
        {
            if (2 > countToChangeBackground)
            {
                throw new ConfigException(
                    $"Line counter {countToChangeBackground} (to {parser.Name}) MUST be greater 1.");
            }
            CountLineMax = countToChangeBackground;

            if (Enum.TryParse(typeof(ConsoleColor), colorNamePerCount,
                ignoreCase: true, out var result2))
            {
                ForegroundColorPerLineCount = (ConsoleColor)result2;
            }
            else
            {
                throw new ConfigException(
                    $"'{colorNamePerCount}' (to {parser.Name}) is NOT color name.");
            }

            ForegroundColorSwitch = ForegroundColorPerLineCount;
            SwitchFore = (it) => SwitchForeColorBack(it, switchBackuground: false);
            ResetFore = () =>
            {
                Console.ForegroundColor = ForegroundColorDefault;
                SwitchFore = (it) => SwitchForeColorBack(it, switchBackuground: false);
                return "";
            };

            if (Enum.TryParse(typeof(ConsoleColor), colorNameTotalLine,
                ignoreCase: true, out var result3))
            {
                ForegroundColorTotalLine = (ConsoleColor)result3;
            }
            else
            {
                throw new ConfigException(
                    $"Total line '{colorNameTotalLine}' (to {parser.Name}) is NOT color name.");
            }

            Reset = () =>
            {
                Console.ResetColor();
                return string.Empty;
            };

            TotalLine = () =>
            {
                Console.ForegroundColor = ForegroundColorTotalLine;
                return string.Empty;
            };

            return CountLineMax;
        }
        #region Foreground Color

        static string DoNothing(string text) => text;

        static string SwitchForeColorBack(string text, bool switchBackuground)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            if (switchBackuground)
            {
                Console.ForegroundColor = ForegroundColorTotalLine;
            }
            else
            {
                Console.ForegroundColor = ForegroundColorDefault;
            }
            SwitchFore = (it) => SwitchForeColorTo(it, switchBackuground);
            return text;
        }
        static string SwitchForeColorTo(string text, bool switchBackuground)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            Console.ForegroundColor = ForegroundColorSwitch;
            SwitchFore = (it) => SwitchForeColorBack(it, switchBackuground);
            return text;
        }
        static internal Func<string,string> SwitchFore { get; private set; } = DoNothing;
        static internal Func<string, string> TotallyResetFore { get; private set; } = DoNothing;
        static internal Func<string> ResetFore { get; private set; } = () => string.Empty;
        #endregion

        #region Background Color
        static internal Func<bool, string> SwitchBackground { set; private get; }
            = (_) => SwitchBackgroundBack();
        static string SwitchBackgroundBack()
        {
            SwitchFore = (it) => SwitchForeColorBack(it, switchBackuground: false);
            return string.Empty;
        }
        static string SwitchBackgroundTo()
        {
            SwitchFore = (it) => SwitchForeColorTo(it, switchBackuground: true);
            SwitchBackground = (_) => SwitchBackgroundBack();
            return string.Empty;
        }
        #endregion

        static string ReturnBlank() => string.Empty;

        static internal IEnumerable<Func<string>> GetBlanks()
        {
            while (true) yield return ReturnBlank;
        }

        static internal IEnumerable<Func<string>> Background(int dbgMarker, int cntMax)
        {
            var cntThis = 0;
            while (true)
            {
                cntThis += 1;
                if (cntThis >= cntMax) cntThis = 0;
                if (cntThis == 0)
                {
                    SwitchBackgroundTo();
                    TotallyResetFore = (_) =>
                    {
                        SwitchBackground(false);
                        TotallyResetFore = DoNothing;
                        return string.Empty;
                    };
                    yield return () => "";
                }
                else
                {
                    SwitchBackground(false);
                    ResetFore();
                    yield return () => "";
                }
            }
        }

        static internal Func<string> Reset { get; private set; } = ReturnBlank;
        static internal Func<string> TotalLine { get; private set; } = ReturnBlank;
        static int CountLineMax { get; set; } = 0;
        static ConsoleColor ForegroundColorDefault { get; set; }
        static ConsoleColor ForegroundColorSwitch { get; set; }
        static ConsoleColor ForegroundColorPerLineCount { get; set; }
        static ConsoleColor ForegroundColorTotalLine { get; set; }
    }

    static internal readonly IInovke<int, IEnumerable<Func<string>>> ColorOpt =
        new ParseInvoker<int, IEnumerable<Func<string>>>("--color",
            help: "INTEGER,COLOR,COLOR-OF-TOTAL-LINE  (Check dir2 --color +?)",
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
                    // TODO: Write Color-Help .MD URI
                    throw new ShowSyntaxException(parser);
                }

                if (aa.Length != 3)
                {
                    throw new ConfigException(
                        $"Too many values are assigned to {parser.Name}.");
                }

                if (int.TryParse(aa[0], out var lineCountToChangeBackgroundColor))
                {
                    var cntMax = Color.Init(parser, lineCountToChangeBackgroundColor, aa[1], aa[2]);
                    parser.SetImplementation((arg) => Color.Background(arg, cntMax));
                }
                else
                {
                    throw new ConfigException(
                        $"Line count '{aa[0]} to {parser.Name} MUSt be a number.");
                }
            });
}