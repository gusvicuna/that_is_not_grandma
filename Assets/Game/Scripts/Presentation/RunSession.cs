using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// The one thing that outlives a run. Reloading the scene wipes every component, so the
    /// "play again" button leaves its note here instead.
    ///
    /// It is static on purpose and it is the only static state in the game: an event channel
    /// cannot carry it, because the object that would listen does not exist yet when the scene
    /// starts loading. Nothing else may be added here without a very good reason — a second flag
    /// is usually a sign that something belongs in the story director's flags instead.
    /// </summary>
    public static class RunSession
    {
        /// <summary>
        /// Set by the end screen before it reloads the scene. The story director puts the house in
        /// its post-intro state and the day starts at once, so a replay drops the player straight
        /// into the search.
        /// </summary>
        public static bool SkipIntro { get; set; }

        /// <summary>
        /// Statics survive "Enter Play Mode" when domain reload is disabled, which would silently
        /// skip the intro the next time the editor hits Play. Cleared before every run instead.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            SkipIntro = false;
        }
    }
}
