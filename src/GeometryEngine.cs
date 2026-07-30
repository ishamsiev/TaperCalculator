using System.Globalization;

namespace TaperCalculator;

/// <summary>
/// Pure machinist geometry math - no UI, no CAM/COM. Every method takes plain numbers and returns a
/// result record, so the window layer only formats. Angles are in degrees at this boundary.
/// </summary>
public static class GeometryEngine
{
    private const double DegToRad = Math.PI / 180.0;
    private const double RadToDeg = 180.0 / Math.PI;

    // ---- Taper -------------------------------------------------------------

    /// <summary>Solved cone/taper dimensions. All diameters and lengths share the same unit.</summary>
    public readonly record struct TaperResult(
        double BigDiameter,
        double SmallDiameter,
        double Length,
        double HalfAngleDeg,
        double IncludedAngleDeg,
        double TaperPerLength,   // (D - d) / L, i.e. taper on diameter per unit length
        double RatioX);          // taper ratio 1 : X on diameter (double.PositiveInfinity when D == d)

    /// <summary>
    /// Solve a taper from any three of {big diameter, small diameter, length, half angle}; the one
    /// argument passed as null is the unknown. Relation: (D - d) = 2 * L * tan(halfAngle).
    /// </summary>
    public static TaperResult SolveTaper(double? bigD, double? smallD, double? length, double? halfAngleDeg)
    {
        var known = new[] { bigD, smallD, length, halfAngleDeg }.Count(v => v.HasValue);
        if (known < 3)
            throw new InvalidOperationException("Enter any three values; leave exactly one empty.");

        double D, d, L, half;

        if (!halfAngleDeg.HasValue)
        {
            D = Req(bigD, "big diameter"); d = Req(smallD, "small diameter"); L = Req(length, "length");
            if (L <= 0) throw new InvalidOperationException("Length must be greater than 0.");
            half = Math.Atan((D - d) / (2 * L)) * RadToDeg;
        }
        else if (!length.HasValue)
        {
            D = Req(bigD, "big diameter"); d = Req(smallD, "small diameter"); half = halfAngleDeg.Value;
            var t = Math.Tan(half * DegToRad);
            if (Math.Abs(t) < 1e-12) throw new InvalidOperationException("With a 0 angle the length is undefined unless D = d.");
            L = (D - d) / (2 * t);
        }
        else if (!bigD.HasValue)
        {
            d = Req(smallD, "small diameter"); L = Req(length, "length"); half = halfAngleDeg.Value;
            D = d + 2 * L * Math.Tan(half * DegToRad);
        }
        else // small diameter unknown
        {
            D = Req(bigD, "big diameter"); L = Req(length, "length"); half = halfAngleDeg.Value;
            d = D - 2 * L * Math.Tan(half * DegToRad);
        }

        if (L <= 0) throw new InvalidOperationException("Length must be greater than 0.");
        var diff = D - d;
        var ratioX = Math.Abs(diff) < 1e-12 ? double.PositiveInfinity : L / diff;
        return new TaperResult(D, d, L, half, half * 2, diff / L, ratioX);
    }

    // ---- Right triangle ----------------------------------------------------

    /// <summary>Right triangle: legs a, b; hypotenuse c; angle A opposite a, angle B opposite b.</summary>
    public readonly record struct TriangleResult(
        double A, double B, double C, double AngleADeg, double AngleBDeg);

