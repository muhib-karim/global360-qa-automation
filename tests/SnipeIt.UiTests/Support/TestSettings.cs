namespace SnipeIt.UiTests.Support;

internal sealed record TestSettings(Uri BaseUrl, string Username, string Password, int UserSeed)
{
    public static TestSettings FromEnvironment()
    {
        var url = new Uri(Environment.GetEnvironmentVariable("SNIPE_BASE_URL")
                          ?? "https://demo.snipeitapp.com");
        if (url.Scheme != Uri.UriSchemeHttps || url.Host != "demo.snipeitapp.com")
            throw new InvalidOperationException("This suite writes demo data. Only https://demo.snipeitapp.com is allowed.");
        var seedText = Environment.GetEnvironmentVariable("SNIPE_USER_SEED");
        var seed = seedText is null ? Random.Shared.Next() : int.Parse(seedText);
        return new TestSettings(url,
            Environment.GetEnvironmentVariable("SNIPE_USERNAME") ?? "admin",
            Environment.GetEnvironmentVariable("SNIPE_PASSWORD") ?? "password", seed);
    }
}
