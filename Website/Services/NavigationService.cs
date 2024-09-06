using Blazored.SessionStorage;

namespace Website;

public class NavigationService
{
    public const string InitialUrlLocationKey = "InitialUrlLocation";

    
    public string? InitialLocation
    {
        get => _sessionStorage.GetItem<string?>(InitialUrlLocationKey);
        set => _sessionStorage.SetItem(InitialUrlLocationKey, value);
    }
    private ISyncSessionStorageService _sessionStorage;

    public NavigationService(ISyncSessionStorageService sessionStorageService)
    {
        _sessionStorage = sessionStorageService;

    }
}