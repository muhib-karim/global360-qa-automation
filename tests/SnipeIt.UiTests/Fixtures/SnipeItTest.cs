using System.Globalization;
using System.Text.Json;
using Allure.Net.Commons;
using Allure.NUnit;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;
using SnipeIt.UiTests.Config;
using SnipeIt.UiTests.Logging;

namespace SnipeIt.UiTests.Fixtures;

/// <summary>
/// Base for browser tests: a fresh context per test (no shared state, safe to run in parallel), tracing and video,
/// failure screenshot and HTML, run metadata, all attached to the NUnit and Allure results.
/// </summary>
[AllureNUnit]
public abstract class SnipeItTest : BrowserTest
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    /// <summary>When the test started (UTC); History timestamps are checked against it.</summary>
    private protected DateTime StartedUtc { get; private set; }

    private protected TestSettings Settings { get; private set; } = null!;
    private protected IBrowserContext Context { get; private set; } = null!;
    private protected IPage Page { get; private set; } = null!;
    private protected ILogger Logger { get; private set; } = null!;
    private protected string ArtifactsDir { get; private set; } = null!;

    /// <summary>Written to run.json in every outcome; the CI job summary reads it.</summary>
    private protected Dictionary<string, object?> RunInfo { get; private set; } = [];

    [SetUp]
    public async Task StartContextAsync()
    {
        StartedUtc = DateTime.UtcNow;
        Settings = TestSettings.Load();
        Logger = NUnitLoggerProvider.Factory.CreateLogger(GetType().Name);
        RunInfo = new() { ["browser"] = BrowserName, ["browserVersion"] = Browser.Version };
        var root = Settings.Diagnostics.OutputDir
                   ?? Path.Join(TestContext.CurrentContext.WorkDirectory, "artifacts");
        ArtifactsDir = Path.GetFullPath(Path.Join(root, string.Create(CultureInfo.InvariantCulture,
            $"{BrowserName}-{TestContext.CurrentContext.Test.MethodName}-{StartedUtc:yyyyMMddTHHmmss}")));
        Directory.CreateDirectory(ArtifactsDir);

        Context = await Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 1000 },
            Locale = "en-US",
            RecordVideoDir = Settings.Diagnostics.Video == ArtifactMode.Off ? null : Path.Join(ArtifactsDir, "video"),
        });
        Context.SetDefaultTimeout(Settings.Snipe.TimeoutMs);
        Assertions.SetDefaultExpectTimeout(Settings.Snipe.TimeoutMs);
        if (Settings.Diagnostics.Trace != ArtifactMode.Off)
            await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
        Page = await Context.NewPageAsync();

        // One result per browser in the merged Allure report, instead of three "retries" of the same test.
        AllureLifecycle.Instance.UpdateTestCase(result =>
        {
            result.name = $"{result.name} [{BrowserName}]";
            result.historyId = $"{result.fullName}[{BrowserName}]";
        });
        AllureApi.AddTestParameter(new Parameter { name = "browser", value = BrowserName });
        Logger.RunStarted(BrowserName, Browser.Version, Settings.Snipe.BaseUrl);
    }

    [TearDown]
    public async Task StopContextAsync()
    {
        var outcome = TestContext.CurrentContext.Result.Outcome.Status;
        var failed = outcome == TestStatus.Failed;
        if (failed)
        {
            await TryAsync("failure screenshot", () =>
                Page.ScreenshotAsync(new() { Path = Path.Join(ArtifactsDir, "failure.png"), FullPage = true }));
            await TryAsync("failure HTML", async () =>
                await File.WriteAllTextAsync(Path.Join(ArtifactsDir, "failure.html"), await Page.ContentAsync()));
        }
        if (Keep(Settings.Diagnostics.Trace, failed))
            await TryAsync("trace", () => Context.Tracing.StopAsync(new() { Path = Path.Join(ArtifactsDir, "trace.zip") }));
        RunInfo["outcome"] = outcome.ToString();
        RunInfo["url"] = Page.Url;
        RunInfo["durationSeconds"] = Math.Round((DateTime.UtcNow - StartedUtc).TotalSeconds, 1);
        await File.WriteAllTextAsync(Path.Join(ArtifactsDir, "run.json"), JsonSerializer.Serialize(RunInfo, Json));

        // The video file is complete only after the context closes.
        await TryAsync("browser context", () => Context.CloseAsync());
        var videoDir = Path.Join(ArtifactsDir, "video");
        if (!Keep(Settings.Diagnostics.Video, failed) && Directory.Exists(videoDir))
            Directory.Delete(videoDir, recursive: true);

        foreach (var file in Directory.EnumerateFiles(ArtifactsDir, "*", SearchOption.AllDirectories))
        {
            TestContext.AddTestAttachment(file);
            AllureApi.AddAttachment(file);
        }
        Logger.ArtifactsSaved(outcome, ArtifactsDir);
    }

    /// <summary>Logs the step and records it as an Allure step.</summary>
    private protected Task StepAsync(string name, Func<Task> action)
    {
        Logger.Step(name);
        return AllureApi.Step(name, action);
    }

    private protected Task<T> StepAsync<T>(string name, Func<Task<T>> action)
    {
        Logger.Step(name);
        return AllureApi.Step(name, action);
    }

    /// <summary>Small viewport JPEG kept as evidence of a verified page.</summary>
    private protected Task EvidenceAsync(string name) =>
        Page.ScreenshotAsync(new() { Path = Path.Join(ArtifactsDir, $"{name}.jpg"), Type = ScreenshotType.Jpeg, Quality = 70 });

    private static bool Keep(ArtifactMode mode, bool failed) =>
        mode == ArtifactMode.On || (mode == ArtifactMode.RetainOnFailure && failed);

    // Diagnostics must never replace the original failure with a secondary one.
    private async Task TryAsync(string artifact, Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception e) when (e is PlaywrightException or IOException)
        {
            Logger.ArtifactFailed(artifact, e.Message);
        }
    }
}
