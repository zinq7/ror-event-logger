using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtractInfo.src.v2
{
    public class StageLoot
    {
        public string stageName = "buff";
        public int stageNum = -1;
        public List<UsefulInfo> stageLoot = new();
    }

    public class UsefulInfo
    {
        public float x, y;
        public ItemTier tier;
        public string pickupToken;
        public string sourceToken;
        public UsefulInfo(float x, float y, ItemTier tier, string pickupToken, string sourceToken)
        {
            this.x = x;
            this.y = y;
            this.tier = tier;
            this.pickupToken = pickupToken;
            this.sourceToken = sourceToken;
        }
    }

}
