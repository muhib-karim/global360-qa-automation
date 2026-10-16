using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Components;

/// <summary>The demo's Select2 dropdowns, addressed by the id of the hidden native select.</summary>
internal sealed class Select2(IPage page, string selectId)
{
    private ILocator Options => page.Locator(".select2-results__option[role='option'][aria-selected]");

    public ILocator SelectedOption => page.Locator($"#{selectId} option:checked");

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
