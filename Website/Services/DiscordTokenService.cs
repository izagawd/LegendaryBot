using Blazored.LocalStorage;

namespace Website.Services;

public class DiscordTokenService
{
    
    public const string DiscordTokenKey = "discord_access_token";
    
    private ISyncLocalStorageService _syncLocalStorage;


    public string? DiscordToken
    {
        get => _syncLocalStorage.GetItem<string>(DiscordTokenKey);
        set => _syncLocalStorage.SetItem(DiscordTokenKey, value);
    }


    public DiscordTokenService(ISyncLocalStorageService syncLocalStorage)
    {
        _syncLocalStorage = syncLocalStorage;
    }
    

}