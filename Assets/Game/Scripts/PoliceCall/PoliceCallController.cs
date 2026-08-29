using System;
using UnityEngine;
using Game.Domain;
using Game.Data;
using Game.Events;

namespace Game.Presentation
{
    public class PoliceCallController : MonoBehaviour
    {
        [SerializeField] private DialogueSO _policeIntroDialogue;
        [SerializeField] private DialogueSO _phoneUnavailableDialogue;

        [SerializeField] private DayStartedEventChannelSO _dayStarted;
        [SerializeField] private DialogueRequestedEventChannelSO _dialogueRequested;
        [SerializeField] private DialogueFinishedEventChannelSO _dialogueFinished;
        [SerializeField] private GameWonEventChannelSO _gameWon;
        [SerializeField] private GameLostEventChannelSO _gameLost;

        [SerializeField] private int _startingTrust = 2;
        [SerializeField] private int _firstAvailableDay = 2;

        public event Action OnAvailabilityChanged;
        public event Action OnCallPanelStateChanged;

        private PoliceCase _case;
        private bool _waitingForIntro;
        private bool _isCallPanelActive;

        public bool IsPhoneAvailable => _case != null && _case.IsPhoneAvailable;
        public bool IsCallPanelActive => _isCallPanelActive;
        public int TrustRemaining => _case != null ? _case.TrustRemaining : 0;

        private void Awake()
        {
            _case = new PoliceCase(_startingTrust, _firstAvailableDay);
        }

        private void OnEnable()
        {
            _dayStarted.Raised += OnDayStarted;
            _dialogueFinished.Raised += OnDialogueFinished;
        }

        private void OnDisable()
        {
            _dayStarted.Raised -= OnDayStarted;
            _dialogueFinished.Raised -= OnDialogueFinished;
        }

        private void OnDayStarted(int day)
        {
            _case.StartDay(day);
            OnAvailabilityChanged?.Invoke();
        }

        public void RequestCall()
        {
            if (_case.CanCall)
            {
                _waitingForIntro = true;
                _dialogueRequested.Raise(_policeIntroDialogue);
            }
            else
            {
                _dialogueRequested.Raise(_phoneUnavailableDialogue);
            }
        }

        private void OnDialogueFinished(DialogueSO dialogue)
        {
            if (!_waitingForIntro || dialogue != _policeIntroDialogue)
                return;

            _waitingForIntro = false;
            _isCallPanelActive = true;
            OnCallPanelStateChanged?.Invoke();
        }

        public PoliceCallOutcome SubmitEvidence(ClueSO clue)
        {
            PoliceCallOutcome outcome = _case.Call(clue.IsEvidence);

            OnAvailabilityChanged?.Invoke();

            if (outcome == PoliceCallOutcome.Won)
            {
                _gameWon.Raise();
            }
            else if (outcome == PoliceCallOutcome.TrustLost)
            {
                _gameLost.Raise(LossReason.PoliceTrustLost);
            }

            return outcome;
        }

        public void CloseCall()
        {
            _isCallPanelActive = false;
            OnCallPanelStateChanged?.Invoke();
        }
    }
}
