using System.Collections.Concurrent;

namespace Code.Tests;

public record TrackName(string Name);

public abstract partial class TestCaseBuilder
{
    protected Repository<Track> TrackRepository = new ();
    private readonly IList<Task> _constructionOfTracks = new List<Task>();

    private readonly ConcurrentQueue<ConstructedTrack> _constructedTracks = new();

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

    public async Task<Track> TrackOrThrowAsync(TrackName? name = null)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    protected TrackName NextTrackName() => new($"Artist {_constructedTracks.Count + 1}");
}

internal class ConstructedTrack(Track track, TrackName trackName)
{
    public Guid Id { get; } = track.Id;
    public bool IsMatch(TrackName name) => name.Name == trackName.Name;
}