using System.Collections.Concurrent;

namespace Code.Tests;

public record UserName(string Name);

public partial class TestCaseBuilder
{
    protected Repository<User> UserRepository = new ();

    private readonly IList<Task> _constructionOfUsers = new List<Task>();

    private readonly ConcurrentQueue<ConstructedUser> _constructedUsers = new();

    protected UserName NextUserName() => new($"Artist {_constructedUsers.Count + 1}");


    public async Task<User> UserOrThrowAsync(UserName? name = null)
    {
        await Task.WhenAll(_constructionOfUsers);

        if (name is null && _constructedUsers.Count != 1)
            throw new Exception("Implicit access of users is only allowed when one user exists. Qualify using name.");

        var constructed = name is null
            ? _constructedUsers.First()
            : _constructedUsers.FirstOrDefault(i => i.IsMatch(name));

        if (constructed is null)
            throw new Exception($"No user found with the test name: {name?.Name}");

        return await UserRepository.LoadAsync(constructed.Id, CancellationToken.None);
    }
}

internal class ConstructedUser(User user, UserName userName)
{
    public Guid Id { get; } = user.Id;
    public bool IsMatch(UserName name) => name.Name == userName.Name;
}