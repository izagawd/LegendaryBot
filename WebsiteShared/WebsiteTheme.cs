namespace WebsiteShared;



public abstract class WebsiteTheme
{
    public bool IsDarkMode => GetType() == typeof(DarkTheme);

    public abstract string DefaultBackgroundColor { get; }
    
    public abstract string DefaultTextColor { get; }


}


public class DarkTheme : WebsiteTheme
{
    public override string DefaultBackgroundColor => "#1A1A1A";
    public override string DefaultTextColor => "lightgrey";

}

public class LightTheme : WebsiteTheme
{
    public override string DefaultBackgroundColor => "white";
    public override string DefaultTextColor => "black";

}

