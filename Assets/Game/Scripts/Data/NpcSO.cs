using System.Collections.Generic;
using Game.Domain;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Npc", menuName = "Game/Data/Npc")]
    public class NpcSO : ScriptableObject
    {
        [SerializeField] private string _id;
        public string Id => _id;

        [SerializeField] private string _displayName;
        public string DisplayName => _displayName;

        [SerializeField] private Color _color;
        public Color Color => _color;

        [SerializeField] private Sprite _portrait;
        public Sprite Portrait => _portrait;

        [SerializeField] private Sprite _worldSprite;
        public Sprite WorldSprite => _worldSprite;

        [SerializeField] private bool _leaksToNotGrandma;
        public bool LeaksToNotGrandma => _leaksToNotGrandma;

        [SerializeField] private ExchangeEntryData[] _exchangeEntries;
        public ExchangeEntryData[] ExchangeEntries => _exchangeEntries;

        [SerializeField] private ClueSO _fallbackReturnClue;
        public ClueSO FallbackReturnClue => _fallbackReturnClue;

        public bool OffersExchange => HasExchangeEntries || _fallbackReturnClue != null;

        private bool HasExchangeEntries => _exchangeEntries != null && _exchangeEntries.Length > 0;

        public NpcProfile ToProfile()
        {
            var returnsByGivenClue = new Dictionary<string, string>();
            if (HasExchangeEntries)
            {
                foreach (ExchangeEntryData entry in _exchangeEntries)
                {
                    if (entry.GivenClue != null && entry.ReturnedClue != null)
                    {
                        returnsByGivenClue[entry.GivenClue.Id] = entry.ReturnedClue.Id;
                    }
                }
            }
            string fallbackId = _fallbackReturnClue != null ? _fallbackReturnClue.Id : null;
            return new NpcProfile(_id, _leaksToNotGrandma, new ExchangeTable(returnsByGivenClue, fallbackId));
        }

        public bool TryResolveClue(string returnedClueId, out ClueSO returnedClue)
        {
            if (HasExchangeEntries)
            {
                foreach (ExchangeEntryData entry in _exchangeEntries)
                {
                    if (entry.ReturnedClue != null && entry.ReturnedClue.Id == returnedClueId)
                    {
                        returnedClue = entry.ReturnedClue;
                        return true;
                    }
                }
            }
            if (_fallbackReturnClue != null && _fallbackReturnClue.Id == returnedClueId)
            {
                returnedClue = _fallbackReturnClue;
                return true;
            }
            returnedClue = null;
            return false;
        }
    }
}
