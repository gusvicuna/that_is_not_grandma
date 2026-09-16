using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class ExchangeEntryData
    {
        [SerializeField] private ClueSO _givenClue;
        public ClueSO GivenClue => _givenClue;

        [SerializeField] private ClueSO _returnedClue;
        public ClueSO ReturnedClue => _returnedClue;
    }
}
