using Microsoft.Extensions.Logging;

namespace SnipeIt.UiTests.Logging;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "Run started: browser {Browser} {BrowserVersion}, target {BaseUrl}")]
    public static partial void RunStarted(this ILogger logger, string browser, string browserVersion, Uri baseUrl);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Step: {Step}")]
    public static partial void Step(this ILogger logger, string step);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information,
        Message = "Asset tag {AssetTag}, name {AssetName}, serial {Serial}, user seed {UserSeed}")]
    public static partial void AssetPrepared(this ILogger logger, string assetTag, string assetName, string serial, int userSeed);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information,
        Message = "Selected user {UserName} (id {UserId}) from option '{OptionText}'")]
    public static partial void UserSelected(this ILogger logger, string userName, string userId, string optionText);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Verified asset page {Url}")]
    public static partial void AssetVerified(this ILogger logger, string url);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information,
        Message = "Accessibility {PageName}: {Violations} violated rules, critical rules [{CriticalRules}]")]
    public static partial void AxeScanned(this ILogger logger, string pageName, int violations, string criticalRules);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "API cross-check: {Detail}")]
    public static partial void ApiChecked(this ILogger logger, string detail);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Artifacts ({Outcome}): {Directory}")]
    public static partial void ArtifactsSaved(this ILogger logger, string outcome, string directory);

    [LoggerMessage(EventId = 9, Level = LogLevel.Warning, Message = "Could not save {Artifact}: {Reason}")]
    public static partial void ArtifactFailed(this ILogger logger, string artifact, string reason);
}
