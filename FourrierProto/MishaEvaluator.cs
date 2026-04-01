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

        medianTimeDist = map.HitObjects
            .Skip(1)
            .Select((x, y) => hit_objects[y].StartTime - x.StartTime)
            .Order()
            .ElementAt(hit_objects.Count / 2);
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
                double smear = -double.Exp(-(delta_t / medianTimeDist) * (delta_t / medianTimeDist) / 2);
                double add = (double.Sin(2 * MathF.PI * delta_t / medianTimeDist) / delta_t) * smear;
                add *= add;
                sum += add;
            }
        }

        sum = sum * (medianTimeDist * medianTimeDist / map.HitObjects.Count) * 10;


        return sum;
    }
}
