using Microsoft.Playwright;
using SnipeIt.UiTests.Config;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Pages;

internal sealed class LoginPage(IPage page, SnipeSettings settings)
{
    public async Task OpenAsync()
    {
        var response = await page.GotoAsync(new Uri(settings.BaseUrl, "/login").ToString());
        // The shared demo answers 503 with a "Resetting the database" page while it restores its data.
        if (response is { Status: >= 500 })
            throw new InvalidOperationException(
                $"Snipe-IT demo unavailable: HTTP {response.Status} for /login (it returns 503 while its database resets).");
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Login", Exact = true })).ToBeVisibleAsync();
    }

    /// <returns>The signed-in user's display name, which History records as the actor.</returns>
    public async Task<string> SignInAsync()
    {
        await page.GetByLabel("Username").FillAsync(settings.Username);
        await page.Locator("input[name='password']").FillAsync(settings.Password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Login", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard", Exact = true }))
            .ToBeVisibleAsync();
        return (await page.Locator(".user-menu.user > a").InnerTextAsync()).Trim();
    }

    /// <summary>Submits the wrong password and verifies the sign-in is refused: the error is shown and the login page stays.</summary>
    public async Task SignInWithWrongPasswordAsync(string wrongPassword)
    {
        await page.GetByLabel("Username").FillAsync(settings.Username);
        await page.Locator("input[name='password']").FillAsync(wrongPassword);
        await page.GetByRole(AriaRole.Button, new() { Name = "Login", Exact = true }).ClickAsync();
        await Expect(page.Locator(".alert-danger")).ToContainTextAsync("The username or password is incorrect");
        await Expect(page).ToHaveURLAsync(new Uri(settings.BaseUrl, "/login").ToString());
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Login", Exact = true })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard", Exact = true })).ToHaveCountAsync(0);
    }
}
