using osu.NET;
using osu.NET.Authorization;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.Beatmaps.Formats;

public static class OSUAPI
{
    static readonly HttpClient client = new();

    static OsuApiClient clientOSU = null!;

    static bool IsSetup = false;

    const string API_FILES = "api_key";

    public static void Setup()
    {
        if (IsSetup) return;

        var file = File.ReadLines(API_FILES);

        OsuClientAccessTokenProvider provider = new(file.First(), file.Skip(1).First());

        clientOSU = new(provider, null);
    }

    public static void SetUp(SetUpOption options)
    {
        var API_SECRET = options.ApiSecret;
        var API_ID = options.ApiId;

        File.WriteAllText(API_FILES, $"{API_ID}\n{API_SECRET}");
        Console.WriteLine("api_key file created");

        OsuClientAccessTokenProvider provider = new(API_ID, API_SECRET);

        clientOSU = new(provider, null);
    }

    public static async Task<Beatmap> GetBeatmapRaw(int id)
    {
        using Stream stream = await client.GetStreamAsync($"https://osu.ppy.sh/osu/{id}");
        using LineBufferedReader reader = new LineBufferedReader(stream);

        return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
    }

    public static IBeatmap GetBeatmapLocal(string filePath)
    {
        using Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using LineBufferedReader reader = new LineBufferedReader(stream);

        var beatmap = Decoder.GetDecoder<Beatmap>(reader).Decode(reader);

        var converter = new OsuBeatmapConverter(beatmap, new OsuRuleset());

        var newBeatmap = converter.Convert();

        foreach (var hitobject in newBeatmap.HitObjects)
        {
            hitobject.ApplyDefaults(newBeatmap.ControlPointInfo, newBeatmap.Difficulty);
        }

        return newBeatmap;

    }

    public static async Task<IBeatmap> GetBeatmap(int id)
    {
        Beatmap beatmap = await OSUAPI.GetBeatmapRaw(id);

        var converter = new OsuBeatmapConverter(beatmap, new OsuRuleset());

        var newBeatmap = converter.Convert();

        foreach (var hitobject in newBeatmap.HitObjects)
        {
            hitobject.ApplyDefaults(newBeatmap.ControlPointInfo, newBeatmap.Difficulty);
        }

        return newBeatmap;
    }

    public static async Task<IEnumerable<int>> GetBeamapSet(int id)
    {
        var ids_result = (await clientOSU.GetBeatmapSetAsync(id)).Value!;

        return ids_result.Beatmaps?.Select(x => x.Id) ?? Enumerable.Empty<int>();
    }

    public static async Task<IEnumerable<int>> GetTopPlaysId(int Player_id)
    {
        var ids_result = (await clientOSU
                .GetUserScoresAsync(
                    Player_id,
                    osu.NET.Enums.UserScoreType.Best,
                    false,
                    false
                    , osu.NET.Enums.Ruleset.Osu)
                ).Value!;

        return ids_result?.Select(x => x.BeatmapId) ?? Enumerable.Empty<int>();
    }
}
