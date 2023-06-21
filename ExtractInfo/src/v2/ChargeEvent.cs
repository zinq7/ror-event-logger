namespace RoRes
{
    public class ChargeStartEvent : RunEvent {
        public ChargeType chargeType;
    }

    public class ChargeEndEvent : RunEvent {
        public ChargeType chargeType;
    }

    public class PillarStartEvent : ChargeStartEvent
    {
        public Pillar pillarType;
    }

    public enum Pillar
    {
        blood,
        soul,
        design,
        mass
    }

    public enum ChargeType
    {
        vent,
        teleporter,
        pillar,
        voidgrovething
    }

}