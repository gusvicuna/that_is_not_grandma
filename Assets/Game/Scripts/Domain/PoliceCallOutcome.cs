namespace Game.Domain
{
    public enum PoliceCallOutcome
    {
        Unavailable,    // the call could not be made at all (wrong day, already called, game over)
        Won,            // the clue was the real evidence
        WrongEvidence,  // wrong clue, trust lost, but the player is still in the game
        TrustLost       // wrong clue and that was the last of the police's patience
    }
}
