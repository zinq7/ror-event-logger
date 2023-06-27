using RoR2;
using Newtonsoft.Json;
using System.Collections.Generic;
using RoRes;
using BepInEx;
using System.IO;
using System;
using RoRGauntlet;
using UnityEngine;

namespace RoRGauntlet
{
    public class AdditionalMetadata
    {
        public static AdditionalInfo info;
        public static StageStartEvent currentStage;
        public static string FilePath = Paths.BepInExRootPath;
        public AdditionalMetadata()
        {
            
            // stage splitting and run data
            On.RoR2.Run.Start += LoadInfo;
            On.RoR2.Run.BeginStage += StageSplit;

            // teleporter events
            TeleporterInteraction.onTeleporterFinishGlobal += (tp) =>
                AddEvent(new ChargeEndEvent() { timestamp = Run.instance.GetRunStopwatch(), chargeType = ChargeType.teleporter }, tp.gameObject);
            TeleporterInteraction.onTeleporterBeginChargingGlobal += (tp) =>
                AddEvent(new ChargeStartEvent() { timestamp = Run.instance.GetRunStopwatch(), chargeType = ChargeType.teleporter }, tp.gameObject);

            // inventory events
            On.RoR2.CharacterMaster.OnItemAddedClient += PickupItem;
            On.RoR2.ScrapperController.BeginScrapping += ScrapItem;
            On.RoR2.PurchaseInteraction.CreateItemTakenOrb += PrintItem;

            // boss events
            On.RoR2.BossGroup.OnEnable += AddBoss;
            On.RoR2.BossGroup.OnDefeatedServer += KillBoss;

            // special item events
            On.RoR2.Items.ContagiousItemManager.StepInventoryInfection += ProcessVoidItems;
            On.RoR2.LunarSunBehavior.FixedUpdate += EgoSucks;

            // pizza
            On.EntityStates.BrotherMonster.UltEnterState.OnEnter += StartPizza;
            On.EntityStates.BrotherMonster.UltExitState.OnEnter += EndPizza;

            // character death
            On.RoR2.CharacterMaster.OnBodyDeath += GetReqt;
            //  On.RoR2.CharacterBody.FixedUpdate += LogPositionTooMuchLoggingIDC;

            // EXPORT
            On.RoR2.Run.BeginGameOver += SaveToFile;

            // On.RoR2.Chat.HandleBroadcastChat += OnChatBreakEverything;
            // On.RoR2.CharacterMaster.OnEnable += UrHere;
        }

        private void OnChatBreakEverything(On.RoR2.Chat.orig_HandleBroadcastChat orig, UnityEngine.Networking.NetworkMessage netMsg)
        {
            orig(netMsg);

            if (!netMsg.reader.ReadString().Equals("pog")) return;

            int i = 0;
            foreach (var node in SceneInfo.instance.groundNodes.nodes)
            {
                if (i++ % 2 == 0) continue;
                var pos = node.position;
                pos.y += 30;

                int resWidth = 200;
                int resHeight = 200;

                var circle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                circle.SetActive(false);
                circle.transform.position = pos;
                var camera = circle.AddComponent<Camera>();
                camera.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));


                RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
                camera.targetTexture = rt;
                Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
                camera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                camera.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                UnityEngine.Object.Destroy(rt);


                byte[] bytes = screenShot.EncodeToPNG();

