using System;

using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Beatmaps;

using ScottPlot;

using FourrierRhythm.Evaluator;
using FourrierRhythm.Math;

namespace FourrierRhythm;

public class EntryPoint
{

    static readonly HttpClient client = new();

    public static async Task Main(params string[] args)
    {

        var newBeatmap = await OSUAPI.GetBeatmap(int.Parse(args[0]));

        var evaluator = new MishaEvaluator(newBeatmap);
        var sampleRate = int.Parse(args[1]);

        var rhythm_complexity = evaluator.Evaluate(sampleRate);

        Console.WriteLine($"result is {rhythm_complexity}");
    }

}

