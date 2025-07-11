using BepInEx;
using System.IO;
using System;
using UnityEngine;
using System.Net.Http;
using System.Text;
using BepInEx.Logging;
using RoRGauntlet;

/**
 * Wrapper for RoRGauntlet mod
 */
public static class RoRGauntletWrapper
{
    private static string _user_id = "INVALID_USER";

    private static string _env = "INVALID_ENV";

    private static string _loadout_num = "INVALID_LOADOUT_NUM";

    private static string _current_run_string = "NOT_FOUND";

    // Constants
    private const string POST_URL = "https://bage.cab/_RoR2Run/"; // for prod build

    // private const string POST_URL = "https://127.0.0.1/_RoR2Run/"; // for local testing

    private const string GAUNTLET_NAME = "RiskOfResources.RoRGauntlet";

    private const string SALT = "2z5clNc0M5wnV"; // Random string

    /**
    * Uploads run summary to a server.
    * 
    * @param string filePath  The path to the file to upload
    *
    * @return void
    */
    public static async void _UploadRun(string filePath)
    {
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GAUNTLET_NAME))
        {
            _InitVars();
            string end_time = AdditionalMetadata.info.endTime.ToString();
            string num_deaths = AdditionalMetadata.info.num_deaths.ToString();

            Debug.Log("trying to upload run...");
            var client = new HttpClient
            {
                BaseAddress = new(POST_URL)
            };
            
            var stream = System.IO.File.OpenRead(filePath);
            using var request = new HttpRequestMessage(HttpMethod.Post, "uploadRun");
            using var content = new MultipartFormDataContent
            {
                { new StreamContent(stream), "file", filePath },
                { new StringContent(_user_id), "user_id" },
                { new StringContent(_env), "env" },
                { new StringContent(_loadout_num), "loadout_num" },
                { new StringContent(_current_run_string), "lobby_string" },
                { new StringContent(end_time), "end_time" },
                { new StringContent(num_deaths), "num_deaths" },
                { new StringContent(SALT), "salt" },
            };

            request.Content = content;

            await client.SendAsync(request);
            Debug.Log("run uploaded");
        }

        else
        {
            Debug.Log("not uploading run, RoRGauntlet not enabled");
        }
    }

    public static void _InitVars()
    {
        var filePath = Paths.ConfigPath + $"\\{GAUNTLET_NAME}.cfg";
        // Init user_id

        /** idk how to use config but this is not working
        var RoRGauntletConfig = new ConfigFile(filePath, false);
        RoRGauntletConfig.TryGetEntry<string>("Player", "User Id", out ConfigEntry<string> user_id);
        if (user_id != null)
        {
            _user_id = user_id.Value;
        }*/

        // just read the file lol
        using (var fileStream = File.OpenRead(filePath))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true)) {
            string line;
            
            // Will need to be changed if config format changes
            while ((line = streamReader.ReadLine()) != null)
            {
                // Init user ID
                if (line.StartsWith("User Id = "))
                {
                    _user_id = line.Split('#')[1];
                }
                
                // Init env
                else if (line.StartsWith("Environment"))
                {
                    _env = line.Split(new [] {" = "}, StringSplitOptions.None)[1];
                }

                // Init loadout number
                else if (line.StartsWith("Loadout Number"))
                {
                    _loadout_num = line.Split(new [] {" = "}, StringSplitOptions.None)[1];
                }
            }
        }
    }

    public static void SetRunString(string runString)
    {
        _current_run_string = runString;
    }
}

/**
* Listen to the log
*/
public class RoRLogListener : ILogListener
{
    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        if (eventArgs.Source.SourceName == "LoadoutHandler")
        {
            // Grab the race data when it's logged
            string event_str = eventArgs.Data.ToString();
            if (event_str.StartsWith("Keeping track of race"))
            {
                // We're keeping track of the race too
                RoRGauntletWrapper.SetRunString(event_str.Split(new [] {"e : "}, StringSplitOptions.None)[1]); // e
            }
        }
    }

    public void Dispose() {}
}