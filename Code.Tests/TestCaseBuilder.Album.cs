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

    // The repositories used to access all data. This should not be exposed directly to the clients.
    protected Repository<Album> AlbumRepository = new ();

    // Keep of all async tasks of artists to create. These may or may not be completed.
    private readonly IList<Task> _constructionOfAlbums = new List<Task>();

    // Keep track of all aggregates that has been constructed.
    private readonly ConcurrentQueue<ConstructedAlbum> _constructedAlbums = new();

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

    public async Task<Album> AlbumOrThrowAsync(AlbumName? name = null)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    // Generate a default name for each type of aggregate.
    protected AlbumName NextAlbumName() => new($"Artist {_constructedAlbums.Count + 1}");
}

internal class ConstructedAlbum(Album album, AlbumName albumName)
{
    public Guid Id { get; } = album.Id;
    public bool IsMatch(AlbumName name) => name.Name == albumName.Name;
}
