using Microsoft.Playwright;
using Xunit;

namespace ValyanClinic.Tests.Integration;

/// <summary>
/// Base class for Playwright integration tests.
/// Handles browser lifecycle management and provides common utilities for E2E testing.
/// </summary>
/// <remarks>
/// Pattern: IAsyncLifetime for async setup/teardown
/// Browser: Chromium (default, can switch to Firefox/WebKit)
/// Features: Auto-wait, screenshots, video recording, network interception
/// </remarks>
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    /// <summary>
    /// Playwright instance for browser automation.
    /// </summary>
    protected IPlaywright Playwright { get; private set; } = default!;

    /// <summary>
    /// Browser instance (Chromium by default).
    /// </summary>
    protected IBrowser Browser { get; private set; } = default!;

    /// <summary>
    /// Browser context for isolated test execution.
    /// </summary>
    protected IBrowserContext Context { get; private set; } = default!;

    /// <summary>
    /// Page instance for interacting with web pages.
    /// </summary>
    protected IPage Page { get; private set; } = default!;

    /// <summary>
    /// Base URL for ValyanClinic application.
    /// Update this to match your development/staging environment.
    /// Default: https://localhost:7164 (configured in launchSettings.json)
    /// Alternative HTTP: http://localhost:5007
    /// </summary>
    protected virtual string BaseUrl { get; } = "https://localhost:7164";

    /// <summary>
    /// Indicates if browser should run in headless mode.
    /// Set to false for debugging and seeing browser interactions.
    /// </summary>
    protected virtual bool Headless { get; } = true;

    /// <summary>
    /// Slow motion delay in milliseconds for debugging.
    /// Set to 0 for maximum speed, 50-100 for visible actions.
    /// </summary>
    protected virtual int SlowMo { get; } = 0;

    /// <summary>
    /// Initializes Playwright, browser, context, and page before each test.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Create Playwright instance
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        // Launch browser with configuration
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Headless,
            SlowMo = SlowMo,
            Args = new[] { "--start-maximized" }
        });

        // Create isolated browser context
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            RecordVideoDir = "videos/", // Record test execution videos
            IgnoreHTTPSErrors = true // For localhost development
        });

        // Create page
        Page = await Context.NewPageAsync();

        // Setup console message logging (optional)
        Page.Console += (_, msg) =>
        {
            Console.WriteLine($"[Browser Console {msg.Type}]: {msg.Text}");
        };

        // Setup page error logging
        Page.PageError += (_, error) =>
        {
            Console.WriteLine($"[Browser Error]: {error}");
        };
    }

    /// <summary>
    /// Cleans up Playwright resources after each test.
    /// </summary>
    public async Task DisposeAsync()
    {
        // Close page
        if (Page != null)
        {
            await Page.CloseAsync();
        }

        // Close context (saves video if configured)
        if (Context != null)
        {
            await Context.CloseAsync();
        }

        // Close browser
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }

        // Dispose Playwright
        Playwright?.Dispose();
    }

    /// <summary>
    /// Navigates to a specific page relative to BaseUrl and waits for load.
    /// </summary>
    /// <param name="relativeUrl">Relative URL path (e.g., "/pacienti/vizualizare")</param>
    /// <param name="waitUntil">Wait strategy (default: NetworkIdle for full page load)</param>
    protected async Task NavigateToAsync(string relativeUrl, WaitUntilState waitUntil = WaitUntilState.NetworkIdle)
    {
        var fullUrl = $"{BaseUrl}{relativeUrl}";
        await Page.GotoAsync(fullUrl, new PageGotoOptions
        {
            WaitUntil = waitUntil,
            Timeout = 30000 // 30 seconds timeout
        });
    }

    /// <summary>
    /// Waits for Blazor Server to finish initialization (SignalR connection ready).
    /// </summary>
    /// <remarks>
    /// Blazor Server requires SignalR connection before components can interact.
    /// This method waits for the global Blazor object to be available.
    /// </remarks>
    protected async Task WaitForBlazorAsync()
    {
        await Page.WaitForFunctionAsync("() => window.Blazor !== undefined", new PageWaitForFunctionOptions
        {
            Timeout = 10000 // 10 seconds timeout
        });

        // Additional wait for Blazor to fully initialize
        await Task.Delay(500);
    }

    /// <summary>
    /// Takes a screenshot of the current page state.
    /// Useful for debugging and visual regression testing.
    /// </summary>
    /// <param name="filename">Screenshot filename (saved to screenshots/ folder)</param>
    protected async Task TakeScreenshotAsync(string filename)
    {
        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = $"screenshots/{filename}",
            FullPage = true
        });
    }

    /// <summary>
    /// Waits for a specific element to be visible on the page.
    /// </summary>
    /// <param name="selector">CSS selector for the element</param>
    /// <param name="timeout">Timeout in milliseconds (default: 5000)</param>
    protected async Task WaitForElementAsync(string selector, int timeout = 5000)
    {
        await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeout
        });
    }

    /// <summary>
    /// Waits for a specific text to appear on the page.
    /// </summary>
    /// <param name="text">Text to wait for</param>
    /// <param name="timeout">Timeout in milliseconds (default: 5000)</param>
    protected async Task WaitForTextAsync(string text, int timeout = 5000)
    {
        await Page.Locator($"text={text}").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeout
        });
    }

    /// <summary>
    /// Fills an input field with the specified value.
    /// </summary>
    /// <param name="selector">CSS selector for the input</param>
    /// <param name="value">Value to fill</param>
    protected async Task FillInputAsync(string selector, string value)
    {
        await Page.FillAsync(selector, value);
    }

    /// <summary>
    /// Clicks an element on the page.
    /// </summary>
    /// <param name="selector">CSS selector for the element</param>
    protected async Task ClickAsync(string selector)
    {
        await Page.ClickAsync(selector);
    }

    /// <summary>
    /// Waits for network to be idle (no pending requests).
    /// Useful after actions that trigger data fetching.
    /// </summary>
    protected async Task WaitForNetworkIdleAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Intercepts network requests matching the URL pattern.
    /// Useful for mocking API responses or testing error scenarios.
    /// </summary>
    /// <param name="urlPattern">URL pattern to intercept (supports wildcards)</param>
    /// <param name="handler">Handler function for the intercepted route</param>
    protected async Task InterceptNetworkAsync(string urlPattern, Func<IRoute, Task> handler)
    {
        await Page.RouteAsync(urlPattern, handler);
    }

    /// <summary>
    /// Gets the text content of an element.
    /// </summary>
    /// <param name="selector">CSS selector for the element</param>
    /// <returns>Text content of the element</returns>
    protected async Task<string?> GetTextContentAsync(string selector)
    {
        return await Page.TextContentAsync(selector);
    }

    /// <summary>
    /// Checks if an element is visible on the page.
    /// </summary>
    /// <param name="selector">CSS selector for the element</param>
    /// <returns>True if visible, false otherwise</returns>
    protected async Task<bool> IsVisibleAsync(string selector)
    {
        return await Page.IsVisibleAsync(selector);
    }

    /// <summary>
    /// Gets the count of elements matching the selector.
    /// </summary>
    /// <param name="selector">CSS selector for the elements</param>
    /// <returns>Count of matching elements</returns>
    protected async Task<int> GetElementCountAsync(string selector)
    {
        var locator = Page.Locator(selector);
        return await locator.CountAsync();
    }
}
