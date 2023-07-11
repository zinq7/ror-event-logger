using RoR2;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections;
using RoRes;
using BepInEx;
using System.IO;
using System;
using RoRGauntlet;
using UnityEngine;
using EntityStates.VoidRaidCrab;

namespace RoRGauntlet
{
    public class ItemLogger : MonoBehaviour
    {
        public AdditionalMetadata.AdditionalInfo info => AdditionalMetadata.info;
        private StageLoot loot;
        private int idCounter = 0;
        public ItemLogger()
        {
            // reset stage stuff
            On.RoR2.Run.Start += LoadStart;
            On.RoR2.Run.BeginStage += LoadStage;

            // Log ALL interactables
            On.RoR2.PurchaseInteraction.Awake += LogAllInteractions;
        }

        private void LoadStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            orig(self);

            idCounter = self.stageClearCount * 100; // im lazy
            AppendLoot();

            loot = new StageLoot() { stageName = self.nextStageScene.nameToken, stageNum = self.stageClearCount };
        }

        private void LoadStart(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);

            loot = new StageLoot() { stageName = self.nextStageScene.nameToken, stageNum = self.nextStageScene.stageOrder };
        }

        private void LogAllInteractions(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);

            // blacklisted: multishop terminals themselves
            if (self.displayNameToken == "MULTISHOP_TERMINAL_NAME") return;

            var pos = self.gameObject.transform.position;
            loot.stageLoot.Add(new UsefulInfo()
            {
                x = pos.x,
                y = pos.y,
                z = pos.z,
                interactorName = self.displayNameToken,
                id = idCounter++
            });
        }

        public void AppendLoot() { info.stageLoots.Add(loot); }
    }

}