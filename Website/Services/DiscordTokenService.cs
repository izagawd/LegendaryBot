using Blazored.LocalStorage;

namespace Website.Services;

public class DiscordTokenService
{
    public const string DiscordTokenKey = "discord_access_token";

    private readonly ISyncLocalStorageService _syncLocalStorage;


    public DiscordTokenService(ISyncLocalStorageService syncLocalStorage)
    {
        _syncLocalStorage = syncLocalStorage;
    }


    public string? DiscordToken
    {
        get => _syncLocalStorage.GetItem<string>(DiscordTokenKey);
        set => _syncLocalStorage.SetItem(DiscordTokenKey, value);
    }
}