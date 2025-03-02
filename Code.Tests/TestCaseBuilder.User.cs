using System.Collections.Concurrent;

namespace Code.Tests;

public record UserName(string Name);

public partial class TestCaseBuilder
{
    // See TestCaseBuilder.Album.cs for comments.

    protected readonly UserRepository UserRepository = new ();
    private readonly IList<Task> _constructionOfUsers = new List<Task>();
    private readonly ConcurrentQueue<ConstructedUser> _constructedUsers = new();

    public TestCaseBuilder WithUser(UserName? name = null, Action<User>? configure = null)
    {
        var user = Task.Run(() =>  AddUserAsync(name, configure));
        _constructionOfUsers.Add(user);
        return this;
    }

    public async Task<User> AddUserAsync(UserName? name = null, Action<User>? configure = null)
    {
        var user = new User { Username = name?.Name ?? "emma.goldman" };
        configure?.Invoke(user);
        await UserRepository.SaveAsync(user, CancellationToken.None);

        _constructedUsers.Enqueue(new ConstructedUser(user, name ?? NextUserName()));

        return user;
    }

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

    protected UserName NextUserName() => new($"Artist {_constructedUsers.Count + 1}");
}

internal class ConstructedUser(User user, UserName userName)
{
    public Guid Id { get; } = user.Id;
    public bool IsMatch(UserName name) => name.Name == userName.Name;
}