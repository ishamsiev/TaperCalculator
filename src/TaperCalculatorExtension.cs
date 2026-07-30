using CAMAPI.Application;
using CAMAPI.DotnetHelper;
using CAMAPI.Extensions;
using CAMAPI.ResultStatus;

namespace TaperCalculator;

/// <summary>
/// Utility entry point: opens the Taper Calculator window from the ENCY utilities menu.
///
/// The window is a WPF window shown on its own STA thread (the ENCY bridge runs MTA, WPF needs STA)
/// and it is non-modal - the user keeps working in ENCY while it stays open. Because the window
/// outlives the <see cref="Run"/> call, the extension is <see cref="IExtensionLazyUnloadable"/>:
/// the <see cref="ExtensionWindowLazyUnloadable"/> helper owns the owner handle, the STA thread and
/// the <see cref="CanUnload"/> bookkeeping, so ENCY does not unload the assembly while the window is
/// still alive.
/// </summary>
public class TaperCalculatorExtension : IExtension, IExtensionUtility, IExtensionLazyUnloadable
{
    private readonly ExtensionWindowLazyUnloadable _windowManager = new();

    /// <inheritdoc />
    public IExtensionInfo? Info { get; set; }

    /// <summary>
    /// Called when the user runs the utility from ENCY.
    /// </summary>
    /// <param name="context">Information about the current ENCY instance.</param>
    /// <param name="resultStatus">Error reporting (exceptions do not cross the host boundary).</param>
    public void Run(IExtensionUtilityContext context, out TResultStatus resultStatus)
    {
        resultStatus = default;
        try
        {
            // Parent the window to the ENCY main window so it stacks above it. The calculator
            // needs no CAM data, so nothing else is read from the application.
            using (var applicationCom = ComWrapper.Create(context.CamApplication))
            {
                _windowManager.SetOwnerHandle(applicationCom);
            }

            // Non-modal: the helper runs the window on its own STA thread and keeps CanUnload false
            // until it closes. A failure on the STA thread cannot be surfaced through resultStatus
            // (Run has already returned by then), so it is swallowed rather than crossing threads.
            _windowManager.ShowWindow(
                () => new TaperCalculatorWindow(),
                onClosed: () => { },
                onException: ex => { });
        }
        catch (Exception e)
        {
            resultStatus.Code = TResultStatusCode.rsError;
            resultStatus.Description = e.Message;
        }
    }

    /// <summary>Allow unloading only when the window is closed.</summary>
    public bool CanUnload
    {
        get => _windowManager.CanUnload;
        set { }
    }
}
