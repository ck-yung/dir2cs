using System.Collections.Generic;
using static dir2.MyOptions;
using static dir2.Helper;

namespace dir2;

static internal partial class Show
{

    static internal bool IsOututCsv { get; private set; } = false;

    static internal Func<string, string> OutputName { get; private set; } = Helper.itself;
    static internal void FormatOuputName(bool isAddClosingMark)
    {
        if (isAddClosingMark)
        {
            OutputName = (arg) => "\"" + arg + "\"";
        }
        else
        {
            OutputName = Helper.itself;
        }
    }

    static internal Func<string, Func<string, string>, string> OutputString
    { get; private set; } = (arg, format) =>
    {
        return format(arg);
    };

    static internal readonly IInovke<bool, bool> OutputOpt =
        new ParseInvoker<bool, bool>("--output", help: "csv",
            init: Always<bool>.True, resolve: (parser, args) =>
            {
                var argThe = Helper.GetUnique(args, parser);
                switch (argThe.ToLower())
                {
                    case "csv":
                        ((ParseInvoker<long, string>)LengthFormatOpt)
                        .SetImplementation((arg) => arg.ToString()+",");

                        ((ParseInvoker<DateTimeOffset, string>)DateFormatOpt)
                        .SetImplementation(
                            (arg) => arg.ToString("yyyy-MM-ddTHH:mm:sszz"));

                        ((ParseInvoker<int, string>)CountFormat).SetImplementation(
                            (_) => string.Empty);

                        IsOututCsv = true;
                        FormatOuputName(true);
                        OutputString = (arg, _) =>
                        {
                            if (string.IsNullOrEmpty(arg)) return "";
                            return "\"" + arg + "\",";
                        };

                        Date = (arg) => OutputString(arg, Helper.itself);

                        Last = Helper.itself;
                        Link = (arg) =>
                        {
                            if (string.IsNullOrEmpty(arg.LinkTarget))
                                return string.Empty;
                            return ",\"" + arg.LinkTarget + "\"";
                        };

                        impPrintInfoTotal = InfoSum.DoNothing;
                        break;
                    default:
                        throw new ConfigException($"'{argThe}' is bad value to {parser.Name}");
                }
            });
}
