namespace Code;

public class Album : IAggregate
{
    // As this is an example, let's limit each album to one artist.
    public Guid ArtistId { get; set; } = Guid.NewGuid();
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}
