using RoR2;
using System.Collections.Generic;
using RoRes;
using UnityEngine;

namespace RoRGauntlet
{
    public class ItemLogger : MonoBehaviour
    {
        public AdditionalMetadata.AdditionalInfo Info => AdditionalMetadata.info;
        private StageLoot loot;
        private Dictionary<GameObject, UsefulInfo> populoot;

        private int idCounter = 0;
        public ItemLogger()
        {
            // reset stage stuff
            On.RoR2.Run.Start += LoadStart;
            On.RoR2.Run.BeginStage += LoadStage;

            // Get interactable tokens
            On.RoR2.PurchaseInteraction.OnEnable += LogAllInteractions;
            On.RoR2.MultiShopController.CreateTerminals += LogMultishopControllers;

            // LOOT
            On.RoR2.ChestBehavior.Roll += LogChestRoll;
            On.RoR2.ShopTerminalBehavior.SetPickupIndex += SetPrinterIndex;
            On.RoR2.OptionChestBehavior.Roll += LogAllLoot;
            On.RoR2.ShrineChanceBehavior.Start += ChanceShenanigans;
            On.RoR2.RouletteChestController.Start += ListItems;

            // Shrines
            On.RoR2.ShrineBloodBehavior.Start += (orig, self) =>
                { orig(self); PopulateLoot(self.gameObject, GenerateShrineData(TokenHelper.BloodEffect)); };
            On.RoR2.ShrineBossBehavior.Start += (orig, self) =>
                { orig(self); PopulateLoot(self.gameObject, GenerateShrineData(TokenHelper.MountainEffect)); };
            On.RoR2.ShrineCombatBehavior.Start += (orig, self) =>
                { orig(self); PopulateLoot(self.gameObject, GenerateShrineData(TokenHelper.CombatEffect)); };
            On.RoR2.ShrineRestackBehavior.Start += (orig, self) =>
                { orig(self); PopulateLoot(self.gameObject, GenerateShrineData(TokenHelper.OrderEffect)); };
            On.RoR2.ShrineHealingBehavior.Awake += (orig, self) =>
                { orig(self); PopulateLoot(self.gameObject, GenerateShrineData(TokenHelper.WoodsEffect)); };

            // Statues
            On.RoR2.PortalStatueBehavior.GrantPortalEntry += (orig, self) =>
            {
                orig(self);
                if (!self.isActiveAndEnabled) return; // TODO: find component that makes them invis
                string token = self.portalType == PortalStatueBehavior.PortalType.Shop ? TokenHelper.NewtEffect : TokenHelper.GoldEffect;
                PopulateLoot(self.gameObject, GenerateShrineData(token));
            };
        }



        private void ListItems(On.RoR2.RouletteChestController.orig_Start orig, RouletteChestController self)
        {
            orig(self);

            var rng = new Xoroshiro128Plus(0) { state0 = self.rng.state0, state1 = self.rng.state1 };

            PickupIndex prev = PickupIndex.none;
            for (int i = 0; i < self.maxEntries; i++)
            {
                PickupIndex next = self.dropTable.GenerateDrop(rng);
                if (next == prev) next = self.dropTable.GenerateDrop(rng); // again if duped


                var data = GenerateItemDataFromPickup(next, true);
                PopulateLoot(self.gameObject, data);

                prev = next;
            }
        }

        private void ChanceShenanigans(On.RoR2.ShrineChanceBehavior.orig_Start orig, ShrineChanceBehavior self)
        {
            const int SHRINE_MAX = 2;
            orig(self);

            var rng = new Xoroshiro128Plus(0) { state0 = self.rng.state0, state1 = self.rng.state1 };
            int succcs = 0;
            while (succcs < SHRINE_MAX)
            {
                if (rng.nextNormalizedFloat > self.failureChance)
                {
                    var pickup = self.dropTable.GenerateDrop(rng);
                    var data = GenerateItemDataFromPickup(pickup);

                    PopulateLoot(self.gameObject, data);
                    succcs++;
                }
                else
                {
                    var data = new ItemData()
                    {
                        isItem = false,
                        nonItem = NonItemType.ShrineEffect,
                        nameToken = TokenHelper.ChanceFailEffect,
                        isKnown = false
                    };

                    PopulateLoot(self.gameObject, data);
                }
            }
        }

        private void LogAllLoot(On.RoR2.OptionChestBehavior.orig_Roll orig, OptionChestBehavior self)
        {
            orig(self);

            // VOID POTENTIALS
            foreach (var itemPickup in self.generatedDrops)
            {
                var data = GenerateItemDataFromPickup(itemPickup);
                PopulateLoot(self.gameObject, data);
            }
        }

