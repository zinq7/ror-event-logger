using BepInEx;
using RoR2;
using RoRGauntlet;
using System;
using UnityEngine;

namespace ExtractInfo
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "RiskOfResources";
        public const string PluginName = "ExtractInfo";
        public const string PluginVersion = "0.0.1";

        const string FOLDER = "C:\\Users\\16132\\Documents\\Modding\\EXTRACTION\\";

        public void Awake()
        {
            new AdditionalMetadata();
            // On.RoR2.AwakeEvent.Awake += ListSurvs;
        }

    }
}