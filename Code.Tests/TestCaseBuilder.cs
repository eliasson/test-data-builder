using System.Collections.Concurrent;

namespace Code.Tests;

//
// Type safe identifier / test-data name for an entity.
//
public record UserName(string Name);
public record ArtistName(string Name);
public record AlbumName(string Name);
public record TrackName(string Name);

//
// The core functionality for the test case builder that is shared by most of the application.
//

public  class TestCaseBuilder
{
    //
    // The repositories used to access all data. This should not be exposed directly to the clients.
    //
    protected Repository<User> UserRepository = new ();
    protected Repository<Artist> ArtistRepository = new ();
    protected Repository<Album> AlbumRepository = new ();
    protected Repository<Track> TrackRepository = new ();

    //
    // Keep of all async tasks of artists to create. These may or may not be completed.
    //
    private readonly IList<Task> _constructionOfUsers = new List<Task>();
    private readonly IList<Task> _constructionOfArtists = new List<Task>();
    private readonly IList<Task> _constructionOfAlbums = new List<Task>();
    private readonly IList<Task> _constructionOfTracks = new List<Task>();

    //
    // Keep track of all aggregates that has been constructed.
    //
    private readonly ConcurrentQueue<ConstructedUser> _constructedUsers = new();
    private readonly ConcurrentQueue<ConstructedArtist> _constructedArtists = new();
    private readonly ConcurrentQueue<ConstructedAlbum> _constructedAlbums = new();
    private readonly ConcurrentQueue<ConstructedTrack> _constructedTracks = new();

    //
    // Generate a default name for each type of aggregate.
    //
    protected UserName NextUserName() => new($"Artist {_constructedUsers.Count + 1}");
    protected ArtistName NextArtistName() => new($"Artist {_constructedArtists.Count + 1}");
    protected AlbumName NextAlbumName() => new($"Artist {_constructedAlbums.Count + 1}");
    protected TrackName NextTrackName() => new($"Artist {_constructedTracks.Count + 1}");

    //
    // Each aggregate type has support for:
    // - Synchronous initiating construction of instance.
    // - Asynchronous adding a new instance after test case is built.
    // - Retrieve a constructed aggregate by test-name.

    public TestCaseBuilder WithArtist(ArtistName? name = null, Action<Artist>? configure = null)
    {
        // Since this is a synchronous method we only fire off a task to do the construction without
        // waiting for its result.
        var task = Task.Run(() =>  AddArtistAsync(name, configure));

        // Keep a reference to the construction task so it can be awaited elsewhere.
        _constructionOfArtists.Add(task);

        return this;
    }

    public async Task<Artist> AddArtistAsync(ArtistName? name = null, Action<Artist>? configure = null)
    {
        // Create an arbitrary artist.
        var artist = new Artist { Name = name?.Name ?? "Arbitrary artist" };

        // Let the caller modify the artist before saving it.
        configure?.Invoke(artist);
        await ArtistRepository.SaveAsync(artist, CancellationToken.None);

        // Remember the artist created by its test name so that it can be retrieved later.
        _constructedArtists.Enqueue(new ConstructedArtist(artist, name ?? NextArtistName()));

        return artist;
    }

    public async Task<Artist> ArtistOrThrowAsync(ArtistName? name = null)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }


    public TestCaseBuilder WithAlbum(AlbumName? name = null, Action<Album>? configure = null)
    {
        var task = Task.Run(() =>  AddAlbumAsync(name, configure));

        _constructionOfAlbums.Add(task);

        return this;
    }

    public async Task<Album> AddAlbumAsync(AlbumName? name = null, Action<Album>? configure = null)
    {
        // TODO Add artist reference

        var album = new Album { Name = name?.Name ?? "Arbitrary album" };
        configure?.Invoke(album);
        await AlbumRepository.SaveAsync(album, CancellationToken.None);

        _constructedAlbums.Enqueue(new ConstructedAlbum(album, name ?? NextAlbumName()));

        return album;
    }

    public TestCaseBuilder WithTrack(TrackName? name = null, Action<Track>? configure = null)
    {
        var task = Task.Run(() =>  AddTrackAsync(name, configure));

        _constructionOfTracks.Add(task);

        return this;
    }

    public async Task<Track> AddTrackAsync(TrackName? name = null, Action<Track>? configure = null)
    {
        // TODO Add album reference

        var track = new Track { Title = name?.Name ?? "Arbitrary track" };
        configure?.Invoke(track);
        await TrackRepository.SaveAsync(track, CancellationToken.None);

        _constructedTracks.Enqueue(new ConstructedTrack(track, name ?? NextTrackName()));

        return track;
    }

    public async Task<User> UserOrThrowAsync(UserName? name = null)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }


    public async Task<Album> AlbumOrThrowAsync(AlbumName? name = null)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<Track> TrackOrThrowAsync(TrackName? name = null)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    protected async Task BuildAsync()
    {
        // Wait for all scheduled construction of aggregates to complete. Wait in order parent to child aggregate since
        // children typically refer to their parent.
        await Task.WhenAll(_constructionOfArtists);
    }
}

//
// Helper types to keep track on a created aggregate ID by their name. Separate types in order to keep
// tests type-save. I.e. you cannot accidentally mix up an artist and an album reference.
//

internal class ConstructedUser(User user, UserName userName)
{
    public Guid Id { get; } = user.Id;
    public bool IsMatch(ArtistName name) => name.Name == userName.Name;
}

internal class ConstructedArtist(Artist artist, ArtistName artistName)
{
    public Guid Id { get; } = artist.Id;
    public bool IsMatch(ArtistName name) => name.Name == artistName.Name;
}

internal class ConstructedAlbum(Album album, AlbumName albumName)
{
    public Guid Id { get; } = album.Id;
    public bool IsMatch(AlbumName name) => name.Name == albumName.Name;
}

internal class ConstructedTrack(Track track, TrackName trackName)
{
    public Guid Id { get; } = track.Id;
    public bool IsMatch(TrackName name) => name.Name == trackName.Name;
}

//
// The concrete test case for, in this case, a service.
//

public class FavouriteTestCase : TestCaseBuilder
{
    public FavouriteService FavouriteService = null!;

    public async Task<FavouriteTestCase> BuildAsync()
    {
        await Task.CompletedTask;

        FavouriteService = new FavouriteService(TrackRepository);

        return this;
    }
}

public static class TestCaseBuilderExtensions
{
    public static FavouriteTestCase AsFavouriteTestCase(this TestCaseBuilder self) =>
        (FavouriteTestCase) self;
}
