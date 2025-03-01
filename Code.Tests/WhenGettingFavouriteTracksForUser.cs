namespace Code.Tests;

public class WhenGettingFavouriteTracksForUser
{
    private static readonly TrackName TrackOne = new("Track One");
    private static readonly TrackName TrackTwo = new("Track Two");
    private FavouriteTestCase _tc = null!;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _tc = await new FavouriteTestCase()
            .WithArtist()
            .WithAlbum()
            .WithTrack(name: TrackOne)
            .WithTrack(name: TrackTwo, configure: t => t.Title = "Pictures of you")
            .AsFavouriteTestCase()
            .BuildAsync();

        var user = await _tc.UserOrThrowAsync();
        var track = await _tc.TrackOrThrowAsync(TrackTwo);

        await _tc.FavouriteService.SetFavouriteTracksAsync(user.Id, track.Id, CancellationToken.None);
    }

    [Test]
    public async Task It_should_have_one_favourite_track()
    {
        var user = await _tc.UserOrThrowAsync();

        var favouriteTracks = await _tc.FavouriteService
            .GetFavouriteTracksAsync(user.Id, CancellationToken.None)
            .ToListAsync();

        var favourites = favouriteTracks.Select(t => t.Title);

        Assert.That(favourites, Is.EqualTo(new [] { "Pictures of you" }));
    }
}
