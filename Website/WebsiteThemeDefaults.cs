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

