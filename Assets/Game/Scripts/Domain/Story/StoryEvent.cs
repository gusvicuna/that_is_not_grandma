namespace Game.Domain
{
    /// <summary>
    /// Something that happened in the game, flattened to ids so the Domain never sees a
    /// ScriptableObject. Presentation fills it in from the channel payload.
    /// </summary>
    public readonly struct StoryEvent
    {
        public StoryEvent(StoryTrigger trigger, string primaryId = null, string secondaryId = null, int number = 0)
        {
            Trigger = trigger;
            PrimaryId = primaryId;
            SecondaryId = secondaryId;
            Number = number;
        }

        public StoryTrigger Trigger { get; }

        /// <summary>Clue id, item id or dialogue id, depending on the trigger.</summary>
        public string PrimaryId { get; }

        /// <summary>Npc id — only <see cref="StoryTrigger.ClueShared"/> uses it.</summary>
        public string SecondaryId { get; }

        /// <summary>Room (as int), day number, or police outcome (as int), depending on the trigger.</summary>
        public int Number { get; }
    }
}
