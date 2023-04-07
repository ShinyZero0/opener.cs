using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
// using Newtonsoft.Json;
using MimeKit;

internal static class Program
{
    private static void Main(string[] args)
    {
        string configFile = Path.Combine(
            Environment.GetEnvironmentVariable("HOME"),
            ".config/jajaro.json"
        );
        string raw;
        try
        {
            raw = File.ReadAllText(configFile);
        }
        catch
        {
            Console.WriteLine("Create a config file in ~/.config/jajaro.json please");
            return;
        }
        try
        {
            // Config = JsonConvert.DeserializeObject<ConfigFile>(raw);
            Config = JsonSerializer.Deserialize<ConfigFile>(raw, SourceGenerationContext.Default.ConfigFile);
        }
        catch (Exception exc)
        {
            Console.WriteLine($"Problem with your config:\n{exc}");
        }
        foreach (Handler handler in Config.Handlers)
        {
            foreach (string mimetype in handler.Types)
            {
                HandlerMaps[mimetype] = handler;
            }
        }
        foreach (string file in args)
        {
            OpenFile(file);
        }
        return;
    }

    private static Dictionary<string, Handler> HandlerMaps = new();
    private static ConfigFile Config;

    private static void OpenFile(string fileName)
    {
        Handler? handler = null;
        foreach (KeyValuePair<string, Handler> entry in HandlerMaps.Reverse())
        {
            if (Regex.IsMatch(MimeTypes.GetMimeType(fileName), entry.Key))
            {
                handler = entry.Value;
                break;
            }
        }
        if (handler is null)
        {
            Console.WriteLine("handler not defined");
            return;
        }
        Process proc;
        if (Console.IsInputRedirected && handler.Term)
        {
            string termPrefix = Config.Term;
            proc = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/sh",
                    Arguments = String.Join(' ', "-c \"", termPrefix, handler.Exec, fileName, "\""),
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
                    Arguments = String.Join(' ', "-c \"", handler.Exec, fileName, "\""),
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

    // public string TermExec;

    public Handler() { }
}

public struct ConfigFile
{
    [JsonInclude]
    public string Term;

    [JsonInclude]
    public Handler[] Handlers;
}
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ConfigFile))]
[JsonSerializable(typeof(Handler))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
