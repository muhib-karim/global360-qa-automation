using SnipeIt.UiTests.Fixtures;
using SnipeIt.UiTests.Pages;

namespace SnipeIt.UiTests.Tests;

[TestFixture]
[Category("E2E")]
[Parallelizable(ParallelScope.Self)]
public sealed class LoginTests : SnipeItTest
{
    [Test]
    public async Task Wrong_password_is_rejected()
    {
        var login = new LoginPage(Page, Settings.Snipe);
        await StepAsync("Open the login page", login.OpenAsync);
        await StepAsync("Sign in with the wrong password",
            () => login.SignInWithWrongPasswordAsync($"not-the-password-{Guid.NewGuid().ToString("N")}"));
        await EvidenceAsync("1-login-rejected");
    }
}
