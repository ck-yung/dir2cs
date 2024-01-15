namespace dir2;
static public partial class MyOptions
{
    static InfoSum impSubDir(string path)
    {
        if (PrintDir == EnumPrint.OnlyDir)
        {
            var cntDir = Helper.GetAllDirs(Helper.ToInfoDir(path))
            .Where((it) => Wild.CheckIfDirNameMatched(it.Name))
            .Where((it) => Wild.IsMatchWithinDate(Show.GetDate(it)))
            .Where((it) => Wild.IsMatchNotWithinDate(Show.GetDate(it)))
            .Invoke(Sort.Dirs)
            .Invoke(Sort.ReverseDir)
            .Invoke(Sort.TakeDir)
            .Select((it) =>
            {
                Helper.ItemWrite(Show.Attributes(it));
                Helper.ItemWrite(Show.Owner(it));
                Helper.ItemWrite(Show.Date(Helper.DateFormatOpt.Invoke(Show.GetDate(it))));
                Helper.ItemWrite(Show.GetDirName(Helper.io.GetRelativeName(it.FullName)));
                Helper.ItemWrite(Show.Link.Invoke(it));
                Helper.ItemWriteLine(string.Empty);
                return it;
            })
            .Count();
            PrintDirCount(cntDir);
            return InfoSum.Fake;
        }
        return Helper.GetAllFiles(Helper.ToInfoDir(path))
        .Select((it) => Helper.ToInfoFile(it))
        .Where((it) => it.IsNotFake)
        .Where((it) => Wild.CheckIfFileNameMatched(it.Name))
        .Where((it) => (false == Wild.ExclFileNameOpt.Invoke(it.Name)))
        .Where((it) => Wild.IsMatchWithinSize(it.Length))
        .Where((it) => Wild.IsMatchWithinDate(Show.GetDate(it)))
        .Where((it) => Wild.IsMatchNotWithinSize(it.Length))
        .Where((it) => Wild.IsMatchNotWithinDate(Show.GetDate(it)))
        .Where((it) => Wild.ExtensionOpt.Invoke(it))
        .Where((it) => Helper.IsHiddenFileOpt.Invoke(it))
        .Where((it) => Helper.IsLinkFileOpt.Invoke(it))
        .Invoke(Sum.Reduce);
    }
}
