using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using System.Numerics;

namespace FourrierRhythm.Evaluator;

public class MishaEvaluator
{

    IBeatmap map;
    double medianTimeDist;

    public MishaEvaluator(IBeatmap pSubject)
    {
        map = pSubject;

        var hit_objects = map.HitObjects;

        var mode_estimation = GetModeKDE(map.HitObjects, 40);

        var trueMedian = GetPercentile(map.HitObjects, 0.5);

        medianTimeDist = double.Min(mode_estimation, trueMedian);
    }

    public double Evaluate(int IntegralSampleRate)
    {
        var times = map.HitObjects.Select(h => h.StartTime).ToArray();
        double sum = 0.0;

        for (int n = 1; n < times.Length; n++)           // n > m
        {
            for (int m = 0; m < n; m++)                 // m < n
            {
                double delta_t = times[n] - times[m];
                double timeDecay = double.Exp(-(delta_t / medianTimeDist) * (delta_t / medianTimeDist) / 2);
                double add = (double.Sin(2 * MathF.PI * delta_t / medianTimeDist) / delta_t) * timeDecay;
                add *= add;
                sum += add;
            }
        }

        sum = sum * (medianTimeDist * medianTimeDist / map.HitObjects.Count) * 10;


        return sum;
    }

    public double[] EvaluateAllNotes(double historyLength)
    {
        throw new NotImplementedException();
    }

    public double PerNoteEvaluator(ReadOnlySpan<double> hitObjects, double medianTimeDist)
    {
        double sum = 0.0;

        for (int n = 1; n < hitObjects.Length; n++)           // n > m
        {
            for (int m = 0; m < n; m++)                 // m < n
            {
                double delta_t = hitObjects[n] - hitObjects[m];
                double timeDecay = double.Exp(-(delta_t / medianTimeDist) * (delta_t / medianTimeDist) / 2);
                double add = (double.Sin(2 * MathF.PI * delta_t / medianTimeDist) / delta_t) * timeDecay;
                add *= add;
                sum += add;
            }
        }

        sum = sum * (medianTimeDist * medianTimeDist / hitObjects.Length) * 10;


        return sum;
    }

    private double GetDelta(HitObject from, HitObject to)
    {
        if (from is Slider from_slider)
            return to.StartTime - from_slider.EndTime;
        return to.StartTime - from.StartTime;
    }

    private IEnumerable<double> GetDeltas(IEnumerable<HitObject> hitObjects)
        => hitObjects
            .Skip(1)
            .Select((x, y) => GetDelta(hitObjects.ElementAt(y), x));


    private double GetPercentile(IEnumerable<HitObject> hitObject, double percentile)
        => GetDeltas(hitObject)
            .Order()
            .ElementAt((int)(hitObject.Count() * percentile));

    private double GetModeKDE(IEnumerable<HitObject> hitObject, double strength)
    {
        var deltas = GetDeltas(hitObject).ToArray();
        Func<double, double> kernelling =
            x => deltas.Sum(y => Math.MathUtils.NormalDistribution(x, y, strength)) / hitObject.Count();

        return deltas.MaxBy(kernelling);
    }
}
