using System.Collections.Generic;
namespace RoRes
{
    public class StageStartEvent : RunEvent
    {
        public List<string> enemies;
        public string englishName;
        public int stageNum;
    }

    public class StageEndEvent : RunEvent
    {
        public string englishName;
        public int stageNum;
    }

    public class MiscEvent : RunEvent
    {
        public string eventInfo;
    }

    public class FamilyEventEvent : RunEvent
    {
        public string familyBodyName;
    }
}