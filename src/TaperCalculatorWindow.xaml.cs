using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TaperCalculator;

/// <summary>
/// Taper Calculator window - a shop-floor geometry helper. Self-contained: it does the math with
/// <see cref="GeometryEngine"/> and touches no CAM/COM objects, so it is safe to run on its own
/// STA thread and to outlive the <see cref="TaperCalculatorExtension.Run"/> call.
/// </summary>
public partial class TaperCalculatorWindow : Window, IDisposable
{
    public TaperCalculatorWindow()
    {
        InitializeComponent();
        // Windows draws the title bar light by default; ask DWM to make it dark so it matches the
        // ENCY-styled body. Must run once the HWND exists.
        SourceInitialized += (_, _) => UseDarkTitleBar();
    }

    // --- dark title bar (DWM) ----------------------------------------------

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20; // Windows 10 20H1+ / 11

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    private void UseDarkTitleBar()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return;
        int on = 1;
        // Harmless on OS builds that don't support it (returns a non-zero HRESULT, ignored).
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref on, sizeof(int));
    }

    /// <summary>
    /// Required by <c>WindowHelper.ShowStaWindow&lt;TWindow&gt;</c> (its <c>TWindow</c> is
    /// constrained to <see cref="IDisposable"/>). Nothing unmanaged is held, so this is empty.
    /// </summary>
    public void Dispose()
    {
    }

    private void SolveTaper_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var r = GeometryEngine.SolveTaper(
                GeometryEngine.ParseOptional(TpBigD.Text),
                GeometryEngine.ParseOptional(TpSmallD.Text),
                GeometryEngine.ParseOptional(TpLength.Text),
                GeometryEngine.ParseOptional(TpHalfAngle.Text));

            TpOutBigD.Text = GeometryEngine.Fmt(r.BigDiameter);
            TpOutSmallD.Text = GeometryEngine.Fmt(r.SmallDiameter);
            TpOutLength.Text = GeometryEngine.Fmt(r.Length);
            TpOutHalf.Text = GeometryEngine.Fmt(r.HalfAngleDeg);
            TpOutIncluded.Text = GeometryEngine.Fmt(r.IncludedAngleDeg);
            TpOutRatio.Text = GeometryEngine.Fmt(r.RatioX);
            ClearStatus();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void SolveTriangle_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var r = GeometryEngine.SolveRightTriangle(
                GeometryEngine.ParseOptional(TrA.Text),
                GeometryEngine.ParseOptional(TrB.Text),
                GeometryEngine.ParseOptional(TrC.Text),
                GeometryEngine.ParseOptional(TrAngle.Text));

            TrOutA.Text = GeometryEngine.Fmt(r.A);
            TrOutB.Text = GeometryEngine.Fmt(r.B);
            TrOutC.Text = GeometryEngine.Fmt(r.C);
            TrOutAngA.Text = GeometryEngine.Fmt(r.AngleADeg);
            TrOutAngB.Text = GeometryEngine.Fmt(r.AngleBDeg);
            ClearStatus();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void SolveThread_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var nominal = GeometryEngine.ParseOptional(ThNominal.Text)
                ?? throw new InvalidOperationException("Enter the nominal diameter.");
            var pitch = GeometryEngine.ParseOptional(ThPitch.Text)
                ?? GeometryEngine.CoarsePitch(nominal)
                ?? throw new InvalidOperationException("No standard coarse pitch for this diameter - enter the pitch.");

            var r = GeometryEngine.SolveMetricThread(nominal, pitch);
            ThPitch.Text = GeometryEngine.Fmt(r.Pitch); // reflect the coarse pitch that was filled in
            ThOutPitch.Text = GeometryEngine.Fmt(r.Pitch);
            ThOutH.Text = GeometryEngine.Fmt(r.FundamentalHeight);
            ThOutD2.Text = GeometryEngine.Fmt(r.PitchDiameter);
            ThOutD1.Text = GeometryEngine.Fmt(r.MinorDiameter);
            ThOutDepth.Text = GeometryEngine.Fmt(r.ThreadDepth);
            ThOutDrill.Text = GeometryEngine.Fmt(r.TapDrill);
            ClearStatus();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ClearTaper_Click(object sender, RoutedEventArgs e)
    {
        TpBigD.Text = TpSmallD.Text = TpLength.Text = TpHalfAngle.Text = string.Empty;
        TpOutBigD.Text = TpOutSmallD.Text = TpOutLength.Text =
            TpOutHalf.Text = TpOutIncluded.Text = TpOutRatio.Text = "-";
        ClearStatus();
        TpBigD.Focus();
    }

    private void ClearTriangle_Click(object sender, RoutedEventArgs e)
    {
        TrA.Text = TrB.Text = TrC.Text = TrAngle.Text = string.Empty;
        TrOutA.Text = TrOutB.Text = TrOutC.Text = TrOutAngA.Text = TrOutAngB.Text = "-";
        ClearStatus();
        TrA.Focus();
    }

    private void ClearThread_Click(object sender, RoutedEventArgs e)
    {
        ThNominal.Text = ThPitch.Text = string.Empty;
        ThOutPitch.Text = ThOutH.Text = ThOutD2.Text =
            ThOutD1.Text = ThOutDepth.Text = ThOutDrill.Text = "-";
        ClearStatus();
        ThNominal.Focus();
    }

    private void ShowError(string message) => StatusText.Text = message;
    private void ClearStatus() => StatusText.Text = string.Empty;
}
