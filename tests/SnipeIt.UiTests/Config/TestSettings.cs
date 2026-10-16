using Microsoft.Extensions.Configuration;

namespace SnipeIt.UiTests.Config;

internal enum ArtifactMode
{
    Off,
    On,
    RetainOnFailure,
}

internal sealed class SnipeSettings
{
    public Uri BaseUrl { get; init; } = new("https://demo.snipeitapp.com");
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
    public int? UserSeed { get; init; }
    public int TimeoutMs { get; init; } = 20_000;
}

internal sealed class DiagnosticsSettings
{
    public ArtifactMode Trace { get; init; } = ArtifactMode.RetainOnFailure;
    public ArtifactMode Video { get; init; } = ArtifactMode.RetainOnFailure;
    public string? OutputDir { get; init; }
}

internal sealed class AccessibilitySettings
{
    public bool Enforce { get; init; } = true;
}

/// <summary>appsettings.json, then environment variables (Snipe__BaseUrl style), then the original SNIPE_* names.</summary>
internal sealed class TestSettings
{
    public const string AllowedHost = "demo.snipeitapp.com";

    private static readonly (string Variable, string Key)[] LegacyVariables =
    [
        ("SNIPE_BASE_URL", "Snipe:BaseUrl"),
        ("SNIPE_USERNAME", "Snipe:Username"),
        ("SNIPE_PASSWORD", "Snipe:Password"),
        ("SNIPE_USER_SEED", "Snipe:UserSeed"),
    ];

    public SnipeSettings Snipe { get; init; } = new();
    public DiagnosticsSettings Diagnostics { get; init; } = new();
    public AccessibilitySettings Accessibility { get; init; } = new();

    public static TestSettings Load(Func<string, string?>? environment = null)
    {
        environment ??= Environment.GetEnvironmentVariable;
        var legacy = LegacyVariables
            .Select(v => new KeyValuePair<string, string?>(v.Key, environment(v.Variable)))
            .Where(v => v.Value is not null);
        var settings = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(legacy)
            .Build()
            .Get<TestSettings>() ?? new TestSettings();

        var url = settings.Snipe.BaseUrl;
        // The suite creates records, so it must never point at a real inventory by accident.
        if (url.Scheme != Uri.UriSchemeHttps || url.Host != AllowedHost)
            throw new InvalidOperationException($"This suite writes demo data. Only https://{AllowedHost} is allowed, got '{url}'.");
        if (string.IsNullOrWhiteSpace(settings.Snipe.Username) || string.IsNullOrWhiteSpace(settings.Snipe.Password))
            throw new InvalidOperationException("Snipe:Username and Snipe:Password must be set.");
        return settings;
    }
}
