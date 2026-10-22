using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Components;

/// <summary>
/// The asset form's collapsible "Optional Information" section. The page saves its open state in a cookie and
/// replays it with a click on document ready, so a blind click can close a section the replay just opened (seen
/// on the edit form after the create form opened it). The caret icon flips with every toggle, so it is the state.
/// </summary>
internal sealed class OptionalInfoSection(IPage page)
{
    private ILocator OpenCaret => page.Locator("#optional_info_icon.fa-caret-down");

    /// <summary>Opens the section unless it is already open, then waits until the name field shows.</summary>
    public async Task OpenAsync()
    {
        // After the load event the replay has run, or will not run.
        await page.WaitForLoadStateAsync(LoadState.Load);
        if (await OpenCaret.CountAsync() == 0)
            await page.Locator("#optional_info").ClickAsync();
        await Expect(OpenCaret).ToHaveCountAsync(1);
        await Expect(page.Locator("#name")).ToBeVisibleAsync();
    }
}
