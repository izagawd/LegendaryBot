using Blazored.SessionStorage;

namespace Website;

public class NavigationService
{
    public const string InitialUrlLocationKey = "InitialUrlLocation";
    private readonly ISyncSessionStorageService _sessionStorage;

    public NavigationService(ISyncSessionStorageService sessionStorageService)
    {
        _sessionStorage = sessionStorageService;
    }


    public string? DesiredRedirectLocation
    {
        get => _sessionStorage.GetItem<string?>(InitialUrlLocationKey);
        set => _sessionStorage.SetItem(InitialUrlLocationKey, value);
    }
}