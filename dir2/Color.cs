using static dir2.MyOptions;

namespace dir2;

static internal partial class Show
{
    static internal class Color
    {
        static internal void InternalReset()
        {
            ForegroundColorDefault = Console.ForegroundColor;
            ForegroundColorSwitch = Console.ForegroundColor;
            ForegroundColorPerLineCount = Console.ForegroundColor;
            ForegroundColorTotalLine = Console.ForegroundColor;
            Reset = ReturnZero;
            TotalLine = ReturnZero;
        }

        static Color()
        {
            InternalReset();
        }

        static internal string[] GetColorNames() =>
            Enum.GetNames<ConsoleColor>().ToArray();

        static internal Func<int, (bool, int)> Init(IParse parser,
            int lineCountToAlterColumnColor,
            string colorToAlterColumnColor,
            string colorTotalLine)
        {
            if (Enum.TryParse(typeof(ConsoleColor), colorToAlterColumnColor,
                ignoreCase: true, out var result2))
            {
                ForegroundColorPerLineCount = (ConsoleColor)result2;
            }
            else
            {
                throw new ConfigException(
                    $"'{colorToAlterColumnColor}' (to {parser.Name}) is NOT color name.");
            }

            ForegroundColorSwitch = ForegroundColorPerLineCount;
            SwitchFore = (it) => SwitchForeColorBack(it, switchBackuground: false);
            ResetFore = () =>
            {
                Console.ForegroundColor = ForegroundColorDefault;
                SwitchFore = (it) => SwitchForeColorBack(it, switchBackuground: false);
                return "";
            };

            if (Enum.TryParse(typeof(ConsoleColor), colorTotalLine,
                ignoreCase: true, out var result3))
            {
                ForegroundColorTotalLine = (ConsoleColor)result3;
            }
            else
            {
                throw new ConfigException(
                    $"Total line '{colorTotalLine}' (to {parser.Name}) is NOT color name.");
            }

            Reset = () =>
            {
                Console.ResetColor();
                return 3;
            };

            TotalLine = () =>
            {
                Console.ForegroundColor = ForegroundColorTotalLine;
                return 4;
            };

            if (2 > lineCountToAlterColumnColor)
            {
                throw new ConfigException(
                    $"Line counter {lineCountToAlterColumnColor} (to {parser.Name}) MUST be greater 1.");
            }

            return (lineCount) =>
            {
                int newLineCount = lineCount + 1;
                bool lineCountResetFlag = false;
                if (newLineCount >= lineCountToAlterColumnColor)
                {
                    lineCountResetFlag = true;
                    newLineCount = 0;
                }
                return (lineCountResetFlag, newLineCount);
            };
        }

        static internal Func<int, (bool, int)> Init(IParse parser,
            string colorToAlterColumnColor)
        {
            if (Enum.TryParse(typeof(ConsoleColor), colorToAlterColumnColor,
                ignoreCase: true, out var result2))
            {
                ForegroundColorPerLineCount = (ConsoleColor)result2;
            }
            else
            {
                throw new ConfigException(
                    $"'{colorToAlterColumnColor}' (to {parser.Name}) is NOT color name.");
            }

            ForegroundColorSwitch = ForegroundColorPerLineCount;
            SwitchFore = (it) => SwitchForeColorBack(it, switchBackuground: false);
            ResetFore = () =>
            {
                Console.ForegroundColor = ForegroundColorDefault;
                SwitchFore = (it) => SwitchForeColorBack(it, switchBackuground: false);
                return "";
            };

            ForegroundColorTotalLine = ForegroundColorPerLineCount;

            Reset = () =>
            {
                Console.ResetColor();
                return 5;
            };

            TotalLine = () =>
            {
                Console.ForegroundColor = ForegroundColorTotalLine;
                return 6;
            };

            return (_) => (false, 1); // lineCountResetFlag always is FALSE
        }

        #region Alter Column Foreground Color
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
        static internal Func<string, string> SwitchFore
        { get; private set; } = Helper.itself;
        static internal Func<string, string> TotallyResetFore
        { get; private set; } = Helper.ReturnEmptyString;
        static internal Func<string> ResetFore
        { get; private set; } = Helper.GetEmptyString;
        #endregion

        #region Alter the Basic of Foreground Color
        static internal Func<bool, string> ForegroundAlterBack { set; private get; }
            = (_) => SwitchBackgroundBack();
        static string SwitchBackgroundBack()
        {
            SwitchFore = (it) => SwitchForeColorBack(it, switchBackuground: false);
            return string.Empty;
        }
        static string ForegroundAlterTo()
        {
            SwitchFore = (it) => SwitchForeColorTo(it, switchBackuground: true);
            ForegroundAlterBack = (_) => SwitchBackgroundBack();
            return string.Empty;
        }
        #endregion

        static int ReturnZero() => 0;

        static internal IEnumerable<Func<int>> GetZeroes()
        {
            while (true) yield return ReturnZero;
        }

