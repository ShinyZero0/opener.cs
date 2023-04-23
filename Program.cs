using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using GlobExpressions;

internal static class Program
{
    private static int Main(string[] args)
    {
        string configFile = Path.Combine(
            Environment.GetEnvironmentVariable("HOME")!,
            ".config/associations.json"
        );
        string raw;
        try
        {
            raw = File.ReadAllText(configFile);
            Config = JsonSerializer.Deserialize<ConfigFile>(
                raw,
                SourceGenerationContext.Default.ConfigFile
            );
        }
        catch (Exception exc)
        {
            Console.WriteLine($"Problem with your config:\n{exc}");
            return 1;
        }

        foreach (Handler handler in Config.Handlers.Reverse())
        {
            if (handler.RegexPatterns != null)
                foreach (string regex in handler.RegexPatterns)
                {
                    RegexHandlers[new RegexPattern(regex)] = handler;
                }
            if (handler.GlobPatterns != null)
                foreach (string glob in handler.GlobPatterns)
                {
                    RegexHandlers[new GlobPattern(glob)] = handler;
                }
        }
        if (args[0] == "-q")
		{
			PreviewFiles(args.Skip(1));
			return 0;
		}
        OpenFiles(args);
        return 0;
    }

    private static Dictionary<RegexPattern, Handler> RegexHandlers = new();
    private static ConfigFile Config;

    private static void PreviewFiles(IEnumerable<string> fileNames)
    {
        var handlerTuple = _getHandler(fileNames);
        if (handlerTuple == null)
            return;
        Console.WriteLine(handlerTuple.Item2.Exec);
    }

    private static Tuple<RegexPattern, Handler>? _getHandler(IEnumerable<string> fileNames)
    {
        Tuple<RegexPattern, Handler>? handlerTuple = null;
        foreach (KeyValuePair<RegexPattern, Handler> entry in RegexHandlers)
        {
            if (entry.Key.IsMatch(fileNames.ToList()[0]))
            {
                handlerTuple = new(entry.Key, entry.Value);
                break;
            }
        }
        if (handlerTuple is null)
        {
            Console.WriteLine("handler not defined");
            return null;
        }
        foreach (string fileName in fileNames)
        {
            if (!handlerTuple.Item1.IsMatch(fileName))
            {
                Console.WriteLine("These files have different handlers!");
                return null;
            }
        }
        return handlerTuple;
    }

    private static void OpenFiles(string[] fileNames)
    {
        var handlerTuple = _getHandler(fileNames);
        if (handlerTuple == null)
            return;
        Dictionary<string, string?> handlerExec = new();

        handlerExec["termPrefix"] =
            Console.IsInputRedirected && handlerTuple.Item2.Term ? Config.Term : null;

        handlerExec["exec"] = handlerTuple.Item2.Exec;
        handlerExec["files"] = String.Join(' ', fileNames.Select(n => $"'{n}'"));

        handlerExec["bgPostfix"] = !handlerTuple.Item2.Term ? "&" : null;

        Process proc;
        proc = new()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "/bin/sh",
                Arguments = String.Concat(
                    "-c \"",
                    string.Join(' ', handlerExec.Where(p => p.Value != null).Select(p => p.Value)),
                    "\""
                ),
                CreateNoWindow = !Console.IsInputRedirected
            }
        };
        proc.Start();
        proc.WaitForExit();
    }
}

public class Handler
{
    [JsonInclude]
    public string[]? RegexPatterns;

    [JsonInclude]
    public string[]? GlobPatterns;

    [JsonInclude]
    public string Exec;

    [JsonInclude]
    public bool Term = false;

    public Handler() { }
}

public class ConfigFile
{
    [JsonInclude]
    public string Term;

    [JsonInclude]
    public Handler[] Handlers;
}

public class RegexPattern
{
    public RegexPattern(string pattern)
    {
        _pattern = pattern;
    }

    protected string _pattern;

    public virtual bool IsMatch(string fileName)
    {
        return Regex.IsMatch(fileName, _pattern);
    }
}

public class GlobPattern : RegexPattern
{
    public GlobPattern(string pattern)
        : base(pattern) { }

    public override bool IsMatch(string fileName)
    {
        return Glob.IsMatch(fileName, _pattern);
    }
}

// some magic for AOT
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ConfigFile))]
[JsonSerializable(typeof(Handler))]
internal partial class SourceGenerationContext : JsonSerializerContext { }
