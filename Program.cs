using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;

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
            foreach (string regex in handler.Types)
            {
                RegexHandlers[regex] = handler;
            }
        }
        OpenFiles(args);
        return 0;
    }

    private static Dictionary<string, Handler> RegexHandlers = new();
    private static ConfigFile Config;

    private static void OpenFiles(string[] fileNames)
    {
        Tuple<string, Handler>? handlerTuple = null;
        foreach (KeyValuePair<string, Handler> entry in RegexHandlers)
        {
            if (Regex.IsMatch(fileNames[0], entry.Key))
            {
                handlerTuple = new(entry.Key, entry.Value);
                break;
            }
        }
        if (handlerTuple is null)
        {
            Console.WriteLine("handler not defined");
            return;
        }
        foreach (string fileName in fileNames)
        {
            if (!Regex.IsMatch(fileName, handlerTuple.Item1))
            {
                Console.WriteLine("These files have different handlers!");
                return;
            }
        }
        Process proc;
        if (Console.IsInputRedirected && handlerTuple.Item2.Term)
        {
            string termPrefix = Config.Term;
            proc = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/sh",
                    Arguments = String.Join(
                        ' ',
                        "-c \"",
                        termPrefix,
                        handlerTuple.Item2.Exec,
                        string.Join(' ', fileNames),
                        "\""
                    ),
                    CreateNoWindow = false,
                }
            };
        }
        else
        {
            proc = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/sh",
                    Arguments = String.Join(
                        ' ',
                        "-c \"",
                        handlerTuple.Item2.Exec,
                        string.Join(' ', fileNames),
                        "\""
                    ),
                }
            };
        }
        proc.Start();
        // Console.WriteLine(proc.StartInfo.FileName);
        proc.WaitForExit();
    }
}

public partial class Handler
{
    [JsonInclude]
    public string[] Types;

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

// some magic for AOT
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ConfigFile))]
[JsonSerializable(typeof(Handler))]
internal partial class SourceGenerationContext : JsonSerializerContext { }
