using System;

using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Beatmaps;
using System.Threading.Tasks;


public static class FourrierRhythmApp
{

    const int SAMPLE_RATE = 5;

    public static async Task DoLogic(arguementOption[] args)
    {
        var SetUp = args.GetArg<SetUpOption>();
        if (SetUp is not null) OSUAPI.SetUp(SetUp);

        // will do the simpler thing rn
        var outputFile = args.GetArg<OutputOption>();

        var result = new List<MapResult[]>();

        foreach (var arg in args)
        {
            if (arg.ShouldBeSkipped()) continue;
            Console.WriteLine($"Doing {arg}");

            var mapResults = await arg.Process();
            result.Add(mapResults);
        }

        var flatResult = result.SelectMany(x => x).OrderBy(x => x.rhythm).ToArray();

        foreach (var res in flatResult)
        {
            Console.WriteLine($"rating : {res.rhythm} | ID : {res.info.OnlineID} | bpm : {res.info.BPM} | Name : {res.info.Metadata.Title} - {res.info.DifficultyName}");
        }

        if (outputFile is not null) WriteResultToFile(outputFile, flatResult);
    }

    static bool ShouldBeSkipped(this arguementOption arg)
        => arg is SetUpOption or OutputOption;

    static T? GetArg<T>(this arguementOption[] args) where T : arguementOption
        => (T?)args.FirstOrDefault(x => x is T);

    static async Task<MapResult[]> Process(this arguementOption arg)
        => arg switch
        {
            MapOption map => await ProcessOptionMap(map),
            MapSetOption mapSet => await ProcessOptionMapSet(mapSet),
            PlayerOption player => await ProcessPlayerOption(player),
            TextFileOption file => await ProcessTextFileOption(file),
            FolderOption folder => await ProcessFolderOption(folder),
            SetUpOption => new MapResult[0] { },
            OutputOption => new MapResult[0] { },
            _ => throw new NotImplementedException(),
        };

    static async Task<MapResult[]> ProcessOptionMap(MapOption map)
        => new[] { await ProcessMap(map.MapId) };

    static async Task<MapResult[]> ProcessOptionMapSet(MapSetOption mapSet)
    {
        var maps = await OSUAPI.GetBeamapSet(mapSet.MapSetId);

        var results = new List<MapResult>();

        foreach (var map_id in maps)
        {
            await Task.Delay(100);
            var map = await ProcessMap(map_id);
            results.Add(map);
        }

        return results.ToArray();
    }

    static async Task<MapResult[]> ProcessPlayerOption(PlayerOption player)
    {
        var maps = await OSUAPI.GetTopPlaysId(player.PlayerId);

        var results = new List<MapResult>();

        foreach (var map_id in maps)
        {
            await Task.Delay(100);
            var map = await ProcessMap(map_id);
            results.Add(map);
        }

        return results.ToArray();
    }

    static async Task<MapResult[]> ProcessTextFileOption(TextFileOption file)
    {
        var lines = File.ReadLines(file.FileName);

        var results = new List<MapResult>();

        foreach (var id in lines)
        {
            await Task.Delay(100);
            var map = await ProcessMap(int.Parse(id));
            results.Add(map);
        }

        return results.ToArray();
    }

    static async Task<MapResult[]> ProcessLocalOption(FolderOption file)
    {
        var map = OSUAPI.GetBeatmapLocal(file.FolderName);
        return new[] { Process(map) };
    }

    static async Task<MapResult[]> ProcessFolderOption(FolderOption file)
    {
        var lines = Directory.GetFiles(file.FolderName);

        var results = new List<MapResult>();

        foreach (var id in lines)
        {
            var map = OSUAPI.GetBeatmapLocal(id);
            results.Add(Process(map));
        }

        return results.ToArray();
    }

    static async Task<MapResult> ProcessMap(int id)
    {
        var newBeatmap = await OSUAPI.GetBeatmap(id);

        return Process(newBeatmap);
    }

    static MapResult Process(IBeatmap beatmap)
    {
        var evaluator = new FourrierRhythm.Evaluator.MishaEvaluator(beatmap);

        var rhythm_complexity = evaluator.Evaluate(SAMPLE_RATE);

        return new MapResult(rhythm_complexity, beatmap.BeatmapInfo);
    }

    static void WriteResultToFile(OutputOption outputfile, MapResult[] results)
    {
        using FileStream stream = new FileStream(outputfile.FolderName, FileMode.Create, FileAccess.Write);

        foreach (var res in results)
        {
            var data = System.Text.Encoding.Default.GetBytes($"rating : {res.rhythm} |ID : {res.info.OnlineID}| stars : {res.info.StarRating} | bpm : {res.info.BPM} | Name : {res.info.Metadata.Title} - {res.info.DifficultyName}");

            stream.Write(data);
        }

    }
}

public record MapResult(double rhythm, BeatmapInfo info);
