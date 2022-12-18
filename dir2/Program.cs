namespace dir2;
public class Program
{
    static public void Main(string[] args)
    {
		try
		{
			RunMain(args);
		}
		catch (Exception ee)
		{
			Console.WriteLine(ee.Message);
		}
    }

	static bool RunMain(string[] mainArgs)
	{
		if (mainArgs.Contains("--version"))
		{
			Console.WriteLine(Helper.GetVersion());
			return false;
		}

        if (mainArgs.Length == 0)
        {
            Console.WriteLine(Helper.GetHelpSyntax());
            return false;
        }

		Console.WriteLine($"#arg={mainArgs.Length}");
        return true;
	}
}
