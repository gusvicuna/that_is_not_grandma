using System.Collections.Generic;
using Game.Data;
using Game.Domain;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Owns the StoryDirector and translates channels into story events. Every rule lives in the
    /// Domain; every scene change lives in the binder. This is only wiring.
    /// </summary>
    public class StoryDirectorBehaviour : MonoBehaviour
    {
        [Header("Story")]
        [Tooltip("Evaluation order is this order — the first beat in the list wins ties.")]
        [SerializeField] private StoryBeatSO[] _beats;
        [SerializeField] private StorySceneBinder _binder;

        [Header("Replay")]
        [Tooltip("Applied in Awake when the player restarts from the end screen: the effects that " +
                 "leave the house exactly as the intro leaves it — actors hidden or shown, NPC " +
                 "dialogues bound, and the flags that keep the intro beats from firing again. Its " +
                 "trigger and match fields are ignored; only its effects are used.")]
        [SerializeField] private StoryBeatSO _skipIntroBeat;

        [Header("Trigger channels")]
        [SerializeField] private ClueEventChannelSO _clueCollected;
        [SerializeField] private ItemEventChannelSO _itemInspected;
        [SerializeField] private DialogueEventChannelSO _dialogueFinished;
        [SerializeField] private NpcClueEventChannelSO _clueShared;
        [SerializeField] private RoomIdEventChannelSO _roomChanged;
        [SerializeField] private IntEventChannelSO _dayStarted;
        [Tooltip("Optional until plan 06 (police call) merges.")]
        [SerializeField] private PoliceCallOutcomeEventChannelSO _policeCallResolved;

        private readonly Dictionary<string, StoryBeatSO> _beatsById = new();
        private StoryDirector _director;

        private void Awake()
        {
            if (!Wiring.Require(this, _binder, nameof(_binder)))
            {
                enabled = false;
                return;
            }

            var beats = new List<StoryBeat>();
            foreach (StoryBeatSO beatSo in _beats)
            {
                if (beatSo == null)
                {
                    continue;
                }
                beats.Add(beatSo.ToBeat());
                _beatsById[beatSo.Id] = beatSo;
            }
            _director = new StoryDirector(beats);
            _binder.Bind(_director);

            // In Awake on purpose: the flags this sets have to be in place before the first
            // channel is raised in Start, or the room announce would replay an intro beat.
            if (RunSession.SkipIntro && _skipIntroBeat != null)
            {
                _binder.Apply(_skipIntroBeat);
            }
        }

        private void OnEnable()
        {
            if (_clueCollected != null) _clueCollected.Raised += OnClueCollected;
            if (_itemInspected != null) _itemInspected.Raised += OnItemInspected;
            if (_dialogueFinished != null) _dialogueFinished.Raised += OnDialogueFinished;
            if (_clueShared != null) _clueShared.Raised += OnClueShared;
            if (_roomChanged != null) _roomChanged.Raised += OnRoomChanged;
            if (_dayStarted != null) _dayStarted.Raised += OnDayStarted;
            if (_policeCallResolved != null) _policeCallResolved.Raised += OnPoliceCallResolved;
        }

        private void OnDisable()
        {
            if (_clueCollected != null) _clueCollected.Raised -= OnClueCollected;
            if (_itemInspected != null) _itemInspected.Raised -= OnItemInspected;
            if (_dialogueFinished != null) _dialogueFinished.Raised -= OnDialogueFinished;
            if (_clueShared != null) _clueShared.Raised -= OnClueShared;
            if (_roomChanged != null) _roomChanged.Raised -= OnRoomChanged;
            if (_dayStarted != null) _dayStarted.Raised -= OnDayStarted;
            if (_policeCallResolved != null) _policeCallResolved.Raised -= OnPoliceCallResolved;
        }

        private void OnClueCollected(ClueSO clue)
        {
            Dispatch(new StoryEvent(StoryTrigger.ClueCollected, clue.Id));
        }

        private void OnItemInspected(ItemSO item)
        {
            Dispatch(new StoryEvent(StoryTrigger.ItemInspected, item.Id));
        }

        private void OnDialogueFinished(DialogueSO dialogue)
        {
            Dispatch(new StoryEvent(StoryTrigger.DialogueFinished, dialogue.Id));
        }

        private void OnClueShared(NpcSO npc, ClueSO clue)
        {
            Dispatch(new StoryEvent(StoryTrigger.ClueShared, clue.Id, npc.Id));
        }

        private void OnRoomChanged(RoomId room)
        {
            Dispatch(new StoryEvent(StoryTrigger.RoomEntered, number: (int)room));
        }

        private void OnDayStarted(int day)
        {
            Dispatch(new StoryEvent(StoryTrigger.DayStarted, number: day));
        }

        private void OnPoliceCallResolved(PoliceCallOutcome outcome)
        {
            Dispatch(new StoryEvent(StoryTrigger.PoliceCallResolved, number: (int)outcome));
        }

        private void Dispatch(StoryEvent storyEvent)
        {
            IReadOnlyList<string> firedBeatIds = _director.Notify(storyEvent);
            for (int i = 0; i < firedBeatIds.Count; i++)
            {
                if (_beatsById.TryGetValue(firedBeatIds[i], out StoryBeatSO beat))
                {
                    _binder.Apply(beat);
                }
            }
        }
    }
}
