namespace Game.Domain
{
    /// <summary>
    /// What can start a story beat. Each value maps to exactly one event channel, listened to by
    /// StoryDirectorBehaviour. Adding a value here means adding a channel subscription there.
    /// </summary>
    public enum StoryTrigger
    {
        ClueCollected,      // CH_ClueCollected
        ItemInspected,      // CH_ItemInspected
        DialogueFinished,   // CH_DialogueFinished
        ClueShared,         // CH_ClueShared  (clue + npc)
        RoomEntered,        // CH_RoomChanged
        DayStarted,         // CH_DayStarted
        PoliceCallResolved  // CH_PoliceCallResolved
    }
}
