using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using System.Numerics;

namespace FourrierRhythm.Evaluator;

public class MishaEvaluator
{

    IBeatmap map;
    double bpm;

    public MishaEvaluator(IBeatmap pSubject)
    {
        map = pSubject;

        var hit_objects = map.HitObjects;

        bpm = map.HitObjects
            .Skip(1)
            .Select((x, y) => hit_objects[y - 1].StartTime - x.StartTime)
            .Order()
            .ElementAt(hit_objects.Count / 2);
    }

    public Complex FourrierRegularMap(double x)
        => 1 - Complex.Exp(Complex.ImaginaryOne * x * bpm * map.HitObjects.Count)
        / 1 - Complex.Exp(Complex.ImaginaryOne * x * bpm);

    public Complex FourrierMap(double x)
    {
        var first_object = map.HitObjects.First().StartTime;

        return map.HitObjects.Select(hit_object => Complex.Exp(Complex.ImaginaryOne * x * (hit_object.StartTime - first_object)))
            .Aggregate((a, b) => a + b);
    }

    public double Evaluate(int IntegralSampleRate)
    {
        var true_sample_rate = IntegralSampleRate * map.HitObjects.Count;
        var step = 2 * MathF.PI / (true_sample_rate);

        var points = Enumerable.Range(0, true_sample_rate)
            .Select(x => x * step)
            .Select(x => (FourrierMap(x / bpm) - FourrierRegularMap(x / bpm)).Magnitude)
            .Select(x => x * x)
            .ToArray();

        var results = 0d;

        for (int i = 1; i < points.Length; i++)
        {
            results += (points[i - 1] + points[i]) / 2d * step;
        }

        return results;

    }
}
