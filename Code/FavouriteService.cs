namespace Code;

public class FavouriteService(Repository<Track> repository)
{
    public async Task SetFavouriteTracksAsync(Guid userId, Guid trackId, CancellationToken ct)
    {

    }

    public async IAsyncEnumerable<Track> GetFavouriteTracksAsync(Guid userId, CancellationToken ct)
    {
        await Task.CompletedTask;
        yield break;
    }
}
