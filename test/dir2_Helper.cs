namespace test;

public class dir2_Helper
{
    [Fact]
    public void VeresionStringShouldNotContainQuestionMark()
    {
        Assert.DoesNotContain("?", Helper.GetVersion());
    }
}
