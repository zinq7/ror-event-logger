using RoR2;

namespace RoRGauntlet
{
    public class TokenHelper
    {
        // ----------------------------------------- REAL TOKENS -----------------------------------------
        public const string MithrixBody = "BROTHER_BODY_NAME";
        public const string MultishopTerminal = "MULTISHOP_TERMINAL_NAME";


        // ----------------------------------------- FAKE TOKENS -----------------------------------------
        // multishops
        public const string WhiteMultishop = "MULTISHOP_WHITE_NAME";
        public const string GreenMultishop = "MULTISHOP_GREEN_NAME";
        public const string EquipMultishop = "MULTISHOP_ORANGE_NAME";
        public const string FallenMultishop = "MULTISHOP_SHORM_NAME";
        public static string GetMultishopFromPickup(PickupDef pickup)
        {
            if (pickup.equipmentIndex != EquipmentIndex.None) return EquipMultishop;

            return pickup.itemTier switch
            {
                ItemTier.Tier1 => WhiteMultishop,
                ItemTier.Tier2 => GreenMultishop,
                _ => "IDFK_WHAT_THIS_MULTISHOP_IS"
            };
        }

        // shrine effects
        public const string MountainEffect = "MOUNTAIN_EFFECT_NAME";
        public const string OrderEffect = "ORDER_EFFECT_NAME";
        public const string WoodsEffect = "WOODS_EFFECT_NAME";
        public const string CombatEffect = "COMBAT_EFFECT_NAME";
        public const string BloodEffect = "BLOOD_EFFECT_NAME";
        public const string ChanceFailEffect = "CHANCE_FAIL_EFFECT_NAME"; // FOR LOGGING FAILS
        public const string GoldEffect = "GOLD_EFFECT_NAME"; // ORB EFFECTS
        public const string NewtEffect = "BLUE_EFFECT_NAME"; // ORB EFFECTS

        // misc tokens
        public const string NoPickup = "NOTHING_PICKUP_NAME";
        public const string TeleporterToken = "TELEPORTER_NAME";

    }
}
