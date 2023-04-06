using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using HeyRed.Mime;
using Newtonsoft.Json;
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
        Config = JsonConvert.DeserializeObject<ConfigFile>(raw);
        foreach (Handler handler in Config.Handlers)
        {
            foreach (string mimetype in handler.Types)
            {
                HandlerMaps[mimetype] = handler.Exec;
            }
        }
        foreach (string file in args) {
            OpenFile(file);
        }
    }

    private static Dictionary<string, string> HandlerMaps = new();
    private static ConfigFile Config;

    private static void OpenFile(string file)
    {
        string handlerExec = null;
        foreach (KeyValuePair<string, string> entry in HandlerMaps)
        {
            if (Regex.IsMatch(MimeTypesMap.GetMimeType(file), entry.Key))
            {
                handlerExec = entry.Value;
                break;
            }
        }
        if (handlerExec is null)
        {
            Console.WriteLine("handler not defined");
            return;
        }
        string termPrefix = "";
        if (Environment.GetEnvironmentVariable("TERM") is null)
            termPrefix = Config.Term;
        Process proc =
            new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = String.Concat(termPrefix, handlerExec),
                    Arguments = file,
                    WorkingDirectory = Environment.GetEnvironmentVariable("PWD"),
                    // RedirectStandardOutput = true,
                    UseShellExecute = false,
                }
            };
        proc.Start();
        // Console.WriteLine(proc.StartInfo.FileName);
        proc.WaitForExit();
    }
}

public class Handler
{
    public string[] Types;
    public string Exec;

    // public string TermExec;

    public Handler() { }
}

public struct ConfigFile
{
    public string Term;
    public Handler[] Handlers;
}
