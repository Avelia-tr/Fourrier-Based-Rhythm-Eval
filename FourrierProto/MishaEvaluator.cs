using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace FourrierRhythm.Evaluator;

public class MishaEvaluator
{
    IBeatmap _map;
    double _globalMode; // kept for backward compatibility, not used in windowed evaluation
    int WS = 9; // window size

    public MishaEvaluator(IBeatmap pSubject)
    {
        _map = pSubject;
        var hitObjects = _map.HitObjects;

        var deltas = hitObjects
                     .Skip(1)
                     .Select((x, y) => Math.Round(GetDeltas(hitObjects[y], x)))
                     .ToList();

        _globalMode = deltas
                      .Where(d => d != 0)
                      .GroupBy(d => d)
        .Select(g => new
        {
            Value = g.Key,
            Count = g.Count()
        })
        .GroupBy(x => x.Count)
        .OrderByDescending(g => g.Key)
        .First()
        .OrderByDescending(x => x.Value)
        .First()
        .Value;
    } //global mode calculation, doesn't affect everything right now

    public double GetDeltas(HitObject from, HitObject to)
    {
        if (from is Slider fromSlider)
            return to.StartTime - fromSlider.EndTime;
        return to.StartTime - from.StartTime;
    }


    public double Evaluate(int integralSampleRate)
    {
        double[] windowComplexities = EvaluateSlidingWindows(WS);
        double[] windowCorrections = CorrectionSlidingWindows(WS);

        double totalComplexity = windowComplexities.Sum() * windowCorrections.Sum();

        totalComplexity = (totalComplexity / _map.HitObjects.Count) / 10;
        totalComplexity = Math.Pow(totalComplexity, 0.5);
        totalComplexity = Math.Round(totalComplexity * 100) / 100;

        return totalComplexity;
    }

    public double[] EvaluateSlidingWindows(int windowSize)
    {
        var hitObjects = _map.HitObjects;
        if (hitObjects.Count < windowSize)
            return Array.Empty<double>();

        var results = new List<double>();

        for (int start = 0; start <= hitObjects.Count - windowSize; start++)
        {
            var window = hitObjects.Skip(start).Take(windowSize).ToList();
            double mode = ComputeModeForWindow(window);
            double MinDist = ComputeMinDistForWindow(window);
            double complexity = ComputeComplexityForWindow(window, mode, MinDist);
            results.Add(complexity);
        }

        return results.ToArray();
    } // Computes ComplexityEval for each sliding window of consecutive hit objects.

    public double[] CorrectionSlidingWindows(int windowSize)
    {
        var hitObjects = _map.HitObjects;
        if (hitObjects.Count < windowSize)
            return Array.Empty<double>();

        var results = new List<double>();

        for (int start = 0; start <= hitObjects.Count - windowSize; start++)
        {
            var window = hitObjects.Skip(start).Take(windowSize).ToList();
            double mode = ComputeModeForWindow(window);
            double MinDist = ComputeMinDistForWindow(window);
            double correction = ComputeCorrectionForWindow(window, mode, MinDist);
            results.Add(correction);
        }

        return results.ToArray();
    }

    private double ComputeModeForWindow(IList<HitObject> window)
    {
        if (window.Count < 2)
            return 1.0; // fallback, should not happen for windowSize >= 2

        var deltas = new List<double>();
        for (int i = 0; i < window.Count - 1; i++)
        {
            double delta = Math.Round(GetDeltas(window[i], window[i + 1]));
            if (delta != 0)
                deltas.Add(delta);
        }

        if (deltas.Count == 0) return 1.0; // fallback if all deltas are zero

        var mode = deltas
                   .GroupBy(d => d)
        .Select(g => new { Value = g.Key, Count = g.Count() })
        .GroupBy(x => x.Count)
        .OrderByDescending(g => g.Key)
        .First()
        .OrderByDescending(x => x.Value)
        .First()
        .Value;

        return mode;
    } // Computes the mode (most frequent non-zero delta) for a list of hit objects, choosing the biggest mode.

    private double ComputeMinDistForWindow(IList<HitObject> window)
    {
        if (window.Count < 2) return 1.0; // fallback, should not happen for windowSize >= 2

        var deltas = new List<double>();

        for (int i = 0; i < window.Count - 1; i++)
        {
            double delta = Math.Round(GetDeltas(window[i], window[i + 1]));

            if (delta != 0) deltas.Add(delta);
        }

        if (deltas.Count == 0) return 1.0; // fallback if all deltas are zero

        var MinDist = deltas.Min();

        return MinDist;
    } // Computes minimal non-zero time-dist in window

    private double ComputeComplexityForWindow(IList<HitObject> window, double mode, double MinDist)
    {
        int count = window.Count;
        var times = window.Select(h => h.StartTime).ToArray();
        double complexity = 0.0;

        for (int n = 2; n < count - 1; n++)
        {
            for (int m = 1; m < n; m++)
            {
                double ratioToMode = Math.Round(times[n] - times[m]) / mode;
                double ratioToMin = Math.Round(times[n] - times[m]) / MinDist;

                double ratioPrevious = (times[m] - times[m - 1]) / (times[n] - times[m]);
                double ratioNext = (times[n + 1] - times[n]) / (times[n] - times[m]);

                ratioPrevious *= ratioPrevious;
                ratioNext *= ratioNext;

                double isolation = 7.27 * Math.Exp(-ratioPrevious * ratioNext); // WYSI

                double add = BaseEval(ratioToMode, ratioToMin, 2);
                complexity += Math.Pow(add, 0.5) * isolation; //artifical isolation of patterns
            }
        }
        double timeDecayCoef = (times[count - 1] - times[0]) / (WS * mode); // making contribution from big windows smaller
        timeDecayCoef *= timeDecayCoef;
        complexity = complexity * Math.Exp(-timeDecayCoef);

        return complexity;
    } // Computes the ComplexityEval for a single window using its mode.

    private double ComputeCorrectionForWindow(IList<HitObject> window, double mode, double MinDist)
    {
        int count = window.Count;
        var times = window.Select(h => h.StartTime).ToArray();
        double complexity = 0.0;

        for (int n = 2; n < count - 1; n++)
        {
            for (int m = 1; m < n; m++)
            {
                double ratio = Math.Round(times[n] - times[m]) / mode;
                double ratio2 = Math.Round(times[n] - times[m]) / MinDist;

                double add = BaseEval(ratio, ratio2, 2);
                complexity += Math.Pow(add, 1);
            }
        }
        double timeDecayCoef = (times[count - 1] - times[0]) / (WS * mode); // making contribution from big windows smaller
        timeDecayCoef *= timeDecayCoef;
        complexity = complexity * Math.Exp(-timeDecayCoef);

        return complexity;
    }

    public double BaseEval(double x, double y, double a)
        => Math.Abs(Math.Sin(a * MathF.PI * x) / (a * MathF.PI * x));

    /* public double sigmoid(double a, double x)
	{
	    return 1/(1 + Math.Exp(-a*x));
	} // sigmoid function, nothing to say about it

	public double nerf(double a, double x, double z_1, double z_2, double b)
	{
	    double arg = Math.Pow(2*(x-(z_2+z_1)/2)/(z_2-z_1),2.0);
	    arg = Math.Exp(-arg*(1-sigmoid(a, x - z_1)*sigmoid(-a, x - z_2)));
	    return (1 - b *(1- arg));
	} // it's basically a smooth implementation of (1-step function between z_2 > z_1), provided a > 0 */
}
