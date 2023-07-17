using RoR2;
using System.Collections.Generic;

namespace RoRes
{
    public class StageLoot
    {
        public string stageName = "buff";
        public int stageNum = -1;
        public List<UsefulInfo> stageLoot = new();
    }

    public class UsefulInfo
    {
        public float x, y, z;
        public bool itemsJoined;
        public List<ItemData> items;
        public string interactorName;
        public int id;
    }

    public class ItemData {
        public bool isItem, isKnown;
        public ItemTier? tier;
        public NonItemType? nonItem;
        public string nameToken;
    }

    public enum NonItemType
    {
        Nothing, 
        Equip,
        LunarEquip,
        Drone,
        ShrineEffect
    }


}
