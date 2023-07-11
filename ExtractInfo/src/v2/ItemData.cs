using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

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
        public ItemTier tier;
        public string itemName;
    }


}
