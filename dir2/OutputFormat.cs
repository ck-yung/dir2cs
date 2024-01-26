using System.Collections.Generic;
using static dir2.MyOptions;
using static dir2.Helper;

namespace dir2;

static internal partial class Show
{

    static internal bool IsOututCsv { get; private set; } = false;

    static internal readonly IInovke<bool, bool> OutputOpt =
        new ParseInvoker<bool, bool>("--output", help: "csv",
            init: Always<bool>.True, resolve: (parser, args) =>
            {
                var argThe = Helper.GetUnique(args, parser);
                switch (argThe.ToLower())
                {
                    case "csv":
                        var a3 = ((IParse)PrintDirOpt).Parse(
                            new List<(bool, ArgType, string)>
                            {
                                new(true, ArgType.CommandLine, "off")
                            });

                        ((ParseInvoker<long, string>)LengthFormatOpt)
                        .SetImplementation((arg) => arg.ToString()+",");

                        ((ParseInvoker<DateTimeOffset, string>)DateFormatOpt)
                        .SetImplementation(
                            (arg) => arg.ToString("yyyy-MM-ddTHH:mm:sszz"));

                        ((ParseInvoker<int, string>)CountFormat).SetImplementation(
                            (_) => string.Empty);

                        Date = (arg) =>
                        {
                            if (string.IsNullOrEmpty(arg)) return "";
                            return "\"" + arg + "\",";
                        };

                        Last = Helper.itself;

                        Link = (arg) =>
                        {
                            if (string.IsNullOrEmpty(arg.LinkTarget)) return "";
                            return ",\"" + arg.LinkTarget + "\"";
                        };

                        IsOututCsv = true;
                        impPrintInfoTotal = InfoSum.DoNothing;
                        break;
                    default:
                        throw new ConfigException($"'{argThe}' is bad value to {parser.Name}");
                }
            });
}
