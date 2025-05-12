using UnityEngine;

namespace Unity.FPS.Game
{
    public class StatueCleansedEvent : GameEvent
    {
        public int StatueNumber;
    }

    public static partial class Events
    {
        public static StatueCleansedEvent StatueCleansedEvent = new StatueCleansedEvent();
    }
}