using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using FourrierRhythm.Math;

using MilliSeconds = double;
using ProbabilityOfHit = double;

namespace FourrierRhythm.Evaluator;

public class FourrierEvaluator
{
    IBeatmap map;

    public FourrierEvaluator(IBeatmap pSubject)
    {
        map = pSubject;
    }

    public ProbabilityOfHit NormalDistribution(MilliSeconds time, MilliSeconds noteTime, MilliSeconds timingWindow)
        => (1 / timingWindow * MathF.Sqrt(MathF.Tau)) * double.Pow(MathF.E, -double.Pow((time - noteTime), 2d) / (2 * timingWindow * timingWindow));

    // extract rhythm of a section

    private ProbabilityOfHit SampleAt(MilliSeconds time)
        => map.HitObjects
        .Select(a => a is Spinner ? 0d : NormalDistribution(time, a.StartTime, a.HitWindows.WindowFor(HitResult.Great)))
        .Sum();

    private IEnumerable<ProbabilityOfHit> SamplePoints(MilliSeconds interval)
    {
        MilliSeconds time = 0;
        while (true)
        {
            Console.WriteLine($"sampleAt {time}");
            yield return SampleAt(time);
            time += interval;
        }
    }

    // apply fourrier transform

    public DiscreteFourrierTransfom DoFourrierTransform(int AmountsOfPoints)
        => new DiscreteFourrierTransfom(SamplePoints(map.GetLastObjectTime() / (MilliSeconds)AmountsOfPoints).Take(AmountsOfPoints).ToArray());

    // get top X rhythm ?

    // win ?



}
