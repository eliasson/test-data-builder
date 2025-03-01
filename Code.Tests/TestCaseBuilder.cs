namespace Code.Tests;

public abstract partial class TestCaseBuilder
{
    protected async Task BuildConstructedAggregatesAsync()
    {
        // Wait for all scheduled construction of aggregates to complete. Wait in order parent to child aggregate since
        // children typically refer to their parent.
        await Task.WhenAll(_constructionOfArtists);
    }
}

public class FavouriteTestCase : TestCaseBuilder
{
    public FavouriteService FavouriteService = null!;

    public async Task<FavouriteTestCase> BuildAsync()
    {
        await BuildConstructedAggregatesAsync();

        FavouriteService = new FavouriteService(TrackRepository);

        return this;
    }
}

public static class TestCaseBuilderExtensions
{
    public static FavouriteTestCase AsFavouriteTestCase(this TestCaseBuilder self) =>
        (FavouriteTestCase) self;
}
