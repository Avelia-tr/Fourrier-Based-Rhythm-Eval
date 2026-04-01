using System;

using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps;
using osu.Game.IO;

using ScottPlot;

using FourrierRhythm.Evaluator;
using FourrierRhythm.Math;

namespace FourrierRhythm;

public class EntryPoint
{

    static readonly HttpClient client = new();

    public static async Task Main(params string[] args)
    {

        Beatmap beatmap = await GetBeatmap(int.Parse(args[0]));

        var converter = new OsuBeatmapConverter(beatmap, new OsuRuleset());

        var newBeatmap = converter.Convert();

        foreach (var hitobject in newBeatmap.HitObjects)
        {
            hitobject.ApplyDefaults(newBeatmap.ControlPointInfo, newBeatmap.Difficulty);
        }

        var evaluator = new MishaEvaluator(newBeatmap);
        var sampleRate = int.Parse(args[1]);

        var rhythm_complexity = evaluator.Evaluate(sampleRate);

        Console.WriteLine($"result is {rhythm_complexity}");

    }

    public static async Task<Beatmap> GetBeatmap(int id)
    {
        using Stream stream = await client.GetStreamAsync($"https://osu.ppy.sh/osu/{id}");
        using LineBufferedReader reader = new LineBufferedReader(stream);

        return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
    }
}

