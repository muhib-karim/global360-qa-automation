using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Components;

/// <summary>
/// The asset form's collapsible "Optional Information" section. The page saves its open state in a cookie and
/// replays it with a click on document ready, so a blind click can close a section the replay just opened (seen
/// on the edit form after the create form opened it). The caret icon flips with every toggle, so it is the state.
/// </summary>
internal sealed partial class OptionalInfoSection(IPage page)
{
    public async Task OpenAsync()
    {
        // After the load event the replay has run, or will not run.
        await page.WaitForLoadStateAsync(LoadState.Load);
        var icon = page.Locator("#optional_info_icon");
        if (!Open().IsMatch(await icon.GetAttributeAsync("class") ?? ""))
            await page.Locator("#optional_info").ClickAsync();
        await Expect(icon).ToHaveClassAsync(Open());
        await Expect(page.Locator("#name")).ToBeVisibleAsync();
    }

    [GeneratedRegex(@"\bfa-caret-down\b")]
    private static partial Regex Open();
}
