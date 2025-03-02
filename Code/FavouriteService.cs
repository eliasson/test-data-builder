namespace Code;

public class FavouriteService(UserRepository userRepository, Repository<Track> trackRepository)
{
    public async Task SetFavouriteTracksAsync(Guid userId, Guid trackId, CancellationToken ct)
    {
        var track = await trackRepository.LoadAsync(userId, trackId, ct);
        track.MarkAsFavourite();
        await trackRepository.SaveAsync(userId, track, ct);
    }

    public IAsyncEnumerable<Track> GetFavouriteTracksAsync(Guid userId, CancellationToken ct)
    {
        return trackRepository.LoadAllAsync(userId, ct).Where(track => track.IsFavorite);
    }
}