    /// <summary>
    /// Solve a right triangle from any sufficient pair of {leg a, leg b, hypotenuse c, angle A}.
    /// Angle A is the angle opposite leg a; angle B = 90 - A. Pass null for unknowns.
    /// </summary>
    public static TriangleResult SolveRightTriangle(double? a, double? b, double? c, double? angleADeg)
    {
        // Normalise a single known angle down to A. Only one angle input (A) is offered by the UI.
        double? A = angleADeg;

        // Reduce every solvable pairing to concrete a, b, c.
        double ra, rb, rc;

        if (a.HasValue && b.HasValue)
        {
            ra = a.Value; rb = b.Value; rc = Math.Sqrt(ra * ra + rb * rb);
        }
        else if (a.HasValue && c.HasValue)
        {
            ra = a.Value; rc = c.Value;
            if (rc <= ra) throw new InvalidOperationException("Hypotenuse must be longer than a leg.");
            rb = Math.Sqrt(rc * rc - ra * ra);
        }
        else if (b.HasValue && c.HasValue)
        {
            rb = b.Value; rc = c.Value;
            if (rc <= rb) throw new InvalidOperationException("Hypotenuse must be longer than a leg.");
            ra = Math.Sqrt(rc * rc - rb * rb);
        }
        else if (A.HasValue && (a.HasValue || b.HasValue || c.HasValue))
        {
            var ang = A.Value;
            if (ang <= 0 || ang >= 90) throw new InvalidOperationException("Angle A must be between 0 and 90 degrees.");
            var rad = ang * DegToRad;
            if (a.HasValue) { ra = a.Value; rc = ra / Math.Sin(rad); rb = ra / Math.Tan(rad); }
            else if (b.HasValue) { rb = b.Value; rc = rb / Math.Cos(rad); ra = rb * Math.Tan(rad); }
            else { rc = c!.Value; ra = rc * Math.Sin(rad); rb = rc * Math.Cos(rad); }
        }
        else
        {
            throw new InvalidOperationException("Enter two values: two sides, or one side and angle A.");
        }

        if (ra <= 0 || rb <= 0) throw new InvalidOperationException("Sides must be greater than 0.");
        var angA = Math.Atan2(ra, rb) * RadToDeg;
        return new TriangleResult(ra, rb, rc, angA, 90 - angA);
    }

    // ---- ISO metric thread -------------------------------------------------

    /// <summary>ISO metric thread dimensions for nominal diameter D and pitch P (mm).</summary>
    public readonly record struct ThreadResult(
        double Nominal,
        double Pitch,
        double FundamentalHeight, // H = P*sqrt(3)/2
        double PitchDiameter,     // d2 = D - 0.649519*P
        double MinorDiameter,     // d1 = D - 1.082532*P (external minor)
        double ThreadDepth,       // external h3 = 0.613435*P
        double TapDrill);         // approx D - P

    /// <summary>Standard coarse pitch for common nominal metric diameters (mm), for quick fill.</summary>
    public static double? CoarsePitch(double nominal) => nominal switch
    {
        3 => 0.5, 4 => 0.7, 5 => 0.8, 6 => 1.0, 8 => 1.25, 10 => 1.5,
        12 => 1.75, 14 => 2.0, 16 => 2.0, 18 => 2.5, 20 => 2.5, 22 => 2.5,
        24 => 3.0, 27 => 3.0, 30 => 3.5, 36 => 4.0, 42 => 4.5, 48 => 5.0,
        _ => null
    };

    public static ThreadResult SolveMetricThread(double nominal, double pitch)
    {
        if (nominal <= 0) throw new InvalidOperationException("Nominal diameter must be greater than 0.");
        if (pitch <= 0) throw new InvalidOperationException("Pitch must be greater than 0.");

        var h = pitch * Math.Sqrt(3) / 2.0;
        return new ThreadResult(
            nominal,
            pitch,
            h,
            nominal - 0.649519 * pitch,
            nominal - 1.082532 * pitch,
            0.613435 * pitch,
            nominal - pitch);
    }

    // ---- helpers -----------------------------------------------------------

    private static double Req(double? v, string name) =>
        v ?? throw new InvalidOperationException($"Missing value: {name}.");

    /// <summary>Parse a user field; empty/blank -> null. Accepts both '.' and ',' as decimal mark.</summary>
    public static double? ParseOptional(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        text = text.Trim().Replace(',', '.');
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            return v;
        throw new InvalidOperationException($"'{text}' is not a number.");
    }

    /// <summary>Format a number for display: trims trailing zeros, up to 4 decimals.</summary>
    public static string Fmt(double v)
    {
        if (double.IsNaN(v)) return "-";
        if (double.IsInfinity(v)) return v > 0 ? "∞" : "-∞";
        return Math.Round(v, 4).ToString("0.####", CultureInfo.InvariantCulture);
    }
}