        HashSet<ShopTerminalBehavior> delDupes = new();
        private void SetPrinterIndex(On.RoR2.ShopTerminalBehavior.orig_SetPickupIndex orig, ShopTerminalBehavior self, PickupIndex newPickupIndex, bool newHidden)
        {
            bool flag = (self.pickupIndex == newPickupIndex && self.hidden == newHidden) || !delDupes.Add(self);
            orig(self, newPickupIndex, newHidden);

            if (newPickupIndex == PickupIndex.none) return;
            if (flag) return; // DON'T LOG UNCHANGES and THE FIRST OCCURANCE


            // MULTISHOP
            if (self.serverMultiShopController != null)
            {
                var itemData = GenerateItemDataFromPickup(newPickupIndex, !newHidden);
                PopulateLoot(self.serverMultiShopController.gameObject, itemData);
            }
            // PRINTER
            else
            {
                var itemData = GenerateItemDataFromPickup(newPickupIndex, true);
                PopulateLoot(self.gameObject, itemData);
            }
        }
        private void LogChestRoll(On.RoR2.ChestBehavior.orig_Roll orig, ChestBehavior self)
        {
            orig(self);

            if (!populoot.ContainsKey(self.gameObject) || populoot[self.gameObject].loot.Count > 0) return; // no rerolling pls
            PopulateLoot(self.gameObject, GenerateItemDataFromPickup(self.dropPickup));
        }

        private void LoadStart(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);

            loot = null;
        }
        private void LoadStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            orig(self);

            idCounter = self.stageClearCount * 100; // im lazy
            if (loot != null) AppendLoot();

            populoot = new();
            loot = new StageLoot() { stageName = self.nextStageScene.nameToken, stageNum = self.stageClearCount + 1 };
        }

        private void LogMultishopControllers(On.RoR2.MultiShopController.orig_CreateTerminals orig, MultiShopController self)
        {
            orig(self);

            //  multishops logged separately to join the terminals
            string token;

            if (self.terminalGameObjects.Length == 2)
            {
                token = TokenHelper.FallenMultishop;
            }
            else
            {
                // grab an item from the table
                var pickup = self.terminalGameObjects[0].GetComponent<ShopTerminalBehavior>().dropTable.GenerateDrop(new Xoroshiro128Plus(0));
                token = TokenHelper.GetMultishopFromPickup(PickupCatalog.GetPickupDef(pickup));
            }

            var info = MakeUsefulInfoBase(self.gameObject);
            info.interactorName = token;

            loot.stageLoot.Add(info);
            populoot.Add(self.gameObject, info);
        }

        private void LogAllInteractions(On.RoR2.PurchaseInteraction.orig_OnEnable orig, PurchaseInteraction self)
        {
            orig(self);

            if (self.displayNameToken == null) return;

            // blacklisted: multishop terminals themselves
            if (self.displayNameToken == TokenHelper.MultishopTerminal) return;

            var info = MakeUsefulInfoBase(self.gameObject);
            info.interactorName = self.displayNameToken;

            loot.stageLoot.Add(info);
            populoot.Add(self.gameObject, info);
        }

        // HELPER METHODS
        public void PopulateLoot(GameObject key, ItemData item)
        {
            if (!populoot.ContainsKey(key))
            {
                Debug.Log("fuck " + key + " this " + item);
                return;
            }
            populoot[key].loot.Add(item);
        }
        public void AppendLoot() { Info.stageLoots.Add(loot); loot = null; delDupes = new(); }

        public UsefulInfo MakeUsefulInfoBase(GameObject obj)
        {
            var pos = obj.transform.position;
            return new UsefulInfo()
            {
                x = pos.x,
                y = pos.y,
                z = pos.z,
                id = idCounter++
            };
        }

        public ItemData GenerateShrineData(string token)
        {
            return new()
            {
                isItem = false,
                nonItem = NonItemType.ShrineEffect,
                nameToken = token,
                isKnown = true
            };
        }

        public ItemData GenerateItemDataFromPickup(PickupIndex pickup, bool known = false)
        {
            if (pickup == PickupIndex.none)
            {
                return new ItemData()
                {
                    isItem = false,
                    nameToken = TokenHelper.NoPickup,
                    nonItem = NonItemType.Nothing,
                    isKnown = false
                };
            }
            var pickupDef = PickupCatalog.GetPickupDef(pickup);
            var data = new ItemData()
            {
                isItem = pickupDef.itemIndex != ItemIndex.None,
                nameToken = pickupDef.nameToken,
                isKnown = known
            };

            if (data.isItem) data.tier = pickupDef.itemTier;
            else
            {
                if (pickupDef.isLunar && pickupDef.equipmentIndex != EquipmentIndex.None)
                {
                    data.nonItem = NonItemType.LunarEquip;
                }
                else
                {
                    data.nonItem = NonItemType.Equip;
                }
            }

            return data;
        }
    }

}