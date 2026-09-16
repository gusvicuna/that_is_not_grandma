namespace Game.Domain
{
    public readonly struct ShareResult
    {
        public ShareOutcome Outcome { get; }
        public string ReturnedClueId { get; }
        public bool LeakedNewRoom { get; }
        public RoomId LeakedRoom { get; }

        public ShareResult(ShareOutcome outcome, string returnedClueId, bool leakedNewRoom, RoomId leakedRoom)
        {
            Outcome = outcome;
            ReturnedClueId = returnedClueId;
            LeakedNewRoom = leakedNewRoom;
            LeakedRoom = leakedRoom;
        }
    }
}
