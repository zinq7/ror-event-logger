using BepInEx;
using RoR2;

namespace ModTemplate
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "RiskOfResources";
        public const string PluginName = "ModTemplate";
        public const string PluginVersion = "0.0.1";

        public void Awake()
        {
            Log.Init(Logger);
        }
    }
}