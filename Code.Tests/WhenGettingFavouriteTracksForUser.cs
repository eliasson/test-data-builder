namespace Code.Tests;

public class WhenGettingFavouriteTracksForUser
{
    private FavouriteTestCase _tc = null!;

    [OneTimeSetUp]
    public async Task MarkTrackAsFavourite()
    {
        _tc = await new FavouriteTestCase()
            .WithUser()
            .WithArtist()
            .WithAlbum()
            .WithTrack(configure: t => t.Title = "Glassy Eyes")
            .AsFavouriteTestCase()
            .BuildAsync();

        var user = await _tc.UserOrThrowAsync();
        var track = await _tc.TrackOrThrowAsync();

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

        Assert.That(favourites, Is.EqualTo(new [] { "Glassy Eyes" }));
    }
}
