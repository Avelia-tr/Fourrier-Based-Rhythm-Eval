using System;

using osu.Game.Rulesets.Osu;

using ScottPlot;


namespace FourrierRhythm;

public class EntryPoint
{

    public static async Task Main(params string[] args)
    {
        var parsed_args = ArguementParser.ParseArgs(args);

        await FourrierRhythmApp.DoLogic(parsed_args);
    }

}