                System.IO.Directory.CreateDirectory("($\"{FilePath}\\Images\\")
                System.IO.File.WriteAllBytes($"{FilePath}\\Images\\{pos.x}_{pos.y}_{pos.z}.png", bytes);
                Debug.Log(string.Format("Took screenshot to: {0}", $"{FilePath}\\{pos.x}_{pos.y}_{pos.z}.png"));
            }
        }

        static int time = 0;
        private void LogPositionTooMuchLoggingIDC(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);

            const int POSITION_LOG_INTERVAL = 1 * 60; // 1 seconds

            if (self != null && self.master != null && self.master.GetBody() != null && self.master.GetBody().isPlayerControlled)
            {
                time++;
                if (POSITION_LOG_INTERVAL % time == 0)
                {
                    AddEvent(new CharacterExistEvent()
                    {
                        timestamp = Run.instance.GetRunStopwatch(),
                        health = self.healthComponent.combinedHealthFraction
                    }, self.gameObject);
                }
            }
        }

        private void SaveToFile(On.RoR2.Run.orig_BeginGameOver orig, Run self, GameEndingDef gameEndingDef)
        {
            orig(self, gameEndingDef);

            var logFolder = $"{FilePath}\\RunReports";
            Directory.CreateDirectory(logFolder);

            File.WriteAllText(logFolder + "\\" + DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss") + ".run.json", GetJSON()); // cool
        }

        private void GetReqt(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            if (body.isPlayerControlled)
            {
                AddEvent(new DeathEvent()
                {
                    killer = BodyCatalog.GetBodyName(self.GetKillerBodyIndex()),
                    timestamp = Run.instance.GetRunStopwatch()
                }, self.gameObject);
            }

            orig(self, body);
        }

        private void EndPizza(On.EntityStates.BrotherMonster.UltExitState.orig_OnEnter orig, EntityStates.BrotherMonster.UltExitState self)
        {
            orig(self);

            AddEvent(new PizzaExitEvent()
            {
                timestamp = Run.instance.GetRunStopwatch(),
                mithrixHealth = self.characterBody.healthComponent.health,
            }, self.gameObject);
        }

        private void StartPizza(On.EntityStates.BrotherMonster.UltEnterState.orig_OnEnter orig, EntityStates.BrotherMonster.UltEnterState self)
        {
            orig(self);

            AddEvent(new PizzaEnterEvent()
            {
                timestamp = Run.instance.GetRunStopwatch(),
                mithrixHealth = self.characterBody.healthComponent.health
            });
        }

        private void EgoSucks(On.RoR2.LunarSunBehavior.orig_FixedUpdate orig, LunarSunBehavior self)
        {
            // shitty copy code :)
            if (self.transformTimer > 60f)
            {
                // simulate
                List<ItemIndex> list = new List<ItemIndex>(self.body.inventory.itemAcquisitionOrder);
                Xoroshiro128Plus clone = new Xoroshiro128Plus(0);
                clone.state0 = self.transformRng.state0; clone.state1 = self.transformRng.state1;
                Util.ShuffleList(list, clone);

                // simulate
                ItemDef itemDef = null;
                foreach (ItemIndex item in list)
                {
                    if (item != DLC1Content.Items.LunarSun.itemIndex)
                    {
                        itemDef = ItemCatalog.GetItemDef(item);
                        if ((bool)itemDef && itemDef.tier != ItemTier.NoTier)
                        {
                            break;
                        }
                    }
                }

                // lost normal
                AddEvent(new InventoryEvent()
                {
                    timestamp = Run.instance.GetRunStopwatch(),
                    transactionType = Transaction.Ego,
                    quantity = -1,
                    item = new Item() { englishName = Eng(itemDef.nameToken), tier = itemDef.tier }
                });

                // gain ego
                AddEvent(new InventoryEvent()
                {
                    timestamp = Run.instance.GetRunStopwatch(),
                    transactionType = Transaction.Ego,
                    quantity = 1,
                    item = new Item() { englishName = Eng(DLC1Content.Items.LunarSun.nameToken), tier = DLC1Content.Items.LunarSun.tier }
                });
            }
            orig(self);
        }

        private bool ProcessVoidItems(On.RoR2.Items.ContagiousItemManager.orig_StepInventoryInfection orig, Inventory inventory, ItemIndex originalItem, int limit, bool isForced)
        {
            ItemDef voidItem = ItemCatalog.GetItemDef(RoR2.Items.ContagiousItemManager.originalToTransformed[(int)originalItem]), normalItem = ItemCatalog.GetItemDef(originalItem);

            // lost normals
            AddEvent(new InventoryEvent()
            {
                timestamp = Run.instance.GetRunStopwatch(),
                transactionType = Transaction.Corrupted,
                quantity = -inventory.GetItemCount(normalItem),
                item = new Item() { englishName = Eng(normalItem.nameToken), tier = normalItem.tier }
            });

            // gain voids
            AddEvent(new InventoryEvent()
            {
                timestamp = Run.instance.GetRunStopwatch(),
                transactionType = Transaction.Corrupted,
                quantity = inventory.GetItemCount(normalItem),
                item = new Item() { englishName = Eng(voidItem.nameToken), tier = voidItem.tier }
            });

            return orig(inventory, originalItem, limit, isForced);
        }

        private void KillBoss(On.RoR2.BossGroup.orig_OnDefeatedServer orig, BossGroup self)
        {
            AddEvent(new BossKillEvent() { timestamp = Run.instance.GetRunStopwatch(), boss = self.bestObservedName }, self.gameObject);

            orig(self);
        }

        private void AddBoss(On.RoR2.BossGroup.orig_OnEnable orig, BossGroup self)
        {
            orig(self);

            AddEvent(new BossSpawnEvent()
            {
                timestamp = Run.instance.GetRunStopwatch(),
                boss = Util.GetBestBodyName(self.bossMemories[0].cachedBody.gameObject),
                mountains = self.bonusRewardCount
            }, self.gameObject);
        }

        bool cursedIgnoreNextBecauseImLazy = false;

        private void PrintItem(On.RoR2.PurchaseInteraction.orig_CreateItemTakenOrb orig, UnityEngine.Vector3 effectOrigin, UnityEngine.GameObject targetObject, ItemIndex itemIndex)
        {
            orig(effectOrigin, targetObject, itemIndex);

            if (cursedIgnoreNextBecauseImLazy)
            {
                cursedIgnoreNextBecauseImLazy = false;
            }
            else
            {
                var pos = targetObject.transform.position;
                var item = ItemCatalog.GetItemDef(itemIndex);
                AddEvent(new InventoryEvent()
                {
                    item = new Item() { englishName = Eng(item.nameToken), tier = item.tier },
                    timestamp = Run.instance.GetRunStopwatch(),
                    transactionType = Transaction.Print,
                    quantity = -1,
                    x = pos.x,
                    y = pos.y,
                    z = pos.z
                });
            }
        }

        private void ScrapItem(On.RoR2.ScrapperController.orig_BeginScrapping orig, ScrapperController self, int intPickupIndex)
        {
            orig(self, intPickupIndex);

            var characterBody = self.interactor.GetComponent<CharacterBody>();
            var item = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(new PickupIndex(intPickupIndex)).itemIndex);
            var pos = characterBody.corePosition;
            AddEvent(new InventoryEvent()
            {
                item = new Item() { englishName = Eng(item.nameToken), tier = item.tier },
                timestamp = Run.instance.GetRunStopwatch(),
                transactionType = Transaction.Scrap,
                quantity = (characterBody.inventory.GetItemCount(item) > 10) ? -10 : -characterBody.inventory.GetItemCount(item),
                x = pos.x,
                y = pos.y,
                z = pos.z
            });

            cursedIgnoreNextBecauseImLazy = true;
        }

        private void PickupItem(On.RoR2.CharacterMaster.orig_OnItemAddedClient orig, CharacterMaster self, ItemIndex itemIndex)
        {
            orig(self, itemIndex);

            if (self != null && self.GetBody() != null && self.GetBody().isPlayerControlled)
            {
                var item = ItemCatalog.GetItemDef(itemIndex);
                var pos = self.GetBody().corePosition;
                AddEvent(new InventoryEvent()
                {
                    item = new Item() { englishName = Eng(item.nameToken), tier = item.tier },
                    timestamp = Run.instance.GetRunStopwatch(),
                    transactionType = Transaction.Pickup,
                    quantity = 1, // gained 1 item per pickup
                    x = pos.x,
                    y = pos.y,
                    z = pos.z
                }); ;
            }
        }

        private void LoadInfo(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);
            info = new AdditionalInfo();

            // artifacts
            var enumerator = RunArtifactManager.enabledArtifactsEnumerable.GetEnumerator();
            while (enumerator.Current != null)
            {
                info.artifacts.Add(enumerator.Current.nameToken);
                enumerator.MoveNext();
            }
            info.difficulty = self.selectedDifficulty.ToString();
            info.player = "REPLACE"; // TODO: replace with real character

            currentStage = null; // not to transfer over between runs
            time = 0;
        }

        private void StageSplit(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            if (currentStage is not null)
            {
                UnityEngine.Debug.Log("Ending split: " + currentStage.englishName);
                AddEvent(new StageEndEvent()
                {
                    stageNum = currentStage.stageNum,
                    englishName = currentStage.englishName,
                    timestamp = Run.instance.GetRunStopwatch()
                }); // end stage
            }


            orig(self);

            currentStage = new StageStartEvent()
            {
                timestamp = Run.instance.GetRunStopwatch(),
                stageNum = Run.instance.stageClearCount + 1,
                englishName = Eng(self.nextStageScene.nameToken)
            };

            UnityEngine.Debug.Log("Starting split: " + Eng(self.nextStageScene.nameToken));
            AddEvent(currentStage);
        }

        private void AddEvent(RunEvent ev, UnityEngine.GameObject posObj = null)
        {
            if (ev.timestamp == default) ev.timestamp = Run.instance.GetRunStopwatch();
            if (posObj != null)
            {
                var pos = posObj.transform.position;
                ev.x = pos.x; ev.y = pos.y; ev.z = pos.z; // xyz 
            }

            ev.eventType = ev.GetType().Name; // add type name for clarify

            info.runEvents.Add(ev);
        }

        private string Eng(string nameToken)
        {
            return Language.english.GetLocalizedStringByToken(nameToken);
        }

        public static void ClearMetadata()
        {
            info.runEvents = new();
        }

        public static string GetJSON()
        {
            return JsonConvert.SerializeObject(info);
        }

        public class AdditionalInfo
        {
            public List<RunEvent> runEvents = new();
            public List<string> artifacts = new();
            public string difficulty = "Eclipse8";
            public string player = "NONE";
        }
    }

}