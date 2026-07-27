namespace FusionOS.Modules.Ai.Application.Forecasting;

/// <summary>
/// Closed-form simple linear least-squares fit: y = Slope * x + Intercept.
/// The one piece of "real math" in the Phase 8 forecasting slice — see
/// GetDemandForecastQueryHandler's doc comment for how it's used, and the
/// backend task's hand-verification note for a worked example confirming
/// this formula.
///
/// Standard normal-equations closed form for a simple (one-variable) fit:
///   Slope     = (n * Σxy - Σx * Σy) / (n * Σx² - (Σx)²)
///   Intercept = (Σy - Slope * Σx) / n
/// </summary>
public static class LeastSquaresRegression
{
    public static (decimal Slope, decimal Intercept) Fit(IReadOnlyList<(int X, decimal Y)> points)
    {
        var n = points.Count;
        if (n == 0)
            return (0m, 0m);

        decimal sumX = 0m, sumY = 0m, sumXY = 0m, sumXX = 0m;
        foreach (var (x, y) in points)
        {
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumXX += (decimal)x * x;
        }

        var denominator = n * sumXX - sumX * sumX;
        if (denominator == 0m)
        {
            // All x values identical (or a single point) — no meaningful slope; fall back to a flat line at the mean.
            return (0m, sumY / n);
        }

        var slope = (n * sumXY - sumX * sumY) / denominator;
        var intercept = (sumY - slope * sumX) / n;
        return (slope, intercept);
    }
}
