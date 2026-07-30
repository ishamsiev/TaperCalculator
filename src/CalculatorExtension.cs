using CAMAPI.Application;
using CAMAPI.DotnetHelper;
using CAMAPI.Extensions;
using CAMAPI.ResultStatus;

namespace Calculator;

/// <summary>
/// Utility entry point: opens the engineering calculator window from the ENCY utilities menu.
///
/// The window is a WPF window shown on its own STA thread (the ENCY bridge runs MTA, WPF needs STA)
/// and it is non-modal - the user keeps working in ENCY while it stays open. Because the window
/// outlives the <see cref="Run"/> call, the extension is <see cref="IExtensionLazyUnloadable"/>:
/// it reports <see cref="CanUnload"/> == false until the window is closed, so ENCY does not unload
/// the assembly while the window is still alive.
/// </summary>
public class CalculatorExtension : IExtension, IExtensionUtility, IExtensionLazyUnloadable
{
    private bool _canUnload = true;

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
            // Owner handle so the calculator sits on top of the ENCY main window. The calculator
            // itself needs no CAM data, so no COM object is captured by the window.
            long ownerHandle = 0;
            try
            {
                using var applicationCom = ComWrapper.Create(context.CamApplication);
                using var mainFormCom = applicationCom.MainForm();
                ownerHandle = mainFormCom.MainWindowHandle();
            }
            catch
            {
                // No main-window handle available (e.g. headless host) - open ownerless.
                ownerHandle = 0;
            }

            // The window is alive from here until the user closes it: block unloading until then.
            _canUnload = false;
            WindowHelper.ShowStaWindow(
                ownerHandle,
                () => new CalculatorWindow(),
                () => _canUnload = true);
        }
        catch (Exception e)
        {
            _canUnload = true;
            resultStatus.Code = TResultStatusCode.rsError;
            resultStatus.Description = e.Message;
        }
    }

    /// <summary>Allow unloading only when the calculator window is closed.</summary>
    public bool CanUnload
    {
        get => _canUnload;
        set { }
    }
}
