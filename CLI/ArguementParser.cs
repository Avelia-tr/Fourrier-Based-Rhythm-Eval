

public static class ArguementParser
{
    // yeah this is slow af idc
    public static arguementOption[] ParseArgs(string[] args)
    {
        var argQueue = new Queue<string>(args);

        var listArgs = new List<arguementOption>();

        while (argQueue.Any())
        {
            ApplyParser(ParseSetup, argQueue, listArgs);
            ApplyParser(ParseMap, argQueue, listArgs);
            ApplyParser(ParseMapSet, argQueue, listArgs);
            ApplyParser(ParseFolder, argQueue, listArgs);
            ApplyParser(ParsePlayer, argQueue, listArgs);
            ApplyParser(ParseTextFile, argQueue, listArgs);
            ApplyParser(ParseOutput, argQueue, listArgs);
        }

        return listArgs.ToArray();
    }

    static void ApplyParser(Func<Queue<string>, arguementOption?> parser, Queue<string> queue, List<arguementOption> result)
    {
        if (!queue.Any()) return;
        var res = parser(queue);
        if (res is not null) result.Add(res);
    }

    static arguementOption? ParseSetup(Queue<string> args)
    {
        if (!FitSetupArgs(args.Peek())) return null;

        args.Dequeue();

        return new SetUpOption(args.Dequeue(), args.Dequeue());
    }

    static arguementOption? ParsePlayer(Queue<string> args)
    {
        if (!FitPlayerArgs(args.Peek())) return null;

        args.Dequeue();

        // maybe do so you can do multiple player id ?
        return new PlayerOption(int.Parse(args.Dequeue()));
    }

    static arguementOption? ParseFolder(Queue<string> args)
    {
        if (!FitFolderArgs(args.Peek())) return null;

        args.Dequeue();

        // maybe verify if the folder exist ? nahhh
        return new FolderOption(args.Dequeue());
    }

    static arguementOption? ParseTextFile(Queue<string> args)
    {
        if (!FitFolderArgs(args.Peek())) return null;

        args.Dequeue();

        // maybe verify if the folder exist ? nahhh
        return new TextFileOption(args.Dequeue());
    }

    static arguementOption? ParseOutput(Queue<string> args)
    {
        if (!FitOutputArgs(args.Peek())) return null;
        args.Dequeue();
        return new OutputOption(args.Dequeue());
    }

    static arguementOption? ParseMap(Queue<string> args)
    {
        if (!FitMapArgs(args.Peek())) return null;
        args.Dequeue();
        return new MapOption(int.Parse(args.Dequeue()));
    }

    static arguementOption? ParseMapSet(Queue<string> args)
    {
        if (!FitMapSetArgs(args.Peek())) return null;
        args.Dequeue();
        return new MapSetOption(int.Parse(args.Dequeue()));
    }

    static bool FitSetupArgs(string arg)
        => arg == "-s";

    static bool FitPlayerArgs(string arg)
        => arg == "-p";

    static bool FitFolderArgs(string arg)
        => arg == "-f";

    static bool FitTextFileArgs(string arg)
        => arg == "-p";

    static bool FitOutputArgs(string arg)
        => arg == "-o";

    static bool FitMapArgs(string arg)
        => arg == "-m";

    static bool FitMapSetArgs(string arg)
        => arg == "-ms";
}

public interface arguementOption;

public record SetUpOption(string ApiId, string ApiSecret) : arguementOption;
public record PlayerOption(int PlayerId) : arguementOption;
public record MapOption(int MapId) : arguementOption;
public record MapSetOption(int MapSetId) : arguementOption;
public record FolderOption(string FolderName) : arguementOption;
public record TextFileOption(string FileName) : arguementOption;
public record OutputOption(string FolderName) : arguementOption;


