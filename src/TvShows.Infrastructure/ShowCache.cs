namespace TvShows.Infrastructure;

internal static class ShowCache
{
    public const string Tag = "shows";
    public const string AllKey = "shows-all";
    public static string PageKey(int page, int pageSize) => $"shows-page-{page}-{pageSize}";
    public static string ByIdKey(int id) => $"show-{id}";
    public static string SearchKey(string name) => $"shows-search-{name.Trim().ToLowerInvariant()}";
    public static readonly string[] Tags = [Tag];
}
