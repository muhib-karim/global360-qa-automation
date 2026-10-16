using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Pages;

internal sealed class Select2(IPage page, string selectId)
{
    private ILocator Options => page.Locator(".select2-results__option[role='option'][aria-selected]");

    public Task OpenAsync() => page.Locator($"#select2-{selectId}-container").ClickAsync();

    public async Task SearchAndSelectAsync(string search, string expectedText)
    {
        await OpenAsync();
        await page.Locator(".select2-search__field").FillAsync(search);
        var option = Options.Filter(new() { HasText = expectedText });
        await Expect(option).ToHaveCountAsync(1);
        await option.ClickAsync();
    }

    public async Task SelectExactAsync(string text)
    {
        await OpenAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = text, Exact = true }).ClickAsync();
    }

    public async Task<string> SelectRandomAsync(Random random)
    {
        await OpenAsync();
        await Expect(Options.First).ToBeVisibleAsync();
        var count = await Options.CountAsync();
        var option = Options.Nth(random.Next(count));
        var text = (await option.InnerTextAsync()).Trim();
        await option.ClickAsync();
        return text;
    }
}
