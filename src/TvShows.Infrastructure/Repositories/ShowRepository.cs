using Microsoft.EntityFrameworkCore;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows;
using TvShows.Infrastructure.Data;

namespace TvShows.Infrastructure.Repositories;

public class ShowRepository(AppDbContext db) : IShowRepository
{
    public async Task<PagedResult<ShowListItem>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = await db.Shows.CountAsync(cancellationToken);
        var items = await db.Shows
            .OrderByDescending(s => s.Premiered)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(s => new ShowListItem(s.Id, s.TvMazeId, s.Name, s.Language, s.Premiered, s.Genres))
            .ToListAsync(cancellationToken);
        return new PagedResult<ShowListItem>(items, page, pageSize, total);
    }

    public async Task<IReadOnlyList<Show>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await db.Shows
            .Where(s => EF.Functions.Like(s.Name, $"%{name}%"))
            .OrderByDescending(s => s.Premiered)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

public async Task<Show?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Shows
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Show?> GetByTvMazeIdAsync(int tvMazeId, CancellationToken cancellationToken = default)
    {
        return await db.Shows
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TvMazeId == tvMazeId, cancellationToken);
    }

    public async Task<IReadOnlyList<Show>> GetByTvMazeIdsAsync(IList<int> ids, CancellationToken cancellationToken = default)
    {
        return await db.Shows
            .Where(s => s.TvMazeId != null && ids.Contains(s.TvMazeId.Value))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Show> shows, CancellationToken cancellationToken = default)
    {
        db.Shows.AddRange(shows);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Show?> GetByIdTrackingAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Shows.FindAsync([id], cancellationToken);
    }

    public async Task<Show> AddAsync(Show show, CancellationToken cancellationToken = default)
    {
        db.Shows.Add(show);
        await db.SaveChangesAsync(cancellationToken);
        return show;
    }

    public async Task UpdateAsync(Show show, CancellationToken cancellationToken = default)
    {
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Show show, CancellationToken cancellationToken = default)
    {
        db.Shows.Remove(show);
        await db.SaveChangesAsync(cancellationToken);
    }
}
