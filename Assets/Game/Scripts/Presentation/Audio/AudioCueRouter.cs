using UnityEngine;
using Game.Data;
using Game.Domain;
using Game.Events;

namespace Game.Presentation
{
    /// <summary>
    /// The only component that knows gameplay events make sound, so adding one later means dropping
    /// a cue asset in a slot instead of editing that feature.
    ///
    /// RoomLeaked is deliberately NOT wired: the leak only fires for the Uncle, so any sound on it
    /// would name the traitor on the player's first trade. The lie motif is authored per
    /// conversation instead, through DialogueSO.PlaysLieMotif.
    /// </summary>
    public class AudioCueRouter : MonoBehaviour
    {
        [SerializeField] private AudioCueEventChannelSO _sfxRequestedChannel;
        [SerializeField] private MusicDirector _musicDirector;

        [Header("Listened channels")]
        [SerializeField] private ItemEventChannelSO _itemInspectedChannel;
        [SerializeField] private ClueEventChannelSO _clueCollectedChannel;
        [SerializeField] private NpcClueEventChannelSO _clueSharedChannel;
        [SerializeField] private RoomIdEventChannelSO _roomChangedChannel;
        [SerializeField] private TensionLevelEventChannelSO _tensionChangedChannel;
        [SerializeField] private DialogueEventChannelSO _dialogueRequestedChannel;
        [SerializeField] private VoidEventChannelSO _nightStartedChannel;
        [SerializeField] private BoolEventChannelSO _nightResolvedChannel;
        [SerializeField] private IntEventChannelSO _dayStartedChannel;

        [Header("Cues (all optional)")]
        [SerializeField] private AudioCueSO _interactCue;
        [SerializeField] private AudioCueSO _clueCollectedCue;
        [SerializeField] private AudioCueSO _clueSharedCue;
        [SerializeField] private AudioCueSO _roomChangeCue;
        [SerializeField] private AudioCueSO _alertCue;
        [SerializeField] private AudioCueSO _hideCue;
        [SerializeField] private AudioCueSO _nightSurvivedCue;
        [SerializeField] private AudioCueSO _caughtCue;
        [SerializeField] private AudioCueSO _morningCue;

        private TensionLevel _previousTension = TensionLevel.Calm;

        private void OnEnable()
        {
            _itemInspectedChannel.Raised += OnItemInspected;
            _clueCollectedChannel.Raised += OnClueCollected;
            _clueSharedChannel.Raised += OnClueShared;
            _roomChangedChannel.Raised += OnRoomChanged;
            _tensionChangedChannel.Raised += OnTensionChanged;
            _dialogueRequestedChannel.Raised += OnDialogueRequested;
            _nightStartedChannel.Raised += OnNightStarted;
            _nightResolvedChannel.Raised += OnNightResolved;
            _dayStartedChannel.Raised += OnDayStarted;
        }

        private void OnDisable()
        {
            _itemInspectedChannel.Raised -= OnItemInspected;
            _clueCollectedChannel.Raised -= OnClueCollected;
            _clueSharedChannel.Raised -= OnClueShared;
            _roomChangedChannel.Raised -= OnRoomChanged;
            _tensionChangedChannel.Raised -= OnTensionChanged;
            _dialogueRequestedChannel.Raised -= OnDialogueRequested;
            _nightStartedChannel.Raised -= OnNightStarted;
            _nightResolvedChannel.Raised -= OnNightResolved;
            _dayStartedChannel.Raised -= OnDayStarted;
        }

        private void OnItemInspected(ItemSO item)
        {
            RequestCue(_interactCue);
        }

        private void OnClueCollected(ClueSO clue)
        {
            RequestCue(_clueCollectedCue);
        }

        private void OnClueShared(NpcSO npc, ClueSO clue)
        {
            // The same sound for every NPC. A different one for the leaker would give him away.
            RequestCue(_clueSharedCue);
        }

        private void OnRoomChanged(RoomId room)
        {
            RequestCue(_roomChangeCue);
        }

        private void OnTensionChanged(TensionLevel level)
        {
            // Rising edge only: a sting that retriggers while already Alert is noise.
            if (level > _previousTension)
            {
                RequestCue(_alertCue);
            }
            _previousTension = level;
        }

        private void OnDialogueRequested(DialogueSO dialogue)
        {
            if (dialogue.PlaysLieMotif)
            {
                _musicDirector.PulseLieMotif();
            }
        }

        private void OnNightStarted()
        {
            RequestCue(_hideCue);
        }

        private void OnNightResolved(bool survived)
        {
            RequestCue(survived ? _nightSurvivedCue : _caughtCue);
        }

        private void OnDayStarted(int dayNumber)
        {
            _previousTension = TensionLevel.Calm;
            RequestCue(_morningCue);
        }

        private void RequestCue(AudioCueSO cue)
        {
            if (cue != null)
            {
                _sfxRequestedChannel.Raise(cue);
            }
        }
    }
}
