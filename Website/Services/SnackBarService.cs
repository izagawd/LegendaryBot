using Website.Components;

namespace Website.Services;

public class SnackBarService
{
    public Task DisplayPendingSnackBarIfExistsAsync()
    {
        if (_pendingSnackBarParam is not null)
        {
            var toDo = DisplaySnackBarAsync(_pendingSnackBarParam.Value);
            _pendingSnackBarParam = null;
            return toDo;
        }
      
        return Task.CompletedTask;

    }

    private SnackBar.SnackBarParams? _pendingSnackBarParam;
    private SnackBar? _snackBar;

    public Task DisplaySnackBarAsync(SnackBar.SnackBarParams snackBarParams)
    {
        if (_snackBar is null)
        {
            _pendingSnackBarParam = snackBarParams;
            return Task.CompletedTask;
        }
        return _snackBar.DisplaySnackBarAsync(snackBarParams);
    }
    
    public void SetSnackBar(SnackBar value)
    {
        if (value is not null)
            _snackBar = value;
    }

}