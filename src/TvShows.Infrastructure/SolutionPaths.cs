namespace TvShows.Infrastructure;

public static class SolutionPaths
{
    public static string DefaultShowsDbConnectionString() =>
        $"Data Source={Path.Combine(SolutionRoot(), "src", "TvShows.Api", "shows.db")}";

    public static string DefaultCacheDbConnectionString() =>
        $"Data Source={Path.Combine(SolutionRoot(), "src", "TvShows.Api", "cache.db")}";

    private static string SolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !dir.EnumerateFiles("*.slnx").Any() && !dir.EnumerateFiles("*.sln").Any())
            dir = dir.Parent;
        return dir?.FullName
            ?? throw new InvalidOperationException(
                "Could not locate solution root from " + AppContext.BaseDirectory);
    }
}
