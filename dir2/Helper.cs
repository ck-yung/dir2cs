using System.Reflection;

namespace dir2;

public static partial class Helper
{
    static internal readonly string ExeName;
    static internal readonly string ExeVersion;
    static Helper()
    {
        var asm = Assembly.GetExecutingAssembly().GetName();
        ExeName = asm.Name ?? "?";
        ExeVersion = asm.Version?.ToString() ?? "?";
    }

    public static string GetVersion() => $"{ExeName} v{ExeVersion}";

    public static string GetHelpSyntax() => $"""
        Get help by
          {ExeName} -?
        """;

    public static string GetSyntax() => $"""
        Syntax: {ExeName} [OPTION ..] [DIRNAME]
        OPTION:
          --sub
          --dir DIR-OPTION
        DIR-OPTION: both only off
        """;
}
