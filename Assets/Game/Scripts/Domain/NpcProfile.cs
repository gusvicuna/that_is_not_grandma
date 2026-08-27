using System;

namespace Game.Domain
{
    public class NpcProfile
    {
        public string Id { get; }
        public bool LeaksToNotGrandma { get; }
        public ExchangeTable Exchanges { get; }

        public NpcProfile(string id, bool leaksToNotGrandma, ExchangeTable exchanges)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Npc id cannot be null, empty or whitespace.", nameof(id));
            }
            Id = id;
            LeaksToNotGrandma = leaksToNotGrandma;
            Exchanges = exchanges ?? throw new ArgumentNullException(nameof(exchanges));
        }
    }
}
