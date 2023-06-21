namespace RoRes
{
    public class BossSpawnEvent : RunEvent
    {
        public string boss;
        public int mountains;
    }

    public class BossKillEvent : RunEvent
    {
        public string boss;
    }

    public class MithrixPhaseStart : RunEvent
    {
        public int phase;
    }

    public class MithrixPhaseEnd : RunEvent
    {
        public int phase;
    }

    public class PizzaEnterEvent : RunEvent
    {
        public float mithrixHealth;
    }

    public class PizzaExitEvent : RunEvent
    {
        public float mithrixHealth;
    }

}