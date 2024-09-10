using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace Website.Services;
public class WebsiteThemeService
{
    public event Action OnWebsiteThemeChanged;
    private const string WebsiteThemeKey = "website_theme";
    private ISyncLocalStorageService _syncLocalStorage;

    private WebsiteTheme _cachedWebsiteTheme;

    public WebsiteTheme WebsiteTheme
    {
        get => _cachedWebsiteTheme;
        set
        {
            _cachedWebsiteTheme = value;
            _syncLocalStorage.SetItem(WebsiteThemeKey,value);
            OnWebsiteThemeChanged.Invoke();
        }
    }

    public WebsiteThemeService(ISyncLocalStorageService syncLocalStorageService)
    {
        _syncLocalStorage = syncLocalStorageService;
        _cachedWebsiteTheme = _syncLocalStorage.GetItem<WebsiteTheme>(WebsiteThemeKey);
    }
}