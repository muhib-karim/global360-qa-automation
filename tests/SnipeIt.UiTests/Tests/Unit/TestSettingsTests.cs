using SnipeIt.UiTests.Config;

namespace SnipeIt.UiTests.Tests.Unit;

[TestFixture]
[Category("Unit")]
public sealed class TestSettingsTests
{
    [Test]
    public void Defaults_target_the_public_demo()
    {
        var settings = TestSettings.Load(_ => null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.Snipe.BaseUrl, Is.EqualTo(new Uri("https://demo.snipeitapp.com")));
            Assert.That(settings.Snipe.Username, Is.EqualTo("admin"));
            Assert.That(settings.Snipe.UserSeed, Is.Null);
            Assert.That(settings.Diagnostics.Trace, Is.EqualTo(ArtifactMode.RetainOnFailure));
        }
    }

    [Test]
    public void Original_SNIPE_variables_still_override_appsettings()
    {
        var environment = new Dictionary<string, string>
        {
            ["SNIPE_USERNAME"] = "someone",
            ["SNIPE_PASSWORD"] = "secret",
            ["SNIPE_USER_SEED"] = "42",
        };

        var settings = TestSettings.Load(name => environment.GetValueOrDefault(name));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.Snipe.Username, Is.EqualTo("someone"));
            Assert.That(settings.Snipe.Password, Is.EqualTo("secret"));
            Assert.That(settings.Snipe.UserSeed, Is.EqualTo(42));
        }
    }

    [TestCase("http://demo.snipeitapp.com")]
    [TestCase("https://example.com")]
    [TestCase("https://demo.snipeitapp.com.evil.test")]
    public void Refuses_any_target_other_than_the_https_demo(string url) =>
        Assert.That(() => TestSettings.Load(name => name == "SNIPE_BASE_URL" ? url : null),
            Throws.InvalidOperationException);
}
