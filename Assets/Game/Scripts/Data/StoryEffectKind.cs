namespace Game.Data
{
    /// <summary>What a beat does when it fires. Applied by StorySceneBinder, never by the Domain.</summary>
    public enum StoryEffectKind
    {
        ShowActor,        // actor id
        HideActor,        // actor id
        MoveActor,        // actor id + room
        SetNpcDialogue,   // npc + dialogue: what the NPC plays on the next click
        PlayDialogue,     // dialogue: queued, played once no panel is open
        SetTension,       // tension level: raises CH_TensionChanged
        SetFlag           // flag name: story state for later conditions
    }
}
