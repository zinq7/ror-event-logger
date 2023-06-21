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

        private void ListSurvs(On.RoR2.AwakeEvent.orig_Awake orig, AwakeEvent self)
        {
            orig(self);

            foreach (var item in RoR2.BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                if (item.portraitIcon is not null)
                {
                    var texture = item.portraitIcon;

                    // Create a temporary RenderTexture of the same size as the texture
                    RenderTexture tmp = RenderTexture.GetTemporary(
                                        texture.width,
                                        texture.height,
                                        0,
                                        RenderTextureFormat.Default,
                                        RenderTextureReadWrite.Linear);

                    // Blit the pixels on texture to the RenderTexture
                    Graphics.Blit(texture, tmp);


                    // Backup the currently set RenderTexture
                    RenderTexture previous = RenderTexture.active;


                    // Set the current RenderTexture to the temporary one we created
                    RenderTexture.active = tmp;


                    // Create a new readable Texture2D to copy the pixels to it
                    Texture2D myTexture2D = new Texture2D(texture.width, texture.height);


                    // Copy the pixels from the RenderTexture to the new Texture
                    myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                    myTexture2D.Apply();


                    // Reset the active RenderTexture
                    RenderTexture.active = previous;


                    // Release the temporary RenderTexture
                    RenderTexture.ReleaseTemporary(tmp);


                    var png = myTexture2D.EncodeToPNG();
                    string path = "/" + Language.english.GetLocalizedStringByToken(item.baseNameToken) + ".png";
                    path = FOLDER + path.Replace(":", " -");
                    path = path.Replace("?", "x");
                    System.IO.File.WriteAllBytes(path, png);
                }
            }

            On.RoR2.AwakeEvent.Awake -= ListSurvs;
        }

    }
}