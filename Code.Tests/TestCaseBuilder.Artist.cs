using System.Collections.Concurrent;

namespace Code.Tests;

public record ArtistName(string Name);

public partial class TestCaseBuilder
{
    protected readonly Repository<Artist> ArtistRepository = new ();

    private readonly IList<Task> _constructionOfArtists = new List<Task>();
    private readonly ConcurrentQueue<ConstructedArtist> _constructedArtists = new();


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
        await Task.WhenAll(_constructionOfArtists);

        if (name is null && _constructedArtists.Count != 1)
            throw new Exception("Implicit access of artists is only allowed when one artist exists. Qualify using name.");

        var constructed = name is null
            ? _constructedArtists.First()
            : _constructedArtists.FirstOrDefault(i => i.IsMatch(name));

        if (constructed is null)
            throw new Exception($"No artist found with the test name: {name?.Name}");

        return await ArtistRepository.LoadAsync(constructed.Id, CancellationToken.None);
    }


    protected ArtistName NextArtistName() => new($"Artist {_constructedArtists.Count + 1}");
}

internal class ConstructedArtist(Artist artist, ArtistName artistName)
{
    public Guid Id { get; } = artist.Id;
    public bool IsMatch(ArtistName name) => name.Name == artistName.Name;
}