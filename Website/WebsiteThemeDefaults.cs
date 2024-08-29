using Blazored.LocalStorage;

namespace Website;



public static class  ThemeExtensions
{
    public static WebsiteThemeDefaults GetThemeDefaults(this WebsiteTheme websiteTheme) =>
        websiteTheme == WebsiteTheme.Dark ? WebsiteThemeDefaults.Dark : WebsiteThemeDefaults.Light;

}
public enum WebsiteTheme : byte
{
    Dark, Light
}

public class WebsiteThemeService
{
    private const string WebsiteThemeKey = "website_theme";
    private ISyncLocalStorageService _syncLocalStorage;

    private WebsiteTheme? _cachedWebsiteTheme = null;

    public WebsiteTheme WebsiteTheme
    {
        get => _cachedWebsiteTheme ??= _syncLocalStorage.GetItem<WebsiteTheme>(WebsiteThemeKey);
        set
        {
            _cachedWebsiteTheme = value;
            _syncLocalStorage.SetItem(WebsiteThemeKey,value);
        }
    }

    public WebsiteThemeService(ISyncLocalStorageService syncLocalStorageService)
    {
        _syncLocalStorage = syncLocalStorageService;
    }
}
public abstract class WebsiteThemeDefaults
{
  
    public bool IsDarkMode => GetType() == typeof(DarkThemeDefaults);
    public abstract string DefaultBackgroundColor { get; }
    


    public static readonly WebsiteThemeDefaults Dark  = new DarkThemeDefaults();
    public static readonly WebsiteThemeDefaults Light  = new LightThemeDefaults();
        public static WebsiteThemeDefaults FromThemeType(bool isDarkMode)
    {
        return isDarkMode ? Dark : Light;
    }
    protected WebsiteThemeDefaults(){}
}



public class DarkThemeDefaults : WebsiteThemeDefaults
{
    public override string DefaultBackgroundColor => "#1A1A1A";
   
}
public class LightThemeDefaults : WebsiteThemeDefaults
{
    public override string DefaultBackgroundColor => "white";


}

