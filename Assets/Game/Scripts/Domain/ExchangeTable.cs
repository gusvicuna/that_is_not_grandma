using System;
using System.Collections.Generic;

namespace Game.Domain
{
    public class ExchangeTable
    {
        private readonly IReadOnlyDictionary<string, string> _returnsByGivenClue;
        private readonly string _fallbackReturnClueId;

        public ExchangeTable(IReadOnlyDictionary<string, string> pairs, string fallbackReturnClueId = null)
        {
            if (pairs == null)
            {
                throw new ArgumentNullException(nameof(pairs));
            }

            var validatedPairs = new Dictionary<string, string>(pairs.Count);
            foreach (KeyValuePair<string, string> pair in pairs)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(pair.Value))
                {
                    throw new ArgumentException("Pairs cannot contain null, empty or whitespace ids.", nameof(pairs));
                }
                validatedPairs[pair.Key] = pair.Value;
            }

            _returnsByGivenClue = validatedPairs;
            _fallbackReturnClueId = fallbackReturnClueId;
        }

        public bool TryGetReturn(string givenClueId, out string returnedClueId)
        {
            if (givenClueId == null)
            {
                throw new ArgumentNullException(nameof(givenClueId));
            }
            if (_returnsByGivenClue.TryGetValue(givenClueId, out returnedClueId))
            {
                return true;
            }
            returnedClueId = _fallbackReturnClueId;
            return returnedClueId != null;
        }
    }
}
