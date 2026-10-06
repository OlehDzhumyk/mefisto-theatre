using O_Dzhumyk_MefistoTheatre.Services;

namespace O_Dzhumyk_MefistoTheatre.Tests;

public class PostContentFormatterTests
{
    private readonly PostContentFormatter _formatter = new();

    [Fact]
    public void ToSafeHtml_KeepsFormattingButRemovesScripts()
    {
        var html = _formatter.ToSafeHtml(
            "<h2>Review</h2><p onclick=\"steal()\">Great <strong>show</strong></p><script>alert(1)</script>");

        Assert.Equal("<h2>Review</h2><p>Great <strong>show</strong></p>", html);
    }

    [Fact]
    public void ToSafeHtml_RemovesJavascriptLinks()
    {
        var html = _formatter.ToSafeHtml("<a href=\"javascript:alert(1)\">tickets</a>");

        Assert.DoesNotContain("javascript:", html);
    }

    [Fact]
    public void ToExcerpt_ReturnsPlainTextWithDecodedEntities()
    {
        var excerpt = _formatter.ToExcerpt("<p>Romeo &amp; Juliet</p><p>Opening night</p>");

        Assert.Equal("Romeo & Juliet Opening night", excerpt);
    }

    [Fact]
    public void ToExcerpt_CutsLongTextAtAWordBoundary()
    {
        var excerpt = _formatter.ToExcerpt("<p>The cast was superb, and the staging was bold.</p>", maxLength: 20);

        Assert.Equal("The cast was superb…", excerpt);
    }

    [Fact]
    public void ToExcerpt_LeavesShortTextAlone()
    {
        Assert.Equal("Short note", _formatter.ToExcerpt("Short note"));
    }
}
