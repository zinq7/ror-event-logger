namespace RoRes
{
    public class DeathEvent : RunEvent
    {
        public string killer;
    }

    public class CharacterExistEvent : RunEvent {
        public float health;
    }

}