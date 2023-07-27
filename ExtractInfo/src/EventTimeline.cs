using RoR2;
using System.Collections.Generic;
using System.Collections;
using RoRes;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoRGauntlet
{
    public class EventTimeline : MonoBehaviour
    {
        public AdditionalMetadata.AdditionalInfo Info => AdditionalMetadata.info;
        public StageStartEvent currentStage;
        private readonly Dictionary<CharacterMaster, Coroutine> activeCharacterTrackers = new();

        public EventTimeline()
        {
            // stage splitting and run data
            On.RoR2.Run.BeginStage += StageSplit;

            // teleporter events
            TeleporterInteraction.onTeleporterFinishGlobal += (tp) =>
                AddEvent(new TeleportHitEvent() { timestamp = Run.instance.GetRunStopwatch() * 1000f }, tp.gameObject);
            TeleporterInteraction.onTeleporterChargedGlobal += (tp) =>
                AddEvent(new ChargeEndEvent() { timestamp = Run.instance.GetRunStopwatch() * 1000f, chargeType = ChargeType.teleporter }, tp.gameObject);
            TeleporterInteraction.onTeleporterBeginChargingGlobal += (tp) =>
                AddEvent(new ChargeStartEvent() { timestamp = Run.instance.GetRunStopwatch() * 1000f, chargeType = ChargeType.teleporter }, tp.gameObject);

            // pillars
            On.RoR2.MoonBatteryMissionController.OnBatteryCharged += (battery, charged, yes) =>
            {
                battery(charged, yes);
                AddEvent(new ChargeEndEvent() { timestamp = Run.instance.GetRunStopwatch() * 1000f, chargeType = ChargeType.pillar }, yes.gameObject);
            };

            // inventory events
            On.RoR2.CharacterMaster.OnItemAddedClient += PickupItem;
            On.RoR2.ScrapperController.BeginScrapping += ScrapItem;
            On.RoR2.PurchaseInteraction.CreateItemTakenOrb += PrintItem;

            // boss events
            On.RoR2.BossGroup.OnEnable += AddBoss;
            On.RoR2.BossGroup.OnDefeatedServer += KillBoss;
            On.EntityStates.Missions.BrotherEncounter.PreEncounter.OnEnter += SpawnMithry;

            // special item events
            On.RoR2.Items.ContagiousItemManager.StepInventoryInfection += ProcessVoidItems;
            On.RoR2.LunarSunBehavior.FixedUpdate += EgoSucks;

            // pizza
            On.EntityStates.BrotherMonster.UltEnterState.OnEnter += StartPizza;
            On.EntityStates.BrotherMonster.UltExitState.OnEnter += EndPizza;

            // character death
            On.RoR2.CharacterMaster.OnBodyDeath += GetReqt;
            CharacterMaster.onCharacterMasterDiscovered += LogCharacterPlease;
           

            // things you'd see in the chat
            On.RoR2.FamilyDirectorCardCategorySelection.OnSelected += LogFams;
            On.EntityStates.VoidCamp.Idle.OnEnter += VoidSeed;
            On.EntityStates.Fauna.VultureEggDeathState.OnEnter += EggLog;

            // portals and orbs
            //On.RoR2.PortalSpawner.OnWillSpawnUpdated += SpawnPortals;
            //On.RoR2.PortalSpawner.Start += SpawnOrbs;

        }

        private void SpawnMithry(On.EntityStates.Missions.BrotherEncounter.PreEncounter.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.PreEncounter self)
        {
            orig(self);

            AddEvent(new BossSpawnEvent()
            {
                boss = TokenHelper.MithrixBody,
                mountains = 0,
                timestamp = Run.instance.GetRunStopwatch() * 1000f
            }, self.gameObject);
        }

        bool delDupes = false;
        private void EggLog(On.EntityStates.Fauna.VultureEggDeathState.orig_OnEnter orig, EntityStates.Fauna.VultureEggDeathState self)
        {
            orig(self);

            if (delDupes = !delDupes) return;

            AddEvent(new MiscEvent()
            {
                eventInfo = "AWU Egg",
                timestamp = Run.instance.GetRunStopwatch() * 1000f
            }, self.gameObject);
        }

        private void VoidSeed(On.EntityStates.VoidCamp.Idle.orig_OnEnter orig, EntityStates.VoidCamp.Idle self)
        {
            orig(self);

            AddEvent(new MiscEvent()
            {
                eventInfo = "Void Seed",
                timestamp = Run.instance.GetRunStopwatch() * 1000f
            }, self.gameObject);
        }

        private void LogFams(On.RoR2.FamilyDirectorCardCategorySelection.orig_OnSelected orig, FamilyDirectorCardCategorySelection self, ClassicStageInfo stageInfo)
        {
            orig(self, stageInfo);

            AddEvent(new FamilyEventEvent()
            {
                timestamp = Run.instance.GetRunStopwatch() * 1000f,
                familyBodyName = self.categories[0].cards[0].spawnCard.name,
                x = 0,
                y = 0,
                z = 0
            });
        }
        private void LogCharacterPlease(CharacterMaster self)
        {
            Debug.Log("found player " + self);
            
            // TODO: VERIFY AND FIX
            if (self.playerCharacterMasterController != null)
            {
                Debug.Log("That was the local player, nice");  
                var pos = self.gameObject.transform.position;
                AddEvent(new SpawnInEvent()
                {
                    timestamp = Run.instance.GetRunStopwatch() * 1000f,
                    x = pos.x,
                    z = pos.z,
                    y = pos.y,
                    firstSpawn = Run.instance.stageClearCount == 0,
                    character = self.GetBody()?.baseNameToken,
                });

                if (activeCharacterTrackers.ContainsKey(self)) return;
                activeCharacterTrackers.Add(self, StartCoroutine(CheckCharacterPos(self)));
            }
        }

        private IEnumerator CheckCharacterPos(CharacterMaster master)
        {
            Debug.Log("Starting Logs for " + master.ToString());
            while (master != null && Run.instance != null)
            {
                var body = master.GetBody();
                yield return new WaitForSeconds(1);
                AddEvent(new CharacterExistEvent()
                {
                    timestamp = Run.instance.GetRunStopwatch() * 1000f,
                    health = body.healthComponent.combinedHealthFraction
                }, body.gameObject);
            }
            Debug.Log("Removing " + master.ToString());
            activeCharacterTrackers.Remove(master); // inactive
        }



        private void GetReqt(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            if (body.isPlayerControlled)
            {
                AddEvent(new DeathEvent()
                {
                    killer = BodyCatalog.GetBodyPrefab(self.GetKillerBodyIndex()).GetComponent<CharacterBody>().baseNameToken,
                    timestamp = Run.instance.GetRunStopwatch() * 1000f
                }, self.gameObject);
            }

            orig(self, body);
        }

        private void EndPizza(On.EntityStates.BrotherMonster.UltExitState.orig_OnEnter orig, EntityStates.BrotherMonster.UltExitState self)
        {
            orig(self);

            AddEvent(new PizzaExitEvent()
            {
                timestamp = Run.instance.GetRunStopwatch() * 1000f,
                mithrixHealth = self.characterBody.healthComponent.health,
            }, self.gameObject);
        }

        private void StartPizza(On.EntityStates.BrotherMonster.UltEnterState.orig_OnEnter orig, EntityStates.BrotherMonster.UltEnterState self)
        {
            orig(self);

            AddEvent(new PizzaEnterEvent()
            {
                timestamp = Run.instance.GetRunStopwatch() * 1000f,
                mithrixHealth = self.characterBody.healthComponent.health
            });
        }

        private void EgoSucks(On.RoR2.LunarSunBehavior.orig_FixedUpdate orig, LunarSunBehavior self)
        {
            // shitty copy code :)
            if (self.transformTimer > 60f)
            {
                // simulate
                var list = new List<ItemIndex>(self.body.inventory.itemAcquisitionOrder);
                var clone = new Xoroshiro128Plus(0)
                {
                    state0 = self.transformRng.state0,
                    state1 = self.transformRng.state1
                };
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
                    timestamp = Run.instance.GetRunStopwatch() * 1000f,
                    transactionType = Transaction.Ego,
                    quantity = -1,
                    item = new Item() { englishName = Eng(itemDef.nameToken), tier = itemDef.tier }
                });

                // gain ego
                AddEvent(new InventoryEvent()
                {
                    timestamp = Run.instance.GetRunStopwatch() * 1000f,
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
                timestamp = Run.instance.GetRunStopwatch() * 1000f,
                transactionType = Transaction.Corrupted,
                quantity = -inventory.GetItemCount(normalItem),
                item = new Item() { englishName = Eng(normalItem.nameToken), tier = normalItem.tier }
            });

            // gain voids
            AddEvent(new InventoryEvent()
            {
                timestamp = Run.instance.GetRunStopwatch() * 1000f,
                transactionType = Transaction.Corrupted,
                quantity = inventory.GetItemCount(normalItem),
                item = new Item() { englishName = Eng(voidItem.nameToken), tier = voidItem.tier }
            });

            return orig(inventory, originalItem, limit, isForced);
        }

        private void KillBoss(On.RoR2.BossGroup.orig_OnDefeatedServer orig, BossGroup self)
        {
            orig(self);

            AddEvent(new BossKillEvent()
            {
                timestamp = Run.instance.GetRunStopwatch() * 1000f,
                boss = self.bossMemories[0].cachedBody.baseNameToken
            }, self.gameObject);
        }

        private void AddBoss(On.RoR2.BossGroup.orig_OnEnable orig, BossGroup self)
        {
            orig(self);

            AddEvent(new BossSpawnEvent()
            {
                timestamp = Run.instance.GetRunStopwatch() * 1000f,
                boss = self.bossMemories[0].cachedBody.baseNameToken,
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
                    timestamp = Run.instance.GetRunStopwatch() * 1000f,
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
                timestamp = Run.instance.GetRunStopwatch() * 1000f,
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
                    timestamp = Run.instance.GetRunStopwatch() * 1000f,
                    transactionType = Transaction.Pickup,
                    quantity = 1, // gained 1 item per pickup
                    x = pos.x,
                    y = pos.y,
                    z = pos.z
                }); ;
            }
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
                    timestamp = Run.instance.GetRunStopwatch() * 1000f
                }); // end stage
            }


            orig(self);

            currentStage = new StageStartEvent()
            {
                timestamp = Run.instance.GetRunStopwatch() * 1000f,
                stageNum = Run.instance.stageClearCount + 1,
                englishName = Eng(self.nextStageScene.nameToken)
            };

            Debug.Log("Starting split: " + Eng(self.nextStageScene.nameToken));
            AddEvent(currentStage);
        }

        public void AddEvent(RunEvent ev, GameObject posObj = null)
        {
            if (ev.timestamp == default) ev.timestamp = Run.instance.GetRunStopwatch() * 1000f;
            if (posObj != null)
            {
                var pos = posObj.transform.position;
                ev.x = pos.x; ev.y = pos.y; ev.z = pos.z; // xyz 
            }

            ev.eventType = ev.GetType().Name; // add type name for clarify

            Info.runEvents.Add(ev);
        }

        private string Eng(string nameToken)
        {
            return nameToken; // just token now, better 
        }


    }

}