using FluentAssertions;
using FusionOS.Modules.Ai.Application.Forecasting;
using Xunit;

namespace FusionOS.Modules.Ai.Tests.Forecasting;

public class LeastSquaresRegressionTests
{
    [Fact]
    public void Fit_PerfectLine_ReturnsExactSlopeAndIntercept()
    {
        // y = 2x + 2 for x = 0..4 -> y = 2, 4, 6, 8, 10 (hand-verified in the Phase 8 build report).
        var points = new (int X, decimal Y)[]
        {
            (0, 2m), (1, 4m), (2, 6m), (3, 8m), (4, 10m),
        };

        var (slope, intercept) = LeastSquaresRegression.Fit(points);

        slope.Should().Be(2m);
        intercept.Should().Be(2m);
    }

    [Fact]
    public void Fit_NoisyLine_ReturnsCloseApproximation()
    {
        // Roughly y = 3x + 1 with small perturbations away from the exact line.
        var points = new (int X, decimal Y)[]
        {
            (0, 1m), (1, 5m), (2, 6m), (3, 11m), (4, 12m),
        };

        // Hand-computed via the closed-form normal equations:
        // n=5, sumX=10, sumY=35, sumXY=100, sumXX=30
        // slope = (5*100 - 10*35) / (5*30 - 10*10) = (500-350)/(150-100) = 150/50 = 3
        // intercept = (35 - 3*10) / 5 = (35-30)/5 = 1
        var (slope, intercept) = LeastSquaresRegression.Fit(points);

        slope.Should().Be(3m);
        intercept.Should().Be(1m);
    }

    [Fact]
    public void Fit_SinglePoint_FallsBackToFlatLineAtThatValue()
    {
        // n=1 makes the denominator (n*sumXX - sumX^2) exactly zero — the code's documented
        // fallback is a flat line (slope 0) at the mean of the y-values, i.e. the single point's
        // own value, rather than throwing a divide-by-zero.
        var points = new (int X, decimal Y)[] { (5, 42m) };

        var (slope, intercept) = LeastSquaresRegression.Fit(points);

        slope.Should().Be(0m);
        intercept.Should().Be(42m);
    }

    [Fact]
    public void Fit_AllIdenticalXValues_FallsBackToFlatLineAtMean()
    {
        // Every point shares the same x, so the denominator is zero even though there are
        // multiple points. The code explicitly guards this (LeastSquaresRegression.cs line 32)
        // and falls back to a flat line at the mean of the y-values rather than throwing.
        var points = new (int X, decimal Y)[]
        {
            (7, 10m), (7, 20m), (7, 30m),
        };

        var (slope, intercept) = LeastSquaresRegression.Fit(points);

        slope.Should().Be(0m);
        intercept.Should().Be(20m); // mean of 10, 20, 30
    }

    [Fact]
    public void Fit_EmptyList_ReturnsZeroes()
    {
        var (slope, intercept) = LeastSquaresRegression.Fit(Array.Empty<(int X, decimal Y)>());

        slope.Should().Be(0m);
        intercept.Should().Be(0m);
    }

    [Fact]
    public void Fit_NegativeSlope_IsComputedCorrectly()
    {
        // y = -1x + 10 exactly, to confirm the formula isn't accidentally biased toward
        // non-negative slopes (demand can decline over time, and the regression itself
        // must reflect that even though the caller later clamps projected quantities at zero).
        var points = new (int X, decimal Y)[]
        {
            (0, 10m), (1, 9m), (2, 8m), (3, 7m),
        };

        var (slope, intercept) = LeastSquaresRegression.Fit(points);

        slope.Should().Be(-1m);
        intercept.Should().Be(10m);
    }
}
