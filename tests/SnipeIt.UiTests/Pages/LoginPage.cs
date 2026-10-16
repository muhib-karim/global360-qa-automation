using Microsoft.Playwright;
using SnipeIt.UiTests.Support;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Pages;

internal sealed class LoginPage(IPage page, TestSettings settings)
{
    public async Task<string> SignInAsync()
    {
        await page.GotoAsync(new Uri(settings.BaseUrl, "/login").ToString());
        await page.GetByLabel("Username").FillAsync(settings.Username);
        await page.Locator("input[name='password']").FillAsync(settings.Password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Login", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard", Exact = true }))
            .ToBeVisibleAsync();
        return (await page.Locator(".user-menu.user > a").InnerTextAsync()).Trim();
    }
}
