using System.Net;
using System.Net.Http.Json;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;

namespace TvShows.Infrastructure.TvMaze;

internal class TvMazeClient(HttpClient httpClient) : ITvMazeClient
{
    public async Task<IReadOnlyList<Show>> SearchShowsAsync(string query, CancellationToken cancellationToken = default)
    {
        var results = await httpClient.GetFromJsonAsync<List<TvMazeSearchResult>>(
            $"search/shows?q={Uri.EscapeDataString(query)}", cancellationToken);

        if (results is null or { Count: 0 })
            return [];

        return results.Select(r => r.Show.ToShow()).ToList();
    }

    public async Task<Dictionary<int, long>> GetShowUpdatesAsync(string since, CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<Dictionary<int, long>>(
            $"updates/shows?since={Uri.EscapeDataString(since)}", cancellationToken);

        return result ?? [];
    }

    public async Task<Show?> GetShowByTvMazeIdAsync(int tvMazeId, CancellationToken cancellationToken = default)
    {
        var tvShow = await GetOrNullAsync<TvMazeShow>($"shows/{tvMazeId}", cancellationToken);
        return tvShow?.ToShow();
    }

    public async Task<IReadOnlyList<Show>?> GetShowsByPageAsync(int page, CancellationToken cancellationToken = default)
    {
        var tvShows = await GetOrNullAsync<List<TvMazeShow>>($"shows?page={page}", cancellationToken);

        if (tvShows is null)
            return null;

        return tvShows.Select(s => s.ToShow()).ToList();
    }

    private async Task<T?> GetOrNullAsync<T>(string url, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }
}
