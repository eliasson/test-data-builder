namespace Code.Tests;

public class WhenGettingFavouriteTracksAcrossAlbums
{
    // Not only using authentic data, it is also great songs (and albums)!
    private static readonly AlbumName SeventeenSeconds = new("Seventeen Seconds");
    private static readonly AlbumName Wish = new("Wish");
    private static readonly TrackName AForest = new("A Forest");
    private static readonly TrackName Apart = new("Apart");
    private static readonly TrackName ALetterToElise = new("A Letter to Elise");
    private FavouriteTestCase _tc = null!;

    [OneTimeSetUp]
    public async Task MarkTracksAsFavourite()
    {
        _tc = await new FavouriteTestCase()
            .WithUser()
            .WithArtist() // You know who it is, right?
            // These to albums will use the default artist, since there is only one.
            .WithAlbum(SeventeenSeconds)
            .WithAlbum(Wish)
            // Add two favourite tracks
            .WithTrack(albumNamed: SeventeenSeconds, name: AForest, configure: t => t.MarkAsFavourite())
            .WithTrack(albumNamed: Wish, name: Apart, configure: t => t.MarkAsFavourite())
            // Add a control track that is not yet a favourite.
            .WithTrack(albumNamed: Wish, name: ALetterToElise)
            .AsFavouriteTestCase()
            .BuildAsync();
    }

    [Test]
    public async Task It_should_have_two_favourite_tracks()
    {
        var user = await _tc.UserOrThrowAsync();

        var favouriteTracks = await _tc.FavouriteService
            .GetFavouriteTracksAsync(user.Id, CancellationToken.None)
            .ToListAsync();

        var favourites = favouriteTracks.Select(t => t.Title);

        Assert.That(favourites, Is.EquivalentTo(new [] { "A Forest", "Apart" }));
    }
}