using System;
using Game.Data;
using Game.Domain;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// The day → night → day loop. Owns the DayClock, charges the actions that cost time, and is
    /// the only place that raises CH_DayStarted — the police call reads the day from that channel,
    /// so nothing else may ever count days.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Balance (Gus's knobs)")]
        [SerializeField] private float _secondsPerDay = 180f;
        [SerializeField] private float _talkCost = 8f;
        [SerializeField] private float _shareCost = 12f;
        [SerializeField] private float _clueCost = 5f;

        [Tooltip("An action's cost never leaves less than this on the clock, so you cannot lose the night in the instant a conversation ends. Only time actually passing runs the day out.")]
        [SerializeField] private float _minimumSecondsAfterAction = 5f;

        [Tooltip("Reading is free: the clock stops while a conversation is on screen and the cost is charged when it ends.")]
        [SerializeField] private bool _pauseWhileDialogueOpen = true;

        [SerializeField] private bool _startDayOnStart = true;

        [Tooltip("Optional. With this set — and Start Day On Start off — the first day begins when " +
                 "this conversation ends instead of on load, so the intro plays on a stopped clock. " +
                 "Point it at the last line of the intro.")]
        [SerializeField] private DialogueSO _startDayAfterDialogue;

        [Header("Panels that also stop the clock (optional)")]
        [Tooltip("The share panel opens after a conversation ends, so the dialogue pause has already been lifted by then.")]
        [SerializeField] private ExchangeController _exchangeController;
        [Tooltip("Reading a clue's text is reading, same as a conversation.")]
        [SerializeField] private ItemInspectPopup _itemInspectPopup;

        [Header("Channels")]
        [SerializeField] private IntEventChannelSO _dayStarted;
        [SerializeField] private VoidEventChannelSO _nightStarted;
        [SerializeField] private BoolEventChannelSO _nightResolved;
        [Tooltip("Raised by NightSequenceView once the screen has faded back in. Leave empty and the next day starts the instant the night resolves — correct only in a scene with no night sequence.")]
        [SerializeField] private VoidEventChannelSO _nightSequenceFinished;
        [SerializeField] private DialogueEventChannelSO _dialogueRequested;
        [SerializeField] private DialogueEventChannelSO _dialogueFinished;
        [SerializeField] private NpcClueEventChannelSO _clueShared;
        [SerializeField] private ClueEventChannelSO _clueCollected;

        /// <summary>Raised whenever the remaining time changes. A clock widget listens to this.</summary>
        public event Action OnClockChanged;

        private DayClock _clock;
        private bool _dayRunning;
        private bool _paused;
        private bool _morningPending;

        public int CurrentDay { get; private set; }

        public float NormalizedRemaining => _clock?.NormalizedRemaining ?? 1f;

        public bool IsDayRunning => _dayRunning;

        private void Awake()
        {
            _clock = new DayClock(_secondsPerDay, _minimumSecondsAfterAction);
            Wiring.Require(this, _dayStarted, nameof(_dayStarted));
            Wiring.Require(this, _nightStarted, nameof(_nightStarted));
            Wiring.Require(this, _nightResolved, nameof(_nightResolved));
        }

        /// <summary>
        /// Decision 4 of plan 04: reading is free. The dialogue pause is lifted when the
        /// conversation ends, which is exactly when the share panel opens — so those panels have to
        /// be asked directly, not inferred from the dialogue channel.
        /// </summary>
        private bool IsReadingPanelOpen
        {
            get
            {
                if (_exchangeController != null && _exchangeController.IsExchangeActive)
                {
                    return true;
                }
                return _itemInspectPopup != null && _itemInspectPopup.IsOpen;
            }
        }

        private void OnEnable()
        {
            if (_nightResolved != null) _nightResolved.Raised += OnNightResolved;
            if (_nightSequenceFinished != null) _nightSequenceFinished.Raised += OnNightSequenceFinished;
            if (_dialogueRequested != null) _dialogueRequested.Raised += OnDialogueRequested;
            if (_dialogueFinished != null) _dialogueFinished.Raised += OnDialogueFinished;
            if (_clueShared != null) _clueShared.Raised += OnClueShared;
            if (_clueCollected != null) _clueCollected.Raised += OnClueCollected;
        }

        private void OnDisable()
        {
            if (_nightResolved != null) _nightResolved.Raised -= OnNightResolved;
            if (_nightSequenceFinished != null) _nightSequenceFinished.Raised -= OnNightSequenceFinished;
            if (_dialogueRequested != null) _dialogueRequested.Raised -= OnDialogueRequested;
            if (_dialogueFinished != null) _dialogueFinished.Raised -= OnDialogueFinished;
            if (_clueShared != null) _clueShared.Raised -= OnClueShared;
            if (_clueCollected != null) _clueCollected.Raised -= OnClueCollected;
        }

        private void Start()
        {
            // A replay has no intro to wait for, so the run begins the moment the scene loads.
            if (!_startDayOnStart && RunSession.SkipIntro)
            {
                StartDay(1);
                return;
            }
            if (!_startDayOnStart)
            {
                return;
            }
            if (_startDayAfterDialogue != null)
            {
                Debug.LogWarning(
                    $"DayNightCycle on '{name}': Start Day On Start is on, so the day begins now and " +
                    $"'{_startDayAfterDialogue.name}' will never start it. Turn one of the two off.",
                    this);
            }
            StartDay(1);
        }

        private void Update()
        {
            if (!_dayRunning || _paused || IsReadingPanelOpen)
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
            if (_nightSequenceFinished != null)
            {
                // The night sequence is on screen. Morning waits for it, so the player does not
                // spend the first seconds of the new day looking at a black screen.
                _morningPending = true;
                return;
            }
            StartDay(CurrentDay + 1);
        }

        private void OnNightSequenceFinished()
        {
            if (!_morningPending)
            {
                return;
            }
            _morningPending = false;
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

            // The last line of the intro starts the run. No talk cost for it: the conversation
            // happened before the clock existed, and StartDay resets it to a full day anyway.
            // CurrentDay guards against a replay of that dialogue rewinding the run to day 1.
            if (_startDayAfterDialogue != null && dialogue == _startDayAfterDialogue && CurrentDay == 0)
            {
                StartDay(1);
                return;
            }

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
