using System.Collections.Concurrent;

namespace Code.Tests;

public record TrackName(string Name);

public abstract partial class TestCaseBuilder
{
    // See TestCaseBuilder.Album.cs for comments.

    protected readonly Repository<Track> TrackRepository = new ();
    private readonly IList<Task> _constructionOfTracks = new List<Task>();
    private readonly ConcurrentQueue<ConstructedTrack> _constructedTracks = new();

    public TestCaseBuilder WithTrack(
        TrackName? name = null,
        Action<Track>? configure = null,
        UserName? userNamed = null,
        AlbumName? albumNamed = null)
    {
        var task = Task.Run(() =>  AddTrackAsync(name, configure, userNamed, albumNamed));
        _constructionOfTracks.Add(task);
        return this;
    }

    public async Task<Track> AddTrackAsync(
        TrackName? name = null,
        Action<Track>? configure = null,
        UserName? userNamed = null,
        AlbumName? albumNamed = null)
    {
        var album = await AlbumOrThrowAsync(albumNamed);
        var user = await UserOrThrowAsync(userNamed);

        var track = new Track
        {
            AlbumId = album.Id,
            Title = name?.Name ?? "Arbitrary track"
        };
        configure?.Invoke(track);

        await TrackRepository.SaveAsync(user.Id, track, CancellationToken.None);

        _constructedTracks.Enqueue(new ConstructedTrack(track, name ?? NextTrackName()));

        return track;
    }

    public async Task<Track> TrackOrThrowAsync(
        TrackName? name = null,
        UserName? userNamed = null)
    {
        await Task.WhenAll(_constructionOfTracks);

        if (name is null && _constructedTracks.Count != 1)
            throw new Exception("Implicit access of tracks is only allowed when one track exists. Qualify using name.");

        var constructed = name is null
            ? _constructedTracks.First()
            : _constructedTracks.FirstOrDefault(i => i.IsMatch(name));

        if (constructed is null)
            throw new Exception($"No track found with the test name: {name?.Name}");

        var user = await UserOrThrowAsync(userNamed);
        return await TrackRepository.LoadAsync(user.Id, constructed.Id, CancellationToken.None);
    }

    protected TrackName NextTrackName() => new($"Artist {_constructedTracks.Count + 1}");
}

internal class ConstructedTrack(Track track, TrackName trackName)
{
    public Guid Id { get; } = track.Id;
    public bool IsMatch(TrackName name) => name.Name == trackName.Name;
}