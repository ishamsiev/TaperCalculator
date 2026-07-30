using System.Globalization;
using System.Text;

namespace Calculator;

/// <summary>
/// A small, self-contained expression evaluator for the calculator. Pure .NET, no CAM/COM
/// dependency. Supports + - * / , unary minus, ^ (power), parentheses, factorial (!), percent (%),
/// the constants pi and e, and the functions sin cos tan asin acos atan sqrt cbrt ln log exp abs.
///
/// Trigonometry honours <see cref="AngleUnit"/> (degrees or radians). Parsing is a hand-written
/// recursive-descent parser: fast enough for a calculator and dependency-free.
/// </summary>
public enum AngleUnit
{
    Degrees,
    Radians
}

public sealed class CalculatorEngine
{
    public AngleUnit AngleUnit { get; set; } = AngleUnit.Degrees;

    private string _text = string.Empty;
    private int _pos;

    /// <summary>
    /// Evaluate an infix expression. Throws <see cref="FormatException"/> on malformed input and
    /// <see cref="DivideByZeroException"/> / <see cref="ArithmeticException"/> on bad maths.
    /// </summary>
    public double Evaluate(string expression)
    {
        _text = expression ?? string.Empty;
        _pos = 0;
        var value = ParseExpression();
        SkipSpaces();
        if (_pos != _text.Length)
            throw new FormatException($"Unexpected character '{_text[_pos]}' at position {_pos + 1}");
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArithmeticException("Result is not a finite number");
        return value;
    }

    // expression := term (('+' | '-') term)*
    private double ParseExpression()
    {
        var value = ParseTerm();
        while (true)
        {
            SkipSpaces();
            var c = Peek();
            if (c == '+') { _pos++; value += ParseTerm(); }
            else if (c == '-') { _pos++; value -= ParseTerm(); }
            else break;
        }
        return value;
    }

    // term := power (('*' | '/' | '%') power)*
    private double ParseTerm()
    {
        var value = ParsePower();
        while (true)
        {
            SkipSpaces();
            var c = Peek();
            if (c == '*') { _pos++; value *= ParsePower(); }
            else if (c == '/')
            {
                _pos++;
                var divisor = ParsePower();
                if (divisor == 0) throw new DivideByZeroException("Division by zero");
                value /= divisor;
            }
            else if (c == '%')
            {
                // Modulo between operands, e.g. 10 % 3. (Standalone "n%" percent is handled in ParseUnary.)
                _pos++;
                var divisor = ParsePower();
                if (divisor == 0) throw new DivideByZeroException("Division by zero");
                value %= divisor;
            }
            else break;
        }
        return value;
    }

    // power := unary ('^' power)?   -- right associative
    private double ParsePower()
    {
        var baseValue = ParseUnary();
        SkipSpaces();
        if (Peek() == '^')
        {
            _pos++;
            var exponent = ParsePower();
            return Math.Pow(baseValue, exponent);
        }
        return baseValue;
    }

    // unary := ('+' | '-') unary | postfix
    private double ParseUnary()
    {
        SkipSpaces();
        var c = Peek();
        if (c == '-') { _pos++; return -ParseUnary(); }
        if (c == '+') { _pos++; return ParseUnary(); }
        return ParsePostfix();
    }

    // postfix := primary ('!' | '%')*
    private double ParsePostfix()
    {
        var value = ParsePrimary();
        while (true)
        {
            SkipSpaces();
            var c = Peek();
            if (c == '!') { _pos++; value = Factorial(value); }
            else if (c == '%') { _pos++; value /= 100.0; }
            else break;
        }
        return value;
    }

    // primary := number | constant | function '(' expression ')' | '(' expression ')'
    private double ParsePrimary()
    {
        SkipSpaces();
        var c = Peek();

        if (c == '(')
        {
            _pos++;
            var value = ParseExpression();
            SkipSpaces();
            Expect(')');
            return value;
        }

        if (char.IsDigit(c) || c == '.')
            return ParseNumber();

        if (char.IsLetter(c))
            return ParseIdentifier();

        throw new FormatException(_pos < _text.Length
            ? $"Unexpected character '{c}' at position {_pos + 1}"
            : "Unexpected end of expression");
    }

