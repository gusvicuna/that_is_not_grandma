using System;
using Game.Data;
using Game.Domain;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// The day → night → day loop. Owns the DayClock, charges actions, and is the only place that
    /// raises CH_DayStarted — plan 06's DebugDayAdvancer dies the day this lands in the scene.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Balance (Gus's knobs)")]
        [SerializeField] private float _secondsPerDay = 180f;
        [SerializeField] private float _talkCost = 8f;
        [SerializeField] private float _shareCost = 12f;
        [SerializeField] private float _clueCost = 5f;

        [Tooltip("Reading is free: the clock stops while a conversation is on screen and the cost is charged when it ends.")]
        [SerializeField] private bool _pauseWhileDialogueOpen = true;

        [SerializeField] private bool _startDayOnStart = true;

        [Header("Channels")]
        [SerializeField] private IntEventChannelSO _dayStarted;
        [SerializeField] private VoidEventChannelSO _nightStarted;
        [SerializeField] private BoolEventChannelSO _nightResolved;
        [SerializeField] private DialogueEventChannelSO _dialogueRequested;
        [SerializeField] private DialogueEventChannelSO _dialogueFinished;
        [SerializeField] private NpcClueEventChannelSO _clueShared;
        [SerializeField] private ClueEventChannelSO _clueCollected;

        /// <summary>Raised whenever the remaining time changes. A clock widget listens to this.</summary>
        public event Action OnClockChanged;

        private DayClock _clock;
        private bool _dayRunning;
        private bool _paused;

        public int CurrentDay { get; private set; }

        public float NormalizedRemaining => _clock?.NormalizedRemaining ?? 1f;

        public bool IsDayRunning => _dayRunning;

        private void Awake()
        {
            _clock = new DayClock(_secondsPerDay);
        }

        private void OnEnable()
        {
            if (_nightResolved != null) _nightResolved.Raised += OnNightResolved;
            if (_dialogueRequested != null) _dialogueRequested.Raised += OnDialogueRequested;
            if (_dialogueFinished != null) _dialogueFinished.Raised += OnDialogueFinished;
            if (_clueShared != null) _clueShared.Raised += OnClueShared;
            if (_clueCollected != null) _clueCollected.Raised += OnClueCollected;
        }

        private void OnDisable()
        {
            if (_nightResolved != null) _nightResolved.Raised -= OnNightResolved;
            if (_dialogueRequested != null) _dialogueRequested.Raised -= OnDialogueRequested;
            if (_dialogueFinished != null) _dialogueFinished.Raised -= OnDialogueFinished;
            if (_clueShared != null) _clueShared.Raised -= OnClueShared;
            if (_clueCollected != null) _clueCollected.Raised -= OnClueCollected;
        }

        private void Start()
        {
            if (_startDayOnStart)
            {
                StartDay(1);
            }
        }

        private void Update()
        {
            if (!_dayRunning || _paused)
            {
                return;
            }
            _clock.Tick(Time.deltaTime);
            OnClockChanged?.Invoke();
            if (_clock.IsExpired)
            {
                EndDayNow();
            }
        }

        public void StartDay(int day)
        {
            CurrentDay = day;
            _clock.ResetForNewDay();
            _dayRunning = true;
            _paused = false;
            OnClockChanged?.Invoke();
            _dayStarted.Raise(day);
        }

        /// <summary>Nightfall, whether the clock ran out or the player chose to hide.</summary>
        public void EndDayNow()
        {
            if (!_dayRunning)
            {
                return;
            }
            _dayRunning = false;
            _paused = false;
            _nightStarted.Raise();
        }

        private void OnNightResolved(bool survived)
        {
            if (!survived)
            {
                return; // the run is over; the end screen takes it from here
            }
            StartDay(CurrentDay + 1);
        }

        private void OnDialogueRequested(DialogueSO dialogue)
        {
            if (_pauseWhileDialogueOpen)
            {
                _paused = true;
            }
        }

        private void OnDialogueFinished(DialogueSO dialogue)
        {
            _paused = false;
            Charge(_talkCost);
        }

        private void OnClueShared(NpcSO npc, ClueSO clue)
        {
            Charge(_shareCost);
        }

        private void OnClueCollected(ClueSO clue)
        {
            Charge(_clueCost);
        }

        private void Charge(float cost)
        {
            if (!_dayRunning)
            {
                return;
            }
            _clock.Spend(cost);
            OnClockChanged?.Invoke();
            if (_clock.IsExpired)
            {
                EndDayNow();
            }
        }
    }
}
