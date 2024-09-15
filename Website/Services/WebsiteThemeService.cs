using Blazored.LocalStorage;

namespace Website.Services;

public class WebsiteThemeService
{
    private const string WebsiteThemeKey = "website_theme";
    private readonly ISyncLocalStorageService _syncLocalStorage;

    private WebsiteTheme _cachedWebsiteTheme;

    public WebsiteThemeService(ISyncLocalStorageService syncLocalStorageService)
    {
        _syncLocalStorage = syncLocalStorageService;
        _cachedWebsiteTheme = _syncLocalStorage.GetItem<WebsiteTheme>(WebsiteThemeKey);
    }

    public WebsiteTheme WebsiteTheme
    {
        get => _cachedWebsiteTheme;
        set
        {
            _cachedWebsiteTheme = value;
            _syncLocalStorage.SetItem(WebsiteThemeKey, value);
            OnWebsiteThemeChanged.Invoke();
        }
    }

    public event Action OnWebsiteThemeChanged;
}