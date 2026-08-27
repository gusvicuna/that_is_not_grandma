using System;
using System.Collections.Generic;
using Game.Data;
using Game.Domain;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    public class ExchangeController : MonoBehaviour
    {
        public event Action OnExchangeStateChanged;

        [SerializeField] private NpcEngagedEventChannelSO _npcEngaged;
        [SerializeField] private DialogueFinishedEventChannelSO _dialogueFinished;
        [SerializeField] private ClueSharedEventChannelSO _clueShared;
        [SerializeField] private ClueCollectedEventChannelSO _clueCollected;
        [SerializeField] private RoomLeakedEventChannelSO _roomLeaked;

        private readonly ExchangeLog _exchangeLog = new();
        private bool _isExchangeActive;
        private NpcSO _currentNpc;
        private NpcProfile _currentNpcProfile;

        public bool IsExchangeActive => _isExchangeActive;
        public NpcSO CurrentNpc => _currentNpc;
        public IReadOnlyList<RoomId> LeakedRooms => _exchangeLog.LeakedRooms;

        private void OnEnable()
        {
            _npcEngaged.Raised += HandleNpcEngaged;
            _dialogueFinished.Raised += HandleDialogueFinished;
        }

        private void OnDisable()
        {
            _npcEngaged.Raised -= HandleNpcEngaged;
            _dialogueFinished.Raised -= HandleDialogueFinished;
        }

        private void HandleNpcEngaged(NpcSO npc)
        {
            _currentNpc = npc;
            _currentNpcProfile = npc.ToProfile();
        }

        private void HandleDialogueFinished(DialogueSO dialogue)
        {
            if (_currentNpc == null || !dialogue.AllowsClueExchange || !_currentNpc.OffersExchange)
            {
                ForgetCurrentNpc();
                return;
            }
            _isExchangeActive = true;
            OnExchangeStateChanged?.Invoke();
        }

        public bool HasSharedWithCurrentNpc(ClueSO clue)
        {
            return _currentNpc != null && _exchangeLog.HasShared(_currentNpc.Id, clue.Id);
        }

        public ClueSO Share(ClueSO givenClue)
        {
            if (_currentNpc == null || givenClue == null)
            {
                return null;
            }

            ShareResult result = _exchangeLog.Share(_currentNpcProfile, givenClue.Id, givenClue.RoomId);
            if (result.Outcome != ShareOutcome.Accepted)
            {
                Debug.LogWarning($"Clue '{givenClue.Id}' was already shared with '{_currentNpc.Id}'.");
                return null;
            }

            _clueShared.Raise(_currentNpc, givenClue);
            if (result.LeakedNewRoom)
            {
                _roomLeaked.Raise(result.LeakedRoom);
            }
            if (result.ReturnedClueId != null && _currentNpc.TryResolveClue(result.ReturnedClueId, out ClueSO returnedClue))
            {
                _clueCollected.Raise(returnedClue);
                return returnedClue;
            }
            return null;
        }

        public void CloseExchange()
        {
            _isExchangeActive = false;
            ForgetCurrentNpc();
            OnExchangeStateChanged?.Invoke();
        }

        private void ForgetCurrentNpc()
        {
            _currentNpc = null;
            _currentNpcProfile = null;
        }
    }
}
