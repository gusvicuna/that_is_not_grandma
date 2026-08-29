using System;
using UnityEngine;

namespace Game.Events
{
    /// <summary>
    /// Base for every event channel. Non-generic so a single custom editor can serve them all;
    /// holds only what is common to every channel, whatever its payload.
    /// </summary>
    public abstract class EventChannelSO : ScriptableObject
    {
        [SerializeField]
        [TextArea(2, 5)]
        [Tooltip("What this channel means and who is expected to raise it. Read by humans, not by code.")]
        private string _description;

        public string Description => _description;

#if UNITY_EDITOR
        /// <summary>Live subscriber count. Editor-only: the answer to "why is nothing reacting?".</summary>
        public abstract int ListenerCount { get; }

        /// <summary>Raises the channel from the inspector button. Never call this from gameplay code.</summary>
        public abstract void RaiseFromEditor();
#endif
    }

    /// <summary>
    /// An event channel carrying one payload. Concrete subclasses exist per payload type because
    /// Unity cannot create assets from an open generic ScriptableObject.
    /// </summary>
    public abstract class EventChannelSO<T> : EventChannelSO
    {
        public event Action<T> Raised;

        public void Raise(T payload)
        {
            Raised?.Invoke(payload);
        }

#if UNITY_EDITOR
        [SerializeField]
        [Tooltip("Payload used by the Raise button below. Editor only — never read at runtime.")]
        private T _editorPayload;

        public override int ListenerCount => Raised?.GetInvocationList().Length ?? 0;

        public override void RaiseFromEditor()
        {
            Raise(_editorPayload);
        }
#endif
    }
}
