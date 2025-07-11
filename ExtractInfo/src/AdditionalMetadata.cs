using RoR2;
using Newtonsoft.Json;
using System.Collections.Generic;
using RoRes;
using BepInEx;
using System.IO;
using System;
using UnityEngine;

namespace RoRGauntlet
{
    public class AdditionalMetadata : MonoBehaviour
    {
        public static AdditionalInfo info;
        public static string FilePath = Paths.BepInExRootPath;

        private readonly EventTimeline timeliner;
        private readonly ItemLogger itemLogger;
        public AdditionalMetadata()
        {
            Debug.Log("AWAKE STARTED");

            timeliner = new EventTimeline(); // event timeline
            itemLogger = new ItemLogger(); // item logs
           
            On.RoR2.GenericSkill.OnExecute += SkillLog;  // using skills
            On.RoR2.CharacterMaster.OnBodyDeath += (orig, self, body) =>
            {
                if (body.isPlayerControlled == true && self.inventory.GetItemCount(RoR2Content.Items.ExtraLife) == 0 && self.inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoid) == 0)
                {
                    info.num_deaths++;
                }
                orig(self, body);
            };

            On.RoR2.Run.Start += LoadInfo;
            On.RoR2.Run.OnClientGameOver += SaveToFile; // EXPORT ALL DATA (to a file)
        }

        private void LoadInfo(On.RoR2.Run.orig_Start orig, Run self)
        {
            info = new AdditionalInfo(); // reset info

            // artifacts TODO: fix
            var enumerator = RunArtifactManager.enabledArtifactsEnumerable.GetEnumerator();
            while (enumerator.Current != null)
            {
                info.artifacts.Add(enumerator.Current.nameToken);
                enumerator.MoveNext();
            }
            info.difficulty = self.selectedDifficulty.ToString();

            // doesn't work with amd
            //info.player = SteamworksClientManager.instance.steamworksClient.Username; // steam username

            timeliner.currentStage = null; // not to transfer over between runs

            orig(self);
        }

        private void SaveToFile(On.RoR2.Run.orig_OnClientGameOver orig, Run self, RunReport rep)
        {
            info.endTime = self.GetRunStopwatch() * 1000;
            timeliner.AddEvent(new RunEndEvent() { timestamp = info.endTime, x = 0, y = 0, z = 0 });
            itemLogger.AppendLoot();

            orig(self, rep);

            var logFolder = $"{FilePath}\\RunReports";
            Directory.CreateDirectory(logFolder);

            var filePath = logFolder + "\\" + DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss") + ".run.json";
            File.WriteAllText(filePath, GetJSON());

            RoRGauntletWrapper._UploadRun(filePath);
        }

        private void SkillLog(On.RoR2.GenericSkill.orig_OnExecute orig, GenericSkill self)
        {
            orig(self);

            if (self.skillNameToken == "") return;
            bool found = info.skillUses.TryGetValue(self.skillNameToken, out int numUses);
            info.skillUses[self.skillNameToken] = found ? numUses + 1 : 1;
        }

        public string GetJSON()
        {
            return JsonConvert.SerializeObject(info);
        }

        public class AdditionalInfo
        {
            public List<RunEvent> runEvents = new(); // timeline of events that occur (generally player initiated)
            public List<string> artifacts = new(); // list of artifact namess
            public string difficulty = "Eclipse8"; // Easy, Medium, Hard, eclispes
            public string player = "NONE"; // player (i.e ZINQ)
            public Dictionary<string, int> skillUses = new(); // skillname, use#
            public List<StageLoot> stageLoots = new(); // list of interactables
            public int num_deaths = 0;
            public float endTime = 0;
        }

    }

}