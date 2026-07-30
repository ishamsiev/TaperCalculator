using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calculator;

/// <summary>
/// The engineering-calculator window. Self-contained: it evaluates expressions with
/// <see cref="CalculatorEngine"/> and touches no CAM/COM objects, so it is safe to run on its own
/// STA thread and to outlive the <see cref="CalculatorExtension.Run"/> call.
/// </summary>
public partial class CalculatorWindow : Window, IDisposable
{
    private readonly CalculatorEngine _engine = new();
    private bool _resultShown;

    public CalculatorWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => InputText.Focus();
    }

    /// <summary>
    /// Required by <c>WindowHelper.ShowStaWindow&lt;TWindow&gt;</c> (its <c>TWindow</c> is
    /// constrained to <see cref="IDisposable"/>). The calculator holds no unmanaged or COM
    /// resources, so there is nothing to release here.
    /// </summary>
    public void Dispose()
    {
    }

    // A key that inserts literal text (digits, operators, constants, brackets).
    private void Insert_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string text })
            Insert(text);
    }

    // A function key: inserts "name(" and, if a result is on screen, wraps it.
    private void Func_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string func })
            return;

        if (_resultShown)
        {
            // Wrap the just-computed value: 9 -> sqrt(9
            InputText.Text = func + InputText.Text;
            InputText.CaretIndex = InputText.Text.Length;
            _resultShown = false;
            ExpressionText.Text = string.Empty;
        }
        else
        {
            Insert(func);
        }
    }

    private void Insert(string text)
    {
        if (_resultShown)
        {
            // Starting a fresh calculation after a result: operators continue it, anything else replaces.
            if (text.Length > 0 && "+-*/^%!".IndexOf(text[0]) < 0)
            {
                InputText.Clear();
            }
            _resultShown = false;
            ExpressionText.Text = string.Empty;
        }

        var caret = InputText.CaretIndex;
        InputText.Text = InputText.Text.Insert(caret, text);
        InputText.CaretIndex = caret + text.Length;
        InputText.Focus();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        InputText.Clear();
        ExpressionText.Text = string.Empty;
        _resultShown = false;
        InputText.Focus();
    }

    private void Backspace_Click(object sender, RoutedEventArgs e)
    {
        if (_resultShown)
        {
            Clear_Click(sender, e);
            return;
        }
        var caret = InputText.CaretIndex;
        if (caret > 0)
        {
            InputText.Text = InputText.Text.Remove(caret - 1, 1);
            InputText.CaretIndex = caret - 1;
        }
        InputText.Focus();
    }

    private void Equals_Click(object sender, RoutedEventArgs e) => Evaluate();

    private void InputText_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Evaluate();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Clear_Click(sender, e);
            e.Handled = true;
        }
    }

    private void Evaluate()
    {
        var expression = InputText.Text.Trim();
        if (string.IsNullOrEmpty(expression))
            return;

        try
        {
            var result = _engine.Evaluate(expression);
            ExpressionText.Text = expression + " =";
            InputText.Text = CalculatorEngine.Format(result);
            InputText.CaretIndex = InputText.Text.Length;
            _resultShown = true;
        }
        catch (Exception ex)
        {
            ExpressionText.Text = expression;
            InputText.Text = "Error: " + ex.Message;
            InputText.SelectAll();
            _resultShown = true;
        }
        InputText.Focus();
    }

    private void Deg_Click(object sender, RoutedEventArgs e)
    {
        _engine.AngleUnit = AngleUnit.Degrees;
        DegButton.IsChecked = true;
        RadButton.IsChecked = false;
    }

    private void Rad_Click(object sender, RoutedEventArgs e)
    {
        _engine.AngleUnit = AngleUnit.Radians;
        RadButton.IsChecked = true;
        DegButton.IsChecked = false;
    }
}