        static internal IEnumerable<Func<int>> ForegroundColors(int dbgMarker,
            Func<int, (bool, int)> incLineCount)
        {
            bool isLineCountReset = false;
            var lineCount = 0;
            while (true)
            {
                (isLineCountReset, lineCount) = incLineCount(lineCount);
                if (isLineCountReset)
                {
                    ForegroundAlterTo();
                    TotallyResetFore = (_) =>
                    {
                        ForegroundAlterBack(false);
                        TotallyResetFore = Helper.itself;
                        return string.Empty;
                    };
                    yield return () => 1;
                }
                else
                {
                    ForegroundAlterBack(false);
                    ResetFore();
                    yield return () => 2;
                }
            }
        }

        static internal Func<int> Reset { get; private set; } = ReturnZero;
        static internal Func<int> TotalLine { get; private set; } = ReturnZero;
        static ConsoleColor ForegroundColorDefault { get; set; }
        static ConsoleColor ForegroundColorSwitch { get; set; }
        static ConsoleColor ForegroundColorPerLineCount { get; set; }
        static ConsoleColor ForegroundColorTotalLine { get; set; }
    }

    static internal readonly IInovke<int, IEnumerable<Func<int>>> ColorOpt =
        new ParseInvoker<int, IEnumerable<Func<int>>>("--color",
            help: "off | COLOR | INTEGER,COLOR,COLOR-OF-TOTAL-LINE (Check dir2 --color +?)",
            init: (_) => Color.GetZeroes(), resolve: (parser, args) =>
            {
                if (Console.IsOutputRedirected) return;

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

                Func<int, (bool, int)> incFunc;
                switch (aa.Length)
                {
                    case 1:
                        if (0 == string.Compare("off", aa[0]))
                        {
                            Color.InternalReset();
                            parser.SetImplementation((arg) => Color.GetZeroes()) ;
                        }
                        else
                        {
                            incFunc = Color.Init(parser, aa[0]);
                            parser.SetImplementation((arg) => Color.ForegroundColors(arg, incFunc));
                        }
                        break;
                    case 3:
                        if (int.TryParse(aa[0], out var lineCountToChangeBackgroundColor))
                        {
                            incFunc = Color.Init(parser, lineCountToChangeBackgroundColor, aa[1], aa[2]);
                            parser.SetImplementation((arg) => Color.ForegroundColors(arg, incFunc));
                        }
                        else
                        {
                            throw new ConfigException(
                                $"Line count to {parser.Name} SHOULD be a number but '{aa[0]}' is found.");
                        }
                        break;
                    default:
                        throw new ShowSyntaxException(parser);
                }
            });

    static internal readonly IInovke<bool, bool> PauseOpt =
        new ParseInvoker<bool, bool>("--pause",
            help: "off | on | INTEGER", init: Helper.itself, resolve: (parser, args) =>
            {
                if (Console.IsOutputRedirected) return;
                var argThe = Helper.GetUnique(args, parser);

                int lineCount = 0;
                int lineCountToPause = 0;
                int cursorRow = 0;
                switch (argThe, int.TryParse(argThe, out lineCountToPause))
                {
                    case ("off", _):
                        parser.SetImplementation((_) => false);
                        break;
                    case ("on", _):
                        lineCountToPause = Console.WindowHeight - 1;
                        parser.SetImplementation((_) =>
                        {
                            lineCount += 1;
                            if (lineCount >= lineCountToPause)
                            {
                                lineCount = 0;
                                Console.Write("Press any key to continue (q to quick) ");
                                var inpChar = Console.ReadKey();
                                if (inpChar.KeyChar == 'q' || inpChar.KeyChar == 'Q')
                                {
                                    Console.ResetColor();
                                    Console.SetCursorPosition(left: 0, top: cursorRow);
                                    // sole.Write("Press any key to continue (q to quick) Q");
                                    Console.Write("                                        ");
                                    Console.SetCursorPosition(left: 0, top: cursorRow);
                                    Environment.Exit(0);
                                }
                                (var _, cursorRow) = Console.GetCursorPosition();
                                Console.SetCursorPosition(left: 0, top: cursorRow);
                                lineCountToPause = Console.WindowHeight - 1;
                            }
                            return false;
                        });
                        break;
                    case (_, true):
                        if (lineCountToPause < 6)
                        {
                            throw new ConfigException(
                                $"Line count to {parser.Name} SHOULD be greater 5 but {lineCountToPause} is found.");
                        }
                        parser.SetImplementation((_) =>
                        {
                            lineCount += 1;
                            if (lineCount >= lineCountToPause)
                            {
                                lineCount = 0;
                                (var _, cursorRow) = Console.GetCursorPosition();
                                Console.Write("Press any key to continue (q to quick) ");
                                var inpChar = Console.ReadKey();
                                if (inpChar.KeyChar == 'q' || inpChar.KeyChar == 'Q')
                                {
                                    Console.ResetColor();
                                    Console.SetCursorPosition(left: 0, top: cursorRow);
                                    // sole.Write("Press any key to continue (q to quick) Q");
                                    Console.Write("                                        ");
                                    Console.SetCursorPosition(left: 0, top: cursorRow);
                                    Environment.Exit(0);
                                }
                                Console.SetCursorPosition(left: 0, top: cursorRow);
                            }
                            return false;
                        });
                        break;
                    default:
                        throw new ConfigException(
                            $"Line count to {parser.Name} SHOULD be 'on', 'off', or a number, but '{argThe}' is found.");
                }
            });
}
