namespace Code.Tests;

public class WithoutTheBuilder
{
    private UserRepository _userRepository = null!;
    private Repository<Artist> _artistRepository = null!;
    private Repository<Album> _albumRepository = null!;
    private Repository<Track> _trackRepository = null!;

    private User _actingUser = null!;
    private FavouriteService _service = null!;

    [OneTimeSetUp]
    public async Task MarkTrackAsFavourite()
    {
        // First, we need to set up the infra to store our stuff.
        // Due to validation rules we must create the correct graph of aggregates.
        _userRepository = new UserRepository();
        _artistRepository = new Repository<Artist>();
        _albumRepository = new Repository<Album>();
        _trackRepository = new Repository<Track>();

        // Create the acting user for our test
        _actingUser = new User { Username = "emma.goldman" };
        await _userRepository.SaveAsync(_actingUser, CancellationToken.None);

        var artist = new Artist { UserId = _actingUser.Id, Name = "Kite" };
        await _artistRepository.SaveAsync(_actingUser.Id, artist, CancellationToken.None);

        var album = new Album { ArtistId = artist.Id, Name = "VII" };
        await _albumRepository.SaveAsync(_actingUser.Id, album, CancellationToken.None);

        var track = new Track { AlbumId = album.Id, Title = "Glassy Eyes" };
        await _trackRepository.SaveAsync(_actingUser.Id, track, CancellationToken.None);

        // Create the service we are writing our test of.
        _service = new FavouriteService(_userRepository, _trackRepository);

        // Perform the action that we are testing
        await _service.SetFavouriteTracksAsync(_actingUser.Id, track.Id, CancellationToken.None);
    }

    [Test]
    public async Task It_should_have_one_favourite_track()
    {
        var favouriteTracks = await _service
            .GetFavouriteTracksAsync(_actingUser.Id, CancellationToken.None)
            .ToListAsync();

        var favourites = favouriteTracks.Select(t => t.Title);

        Assert.That(favourites, Is.EqualTo(new [] { "Glassy Eyes" }));
    }
}