namespace test;

public class UnitTest_Helper
{
    [Fact]
    public void VeresionStringShouldNotContainQuestionMark()
    {
        Assert.DoesNotContain("?", Helper.GetVersion());
    }
}
