using System.Collections.Concurrent;

namespace Code.Tests;

public record ArtistName(string Name);

public partial class TestCaseBuilder
{
    // See TestCaseBuilder.Album.cs for comments.

    protected readonly Repository<Artist> ArtistRepository = new ();
    private readonly IList<Task> _constructionOfArtists = new List<Task>();
    private readonly ConcurrentQueue<ConstructedArtist> _constructedArtists = new();

    public TestCaseBuilder WithArtist(
        ArtistName? name = null,
        Action<Artist>? configure = null,
        UserName? userNamed = null)
    {
        var task = Task.Run(() =>  AddArtistAsync(name, configure, userNamed));
        _constructionOfArtists.Add(task);
        return this;
    }

    public async Task<Artist> AddArtistAsync(
        ArtistName? name = null,
        Action<Artist>? configure = null,
        UserName? userNamed = null)
    {
        var user = await UserOrThrowAsync(userNamed);

        var artist = new Artist
        {
            UserId = user.Id,
            Name = name?.Name ?? "Arbitrary artist"
        };

        configure?.Invoke(artist);
        await ArtistRepository.SaveAsync(user.Id, artist, CancellationToken.None);

        _constructedArtists.Enqueue(new ConstructedArtist(artist, name ?? NextArtistName()));

        return artist;
    }

    public async Task<Artist> ArtistOrThrowAsync(
        ArtistName? name = null,
        UserName? userNamed = null)
    {
        await Task.WhenAll(_constructionOfArtists);

        if (name is null && _constructedArtists.Count != 1)
            throw new Exception("Implicit access of artists is only allowed when one artist exists. Qualify using name.");

        var constructed = name is null
            ? _constructedArtists.First()
            : _constructedArtists.FirstOrDefault(i => i.IsMatch(name));

        if (constructed is null)
            throw new Exception($"No artist found with the test name: {name?.Name}");

        var user = await UserOrThrowAsync(userNamed);
        return await ArtistRepository.LoadAsync(user.Id, constructed.Id, CancellationToken.None);
    }

    protected ArtistName NextArtistName() => new($"Artist {_constructedArtists.Count + 1}");
}

internal class ConstructedArtist(Artist artist, ArtistName artistName)
{
    public Guid Id { get; } = artist.Id;
    public bool IsMatch(ArtistName name) => name.Name == artistName.Name;
}