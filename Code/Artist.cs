namespace Code;

public class Artist : IAggregate
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}
