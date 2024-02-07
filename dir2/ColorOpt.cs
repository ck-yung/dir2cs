using static dir2.MyOptions;

namespace dir2;

static internal partial class Show
{
    static internal class Color
    {
        static internal void InternalReset()
        {
            ForegroundColorDefault = Console.ForegroundColor;
            BackgroundColorTotalLine = Console.BackgroundColor;
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

        static internal Func<int, (bool, int)> Init(IParse parser,
            int lineCountToAlterColumnColor,
            string colorToAlterColumnColor,
            string colorTotalLine,
            string backColorTotalLine = "")
        {
            if (TryParseToForeColor(colorToAlterColumnColor, out var tmp))
            {
                ForegroundColorPerLineCount = tmp;
            }
            else
            {
                throw new ConfigException(
                    $"'{colorToAlterColumnColor}' (to {parser.Name}) is NOT color name.");
            }

            ForegroundColorSwitch = ForegroundColorPerLineCount;
            SwitchFore = (it) => SwitchForeColorBack(it);
            ResetFore = () =>
            {
                Console.ForegroundColor = ForegroundColorDefault;
                SwitchFore = (it) => SwitchForeColorBack(it);
                return "";
            };

            if (TryParseToForeColor(colorTotalLine, out tmp))
            {
                ForegroundColorTotalLine = tmp;
            }
            else
            {
                throw new ConfigException(
                    $"Total line '{colorTotalLine}' (to {parser.Name}) is NOT color name.");
            }

            if (false == string.IsNullOrEmpty(backColorTotalLine))
            {
                if (TryParseToForeColor(backColorTotalLine, out tmp))
                {
                    BackgroundColorTotalLine = tmp;
                }
                else
                {
                    throw new ConfigException(
                        $"Background of total line '{colorTotalLine}' (to {parser.Name}) is NOT color name.");
                }
            }

            Reset = () =>
            {
                Console.ResetColor();
                return 3;
            };

            TotalLine = () =>
            {
                Console.ForegroundColor = ForegroundColorTotalLine;
                Console.BackgroundColor = BackgroundColorTotalLine;
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
            if (TryParseToForeColor(colorToAlterColumnColor, out var tmp))
            {
                ForegroundColorPerLineCount = tmp;
            }
            else
            {
                throw new ConfigException(
                    $"'{colorToAlterColumnColor}' (to {parser.Name}) is NOT color name.");
            }

            ForegroundColorSwitch = ForegroundColorPerLineCount;
            SwitchFore = (it) => SwitchForeColorBack(it);
            ResetFore = () =>
            {
                Console.ForegroundColor = ForegroundColorDefault;
                SwitchFore = (it) => SwitchForeColorBack(it);
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
                Console.BackgroundColor = BackgroundColorTotalLine;
                return 6;
            };

            return (_) => (false, 1); // lineCountResetFlag always is FALSE
        }

        #region Alter Column Foreground Color
        static string SwitchForeColorBack(string text, bool isTotalLine = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            if (isTotalLine)
            {
                Console.ForegroundColor = ForegroundColorTotalLine;
            }
            else
            {
                Console.ForegroundColor = ForegroundColorDefault;
            }
            SwitchFore = (it) => SwitchForeColorTo(it, isTotalLine);
            return text;
        }
        static string SwitchForeColorTo(string text, bool isTotalLine)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            Console.ForegroundColor = ForegroundColorSwitch;
            SwitchFore = (it) => SwitchForeColorBack(it, isTotalLine);
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
            SwitchFore = (it) => SwitchForeColorBack(it);
            return string.Empty;
        }
        static string ForegroundAlterTo()
        {
            SwitchFore = (it) => SwitchForeColorTo(it, isTotalLine: true);
            ForegroundAlterBack = (_) => SwitchBackgroundBack();
            return string.Empty;
        }
        #endregion

        static int ReturnZero() => 0;

        static internal IEnumerable<Func<int>> GetZeroes()
        {
            while (true) yield return ReturnZero;
        }

        static internal IEnumerable<Func<int>> ForegroundColors(
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
        static ConsoleColor BackgroundColorTotalLine { get; set; }

        static public readonly string ShortcutOriginalForeColor = "=";
        static bool TryParseToForeColor(string arg, out ConsoleColor output)
        {
            if (arg == ShortcutOriginalForeColor)
            {
                output = Console.ForegroundColor;
                return true;
            }

            if (Enum.TryParse(typeof(ConsoleColor), arg,
                ignoreCase: true, out var result))
            {
                output = (ConsoleColor)result;
                return true;
            }
            output = ConsoleColor.White;
            return false;
        }
    }

    static internal readonly IInovke<int, IEnumerable<Func<int>>> ColorOpt =
        new ParseInvoker<int, IEnumerable<Func<int>>>("--color",
            help: "off | COLOR[,NUMBER,COLOR-OF-TOTAL-LINE[,BACKGROUND-COLOR-OF-TOTAL-LINE]]",
            extraHelp: """
            For example,
                dir2 --color darkred
                dir2 --color yellow,10,green

            https://github.com/ck-yung/dir2cs/blob/main/docs/info-color.md
            """,
            init: (_) => Color.GetZeroes(), resolve: (parser, args) =>
            {
                var aa = args
                .Select((it) => it.Split(';', ','))
                .SelectMany((it) => it)
                .Where((it) => it.Length > 0)
                .Take(5)
                .ToArray();

                if (aa.Any((it) => it == Helper.ExtraHelp
                || 0 == string.Compare(it, "color", ignoreCase: true)))
                {
                    Console.WriteLine("Color name:");
                    Console.WriteLine($"\t{Color.ShortcutOriginalForeColor,-12} Current color");

                    Action<bool, ConsoleColor> switchBackgroundColor = (isBlack, arg) =>
                    {
                        Console.ForegroundColor = arg;
                        switch (isBlack, arg)
                        {
                            case (_, ConsoleColor.Black):
                                Console.BackgroundColor = ConsoleColor.Gray;
                                break;
                            case (_, ConsoleColor.Gray):
                                Console.BackgroundColor = ConsoleColor.Black;
                                break;
                            case (true, _):
                                Console.BackgroundColor = ConsoleColor.Black;
                                break;
                            default:
                                Console.BackgroundColor = ConsoleColor.Gray;
                                break;
                        }
                        Console.Write(isBlack ? " Good " : " Demo ");
                    };

                    Action resetColor = () => Console.ResetColor();

                    if (Console.IsOutputRedirected)
                    {
                        switchBackgroundColor = (_, _) => { };
                        resetColor = () => { };
                    }

                    foreach (ConsoleColor cr2 in Enum.GetValues(typeof(ConsoleColor)))
                    {
                        Console.Write($"\t{cr2,-12}");
                        switchBackgroundColor(true, cr2);
                        switchBackgroundColor(false, cr2);
                        resetColor();
                        Console.WriteLine();
                    }
                    throw new ShowSyntaxException(parser);
                }

                if (Console.IsOutputRedirected) return;

                Func<int, (bool, int)> incFunc;
                int lineCountToChangeBackgroundColor;
                switch (aa.Length)
                {
                    case 1:
                        if (0 == string.Compare("off", aa[0], ignoreCase: true))
                        {
                            Color.InternalReset();
                            parser.SetImplementation((arg) => Color.GetZeroes()) ;
                        }
                        else
                        {
                            incFunc = Color.Init(parser, aa[0]);
                            parser.SetImplementation((_) => Color.ForegroundColors(incFunc));
                        }
                        break;
                    case 3:
                        if (int.TryParse(aa[1], out lineCountToChangeBackgroundColor))
                        {
                            incFunc = Color.Init(parser, lineCountToChangeBackgroundColor, aa[0], aa[2]);
                            parser.SetImplementation((_) => Color.ForegroundColors(incFunc));
                        }
                        else
                        {
                            throw new ConfigException(
                                $"Line count to {parser.Name} should be a number but '{aa[1]}' is found.");
                        }
                        break;
                    case 4:
                        if (int.TryParse(aa[1], out lineCountToChangeBackgroundColor))
                        {
                            incFunc = Color.Init(parser, lineCountToChangeBackgroundColor, aa[0], aa[2], aa[3]);
                            parser.SetImplementation((_) => Color.ForegroundColors(incFunc));
                        }
                        else
                        {
                            throw new ConfigException(
                                $"Line count to {parser.Name} should be a number but '{aa[1]}' is found.");
                        }
                        break;
                    default:
                        throw new ShowSyntaxException(parser);
                }
            });

    static void ConsoleReadKey()
    {
        Console.Write("Press any key (q to quit) ");
        var inp = Console.ReadKey();
        Console.Write("\r");
        // sole.Write("Press any key (q to quit) AB");
        Console.Write("                             ");
        Console.Write("\r");
        if (inp.KeyChar == 'q' || inp.KeyChar == 'Q')
        {
            Console.ResetColor();
            Environment.Exit(0);
        }
    }

    static internal readonly IInovke<bool, bool> PauseOpt =
        new ParseInvoker<bool, bool>("--pause",
            help: "off | on | NUMBER", init: Helper.itself,
            resolve: (parser, args) =>
            {
                if (Console.IsOutputRedirected) return;
                var argThe = Helper.GetUnique(args, parser);

                int lineCount = 0;
                switch (argThe.ToLower(), int.TryParse(argThe,
                    out var lineCountToPause))
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
                                ConsoleReadKey();
                                lineCountToPause = Console.WindowHeight - 1;
                            }
                            return false;
                        });
                        break;
                    case (_, true):
                        if (lineCountToPause < 3)
                        {
                            throw new ConfigException(
                                $"Line count to {parser.Name} should be greater 2 but {lineCountToPause} is found.");
                        }
                        parser.SetImplementation((_) =>
                        {
                            lineCount += 1;
                            if (lineCount >= lineCountToPause)
                            {
                                lineCount = 0;
                                ConsoleReadKey();
                            }
                            return false;
                        });
                        break;
                    default:
                        throw new ConfigException(
                            $"Value to {parser.Name} should be 'on', 'off', or a number, but '{argThe}' is found.");
                }
            });
}
