using RoR2;

namespace RoRes
{
    public class InventoryEvent : RunEvent
    {
        public Transaction transactionType;
        public Item item;
        public int quantity; // -3 would be losing 3
    }

    public class Item
    {
        public string englishName;
        public ItemTier tier;
    }

    public class Equipment : Item
    {
        // risk of rain 2
    }
    public enum Transaction
    {
        Pickup,
        Scrap,
        Print,
        Shorder,
        Death, // Plus5 and Dios and the Ploripotent item
        Corrupted,
        Bloom,
        Ego,
        Consumed
    }
}