    private double ParseNumber()
    {
        var start = _pos;
        while (_pos < _text.Length && (char.IsDigit(_text[_pos]) || _text[_pos] == '.'))
            _pos++;
        // Exponent form: 1.5e-3
        if (_pos < _text.Length && (_text[_pos] == 'e' || _text[_pos] == 'E'))
        {
            var save = _pos;
            _pos++;
            if (_pos < _text.Length && (_text[_pos] == '+' || _text[_pos] == '-')) _pos++;
            if (_pos < _text.Length && char.IsDigit(_text[_pos]))
                while (_pos < _text.Length && char.IsDigit(_text[_pos])) _pos++;
            else
                _pos = save; // not an exponent after all
        }

        var token = _text.Substring(start, _pos - start);
        if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            throw new FormatException($"Invalid number '{token}'");
        return value;
    }

    private double ParseIdentifier()
    {
        var start = _pos;
        while (_pos < _text.Length && char.IsLetter(_text[_pos]))
            _pos++;
        var name = _text.Substring(start, _pos - start).ToLowerInvariant();

        switch (name)
        {
            case "pi": return Math.PI;
            case "e": return Math.E;
        }

        // Function: name must be followed by '('
        SkipSpaces();
        Expect('(');
        var arg = ParseExpression();
        SkipSpaces();
        Expect(')');

        return name switch
        {
            "sin" => Math.Sin(ToRadians(arg)),
            "cos" => Math.Cos(ToRadians(arg)),
            "tan" => Math.Tan(ToRadians(arg)),
            "asin" => FromRadians(Math.Asin(arg)),
            "acos" => FromRadians(Math.Acos(arg)),
            "atan" => FromRadians(Math.Atan(arg)),
            "sqrt" => arg < 0 ? throw new ArithmeticException("sqrt of a negative number") : Math.Sqrt(arg),
            "cbrt" => Math.Cbrt(arg),
            "ln" => arg <= 0 ? throw new ArithmeticException("ln needs a positive argument") : Math.Log(arg),
            "log" => arg <= 0 ? throw new ArithmeticException("log needs a positive argument") : Math.Log10(arg),
            "exp" => Math.Exp(arg),
            "abs" => Math.Abs(arg),
            _ => throw new FormatException($"Unknown function '{name}'")
        };
    }

    private double ToRadians(double value) =>
        AngleUnit == AngleUnit.Degrees ? value * Math.PI / 180.0 : value;

    private double FromRadians(double value) =>
        AngleUnit == AngleUnit.Degrees ? value * 180.0 / Math.PI : value;

    private static double Factorial(double value)
    {
        if (value < 0 || value != Math.Floor(value))
            throw new ArithmeticException("Factorial needs a non-negative integer");
        if (value > 170)
            throw new ArithmeticException("Factorial too large");
        double result = 1;
        for (var i = 2; i <= (int)value; i++)
            result *= i;
        return result;
    }

    private char Peek() => _pos < _text.Length ? _text[_pos] : '\0';

    private void SkipSpaces()
    {
        while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos]))
            _pos++;
    }

    private void Expect(char expected)
    {
        if (Peek() != expected)
            throw new FormatException($"Expected '{expected}' at position {_pos + 1}");
        _pos++;
    }

    /// <summary>Format a result for display: trims trailing zeros, keeps engineering precision.</summary>
    public static string Format(double value)
    {
        if (value == 0) return "0";
        var text = value.ToString("G12", CultureInfo.InvariantCulture);
        // Normalise exponent casing for readability.
        if (text.Contains('E'))
            text = text.Replace("E", "e");
        return text;
    }
}
