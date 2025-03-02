namespace Code;

public class Track : IAggregate
{
    public Guid AlbumId { get; set; } = Guid.NewGuid();
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public bool IsFavorite { get; private set;  } = false;
    
    public void MarkAsFavourite() => IsFavorite = true;
    public void UnmarkAsFavourite() => IsFavorite = false;
}
