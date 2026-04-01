
public static class App
{
    static string? outputFile;

    static async Task DoLogic(arguementOption[] args)
    {
        var SetUp = args.GetArg<SetUpOption>();
        if (SetUp is not null) OSUAPI.SetUp(SetUp);

        // will do the simpler thing rn
        outputFile = args.GetArg<OutputOption>()?.FolderName;


    }

    static T? GetArg<T>(this arguementOption[] args) where T : arguementOption
        => (T)args.First(x => x is T);


}
