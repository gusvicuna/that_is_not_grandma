using System;
using Game.Domain;
using UnityEngine;

namespace Game.Data
{
    /// <summary>Inspector shape of a <see cref="StoryCondition"/>.</summary>
    [Serializable]
    public class StoryConditionData
    {
        [Tooltip("Every one of these flags must be set for the beat to fire.")]
        [SerializeField] private string[] _requiredFlags;

        [Tooltip("If any of these flags is set, the beat does not fire.")]
        [SerializeField] private string[] _forbiddenFlags;

        [Tooltip("Earliest day this beat may fire. 0 = any day.")]
        [SerializeField] private int _minDay;

        public StoryCondition ToCondition()
        {
            return new StoryCondition(_requiredFlags, _forbiddenFlags, _minDay);
        }
    }
}
