using BepInEx;
using BepInEx.Logging;
using RoR2;
using RoRGauntlet;
using System;
using UnityEngine;

namespace ExtractInfo
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("RoRGauntlet", BepInDependency.DependencyFlags.SoftDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "riskoresources-dev";
        public const string PluginName = "ExtractInfo";
        public const string PluginVersion = "0.0.1";

        public void Awake()
        {
            BepInEx.Logging.Logger.Listeners.Add(new RoRLogListener());
            new AdditionalMetadata();
            instance = this;
        }

        public static Main instance;
    }
}