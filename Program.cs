using System;

using osu.NET.Authorization;
using osu.NET.Models.Scores;
using osu.NET;

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

    private static OsuApiClient api = null!;

    static readonly HttpClient client = new();

    public static async Task Main(params string[] args)
    {
        Console.WriteLine("InitializingApi");

        InitApi(args[0], args[1]);

        Beatmap beatmap = await GetBeatmap(int.Parse(args[2]));

        var converter = new OsuBeatmapConverter(beatmap, new OsuRuleset());

        var newBeatmap = converter.Convert();

        foreach (var hitobject in newBeatmap.HitObjects)
        {
            hitobject.ApplyDefaults(newBeatmap.ControlPointInfo, newBeatmap.Difficulty);
        }

        FourrierEvaluator evaluator = new(newBeatmap!);

        DiscreteFourrierTransfom dft = evaluator.DoFourrierTransform(int.Parse(args[3]));

        var plot = new ScottPlot.Plot();

        plot.Add.Signal(dft.GetMagnetudeOfCos().ToArray());

        plot.SavePng($"{newBeatmap.BeatmapInfo.BeatmapSet.Metadata.Title}_{newBeatmap.BeatmapInfo.DifficultyName}.png", 1920, 1080);

    }


    public static void InitApi(string ClientId, string secret)
    {
        OsuClientAccessTokenProvider provider = new(ClientId, secret);

        OsuApiClientOptions options = new();

        api = new(provider, options, null);
    }

    public static async Task<Beatmap> GetBeatmap(int id)
    {
        using Stream stream = await client.GetStreamAsync($"https://osu.ppy.sh/osu/{id}");
        using LineBufferedReader reader = new LineBufferedReader(stream);

        return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
    }
}

