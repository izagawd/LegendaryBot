using Microsoft.JSInterop;

namespace Website.Services;

public class JSHelperService
{
    private readonly IJSRuntime _jsRuntime;

    public JSHelperService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask AlertAsync(object? toAlert)
    {
        return _jsRuntime.InvokeVoidAsync("alert", toAlert?.ToString() ?? "");
    }
}