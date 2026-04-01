using System;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.Beatmaps.Formats;

public static class OSUAPI
{
    static readonly HttpClient client = new();

    static string API_SECRET = "";
    static string API_ID = "";

    static bool IsSetup = false;

    const string API_FILES = "api_key";

    public static void Setup()
    {
        if (IsSetup) return;

        var file = File.ReadLines(API_FILES);

        API_ID = file.First();
        API_SECRET = file.Skip(1).First();

        IsSetup = true;
    }

    public static void SetUp(SetUpOption options)
    {
        API_SECRET = options.ApiSecret;
        API_ID = options.ApiId;

        File.WriteAllText(API_FILES, $"{API_ID}\n{API_SECRET}");
        IsSetup = true;
    }

    public static async Task<Beatmap> GetBeatmap(int id)
    {
        using Stream stream = await client.GetStreamAsync($"https://osu.ppy.sh/osu/{id}");
        using LineBufferedReader reader = new LineBufferedReader(stream);

        return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
    }
}
