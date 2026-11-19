using System;
using Microsoft.AspNetCore.Components;

namespace BlazorAdmin.Helpers;

public class BlazorLayoutComponent : LayoutComponentBase, IDisposable
{
    private readonly RefreshBroadcast _refresh = RefreshBroadcast.Instance;

    protected override void OnInitialized()
    {
        _refresh.RefreshRequested += DoRefresh;
        base.OnInitialized();
    }

    public void CallRequestRefresh()
    {
        _refresh.CallRequestRefresh();
    }

    private void DoRefresh()
    {
        StateHasChanged();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // RefreshBroadcast is a process-wide singleton; without this the component
            // (and everything it references) stays alive and keeps re-rendering.
            _refresh.RefreshRequested -= DoRefresh;
        }
    }
}
