namespace Code;

public class FavouriteService(Repository<Track> repository)
{
    public async Task SetFavouriteTracksAsync(Guid userId, Guid trackId, CancellationToken ct)
    {
        var track = await repository.LoadAsync(userId, trackId, ct);
        track.IsFavorite = true;
        await repository.SaveAsync(userId, track, ct);
    }

    public IAsyncEnumerable<Track> GetFavouriteTracksAsync(Guid userId, CancellationToken ct)
    {
        return repository.LoadAllAsync(userId, ct).Where(track => track.IsFavorite);
    }
}
