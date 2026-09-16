using Game.Domain;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// One authored story beat. The match fields are asset references rather than typed ids, so
    /// renaming a clue can never silently break the story.
    /// </summary>
    [CreateAssetMenu(fileName = "Beat", menuName = "Game/Story/Beat")]
    public class StoryBeatSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique across the whole story, e.g. beat_phone_appears.")]
        [SerializeField] private string _id;
        public string Id => _id;

        [SerializeField] private StoryTrigger _trigger;

        [Header("Match (leave empty for 'any')")]
        [Tooltip("ClueCollected / ClueShared: which clue. Empty = any clue.")]
        [SerializeField] private ClueSO _matchClue;

        [Tooltip("ItemInspected: which item. Empty = any item.")]
        [SerializeField] private ItemSO _matchItem;

        [Tooltip("DialogueFinished: which conversation. Empty = any conversation.")]
        [SerializeField] private DialogueSO _matchDialogue;

        [Tooltip("ClueShared: which NPC received the clue. Empty = any NPC.")]
        [SerializeField] private NpcSO _matchNpc;

        [Tooltip("RoomEntered: which room.")]
        [SerializeField] private RoomId _matchRoom;

        [Tooltip("RoomEntered: tick to fire on entering any room.")]
        [SerializeField] private bool _matchAnyRoom;

        [Tooltip("DayStarted: which day. 0 = any day.")]
        [SerializeField] private int _matchDay;

        [Tooltip("PoliceCallResolved: which outcome.")]
        [SerializeField] private PoliceCallOutcome _matchOutcome;

        [Tooltip("PoliceCallResolved: tick to fire on any outcome.")]
        [SerializeField] private bool _matchAnyOutcome;

        [Header("Rules")]
        [Tooltip("Off = fires at most once per run.")]
        [SerializeField] private bool _repeatable;

        [SerializeField] private StoryConditionData _condition;

        [Header("Effects")]
        [SerializeField] private StoryEffectData[] _effects;
        public StoryEffectData[] Effects => _effects;

        public StoryBeat ToBeat()
        {
            return new StoryBeat(
                _id,
                _trigger,
                MatchPrimaryId(),
                MatchSecondaryId(),
                MatchNumber(),
                _condition != null ? _condition.ToCondition() : StoryCondition.Always,
                _repeatable);
        }

        private string MatchPrimaryId()
        {
            switch (_trigger)
            {
                case StoryTrigger.ClueCollected:
                case StoryTrigger.ClueShared:
                    return _matchClue != null ? _matchClue.Id : null;
                case StoryTrigger.ItemInspected:
                    return _matchItem != null ? _matchItem.Id : null;
                case StoryTrigger.DialogueFinished:
                    return _matchDialogue != null ? _matchDialogue.Id : null;
                default:
                    return null;
            }
        }

        private string MatchSecondaryId()
        {
            if (_trigger != StoryTrigger.ClueShared)
            {
                return null;
            }
            return _matchNpc != null ? _matchNpc.Id : null;
        }

        private int MatchNumber()
        {
            switch (_trigger)
            {
                case StoryTrigger.RoomEntered:
                    return _matchAnyRoom ? StoryBeat.AnyNumber : (int)_matchRoom;
                case StoryTrigger.DayStarted:
                    return _matchDay > 0 ? _matchDay : StoryBeat.AnyNumber;
                case StoryTrigger.PoliceCallResolved:
                    return _matchAnyOutcome ? StoryBeat.AnyNumber : (int)_matchOutcome;
                default:
                    return StoryBeat.AnyNumber;
            }
        }
    }
}
