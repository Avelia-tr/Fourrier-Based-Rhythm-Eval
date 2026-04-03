using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using FourrierRhythm.FMath;

using MilliSeconds = double;
using ProbabilityOfHit = double;

namespace FourrierRhythm.Evaluator;

[Obsolete("this is the old evaluator here for history")]
public class FourrierEvaluator
{
    IBeatmap map;

    public FourrierEvaluator(IBeatmap pSubject)
    {
        map = pSubject;
    }


    // extract rhythm of a section

    private ProbabilityOfHit SampleAt(MilliSeconds time)
        => map.HitObjects
        .Select(a => a is Spinner ? 0d : MathUtils.NormalDistribution(time, a.StartTime, GetHitWindow(a)))
        .Sum();

    private MilliSeconds GetHitWindow(HitObject pObject)
    {
        if (pObject is Slider lSlider) return lSlider.HeadCircle.HitWindows.WindowFor(HitResult.Great);

        return pObject.HitWindows.WindowFor(HitResult.Great);
    }

    private IEnumerable<ProbabilityOfHit> SamplePoints(MilliSeconds interval)
    {
        MilliSeconds time = 0;

        while (true)
        {
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
