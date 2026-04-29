using TvShows.Infrastructure;

namespace TvShows.Tests;

public class ShowCacheTests
{
    [Fact]
    public void SearchKey_normalizes_whitespace_and_casing()
    {
        Assert.Equal(ShowCache.SearchKey("game of thrones"), ShowCache.SearchKey("  GAME of Thrones "));
    }
}
