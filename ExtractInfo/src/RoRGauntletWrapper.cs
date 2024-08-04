using BepInEx;
using System.IO;
using System;
using UnityEngine;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

/**
 * Wrapper for RoRGauntlet mod
 */
public static class RoRGauntletWrapper
{
    private static string _user_id = null;

    // Constants
    private const string POST_URL = "http://bage.cab/_RoR2Run/";

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
        string user_id;
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GAUNTLET_NAME))
        {
            user_id = _GetUser();
            if (user_id == null || user_id == "")
            {
                user_id = "INVALID_USER";
            }

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
                { new StringContent(user_id), "user_id" },
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

    private static string _GetUser()
    {
        Debug.Log("Getting user");
        if (_user_id == null)
        {
            var filePath = Paths.ConfigPath + $"\\{GAUNTLET_NAME}.cfg";
            Debug.Log("User id null: initializing");
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
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.StartsWith("User Id = "))
                    {
                        _user_id = line.Split('#')[1];
                        break;
                    }
                }
            }
        }

        Debug.Log("User ID " + _user_id);
        return _user_id == null ? "" : _user_id;
    }

}