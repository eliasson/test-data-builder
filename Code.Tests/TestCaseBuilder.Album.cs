using System.Collections.Concurrent;

namespace Code.Tests;

public record AlbumName(string Name);

public partial class TestCaseBuilder
{
    //
    // Each aggregate type has support for:
    // - Synchronous initiating construction of instance.
    // - Asynchronous adding a new instance after test case is built.
    // - Retrieve a constructed aggregate by test-name.

    // The repositories used to access any data. This should not be exposed directly to the clients.
    protected readonly Repository<Album> AlbumRepository = new ();

    // Keep of all async tasks of albums to create. These may or may not be completed.
    private readonly IList<Task> _constructionOfAlbums = new List<Task>();

    // Keep track of all ablums that has been constructed.
    private readonly ConcurrentQueue<ConstructedAlbum> _constructedAlbums = new();

    // Instruct the builder to create an album as part of the setup.
    // The album will be created at a later point in time and cannot be accessed until after building the test case.
    public TestCaseBuilder WithAlbum(
        AlbumName? name = null,
        Action<Album>? configure = null,
        UserName? userNamed = null)
    {
        // Since this is a synchronous method we only fire off a task to do the construction without
        // waiting for its result.
        var task = Task.Run(() =>  AddAlbumAsync(name, configure, userNamed));

        // Keep a reference to the construction task so it can be awaited elsewhere.
        _constructionOfAlbums.Add(task);

        return this;
    }

    // Add an album after the building of the test case is completed.
    public async Task<Album> AddAlbumAsync(
        AlbumName? name = null,
        Action<Album>? configure = null,
        UserName? userNamed = null,
        ArtistName? artistNamed = null)
    {
        // Each album is associated with an artist by ID (I know, this is a simplified mode).
        // So we need to get the artist to associate with, by using its test name, or the default album
        // if no name is given.
        var artist = await ArtistOrThrowAsync(artistNamed);

        // The same this is done for the user, since all data is associated with the owning user.
        var user = await UserOrThrowAsync(userNamed);

        // Create an arbitrary album.
        var album = new Album
        {
            ArtistId = artist.Id,
            Name = name?.Name ?? "Arbitrary album"
        };

        // Let the caller modify the album before saving it.
        configure?.Invoke(album);
        await AlbumRepository.SaveAsync(user.Id, album, CancellationToken.None);

        // Remember the created album by its test name so that it can be retrieved later.
        _constructedAlbums.Enqueue(new ConstructedAlbum(album, name ?? NextAlbumName()));

        return album;
    }

    // Fetch an album by its test-name.
    // Using null for the name will return the default created album if only a single album is created.
    //
    // When having a test with multiple users, the user must be qualified with the owning user's test-name.
    public async Task<Album> AlbumOrThrowAsync(
        AlbumName? name = null,
        UserName? userNamed = null)
    {
        await Task.WhenAll(_constructionOfAlbums);

        if (name is null && _constructedAlbums.Count != 1)
            throw new Exception("Implicit access of albums is only allowed when one album exists. Qualify using name.");

        var constructed = name is null
            ? _constructedAlbums.First()
            : _constructedAlbums.FirstOrDefault(i => i.IsMatch(name));

        if (constructed is null)
            throw new Exception($"No album found with the test name: {name?.Name}");

        var user = await UserOrThrowAsync(userNamed);
        return await AlbumRepository.LoadAsync(user.Id, constructed.Id, CancellationToken.None);
    }

    // Generate a default name for each type of aggregate.
    protected AlbumName NextAlbumName() => new($"Artist {_constructedAlbums.Count + 1}");
}

internal class ConstructedAlbum(Album album, AlbumName albumName)
{
    public Guid Id { get; } = album.Id;
    public bool IsMatch(AlbumName name) => name.Name == albumName.Name;
}
