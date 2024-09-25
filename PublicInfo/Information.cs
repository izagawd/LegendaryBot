namespace PublicInfo;

public class Information
{
    public const string BattleImagesDirectory = ApiDomainName + "/battle_images";
    public const string BlessingImagesDirectory = BattleImagesDirectory + "/blessings";
    public const string GearsImagesDirectory = BattleImagesDirectory + "/gears";
    public const string ItemsImagesDirectory = BattleImagesDirectory + "/items";
    public const string StatusEffectsImagesDirectory = BattleImagesDirectory + "/status_effects";
    public const string MovesImagesDirectory = BattleImagesDirectory + "/moves";
    public const string CharactersImagesDirectory = BattleImagesDirectory + "/characters";
    public const string DiscordClientId = IsTesting ? "961405563881787392" : "340054610989416460";
    public const string GlobalFontName = "Arial";
    public const bool IsTesting = true;
    public const string ApiDomainName = IsTesting ? "https://localhost:5000" : "https://api.legendarygawds.com";
    public const string WebsiteDomainName = IsTesting ? "https://localhost" : "https://www.legendarygawds.com";
}