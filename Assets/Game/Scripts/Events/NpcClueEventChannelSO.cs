using System;
using UnityEngine;
using Game.Data;

namespace Game.Events
{
    /// <summary>
    /// Two payloads, so it can't ride EventChannelSO&lt;T&gt;. One bespoke channel is cheaper than
    /// a second generic no other signal would use.
    /// </summary>
    [CreateAssetMenu(fileName = "CH_NpcClue", menuName = "Game/Events/Npc Clue Shared")]
    public class NpcClueEventChannelSO : EventChannelSO
    {
        public event Action<NpcSO, ClueSO> Raised;

        public void Raise(NpcSO npc, ClueSO clue)
        {
            Raised?.Invoke(npc, clue);
        }

#if UNITY_EDITOR
        [SerializeField]
        [Tooltip("Payload used by the Raise button below. Editor only — never read at runtime.")]
        private NpcSO _editorNpc;

        [SerializeField]
        [Tooltip("Payload used by the Raise button below. Editor only — never read at runtime.")]
        private ClueSO _editorClue;

        public override int ListenerCount => Raised?.GetInvocationList().Length ?? 0;

        public override void RaiseFromEditor()
        {
            Raise(_editorNpc, _editorClue);
        }
#endif
    }
}
