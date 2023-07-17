using RoR2;
using System.Collections.Generic;
using RoRes;
using UnityEngine;

namespace RoRGauntlet
{
    public class ItemLogger : MonoBehaviour
    {
        public AdditionalMetadata.AdditionalInfo info => AdditionalMetadata.info;
        private StageLoot loot;
        private Dictionary<GameObject, UsefulInfo> populoot;

        private int idCounter = 0;
        public ItemLogger()
        {
            // reset stage stuff
            On.RoR2.Run.Start += LoadStart;
            On.RoR2.Run.BeginStage += LoadStage;

            // Get interactable tokens
            On.RoR2.PurchaseInteraction.Awake += LogAllInteractions;

            // multishop special

        }

        private void LoadStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            orig(self);

            idCounter = self.stageClearCount * 100; // im lazy
            AppendLoot();

            populoot = new();
            loot = new StageLoot() { stageName = self.nextStageScene.nameToken, stageNum = self.stageClearCount };
        }

        private void LoadStart(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);

            populoot = new();
            loot = new StageLoot() { stageName = self.nextStageScene.nameToken, stageNum = self.nextStageScene.stageOrder };
        }

        private void LogAllInteractions(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);

            // blacklisted: multishop terminals themselves
            if (self.displayNameToken == "MULTISHOP_TERMINAL_NAME") return;

            var pos = self.gameObject.transform.position;
            var info = new UsefulInfo()
            {
                x = pos.x,
                y = pos.y,
                z = pos.z,
                interactorName = self.displayNameToken,
                id = idCounter++
            };
            loot.stageLoot.Add(info);
            populoot.Add(self.gameObject, info);
        }

        public void PopulateLoot(GameObject key, ItemData item) { populoot[key].items.Add(item); }
        public void AppendLoot() { info.stageLoots.Add(loot); }
    }

    public class SpecialTokens
    {
        public const string WhiteMultishop = "MULTISHOP_WHITE_NAME";
        public const string GreenMultishop = "MULTISHOP_GREEN_NAME";
        public const string EquipMultishop = "MULTISHOP_ORANGE_NAME";
        public const string FallenMultishop = "MULTISHOP_SHORM_NAME";
    }

}