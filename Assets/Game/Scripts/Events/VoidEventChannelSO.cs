using System;
using UnityEngine;

namespace Game.Events
{
    /// <summary>
    /// A signal with no payload. Not an EventChannelSO&lt;T&gt;: "no payload" isn't a payload type,
    /// and Action beats Action&lt;Void&gt; at every call site.
    /// </summary>
    [CreateAssetMenu(fileName = "CH_Void", menuName = "Game/Events/Void")]
    public class VoidEventChannelSO : EventChannelSO
    {
        public event Action Raised;

        public void Raise()
        {
            Raised?.Invoke();
        }

#if UNITY_EDITOR
        public override int ListenerCount => Raised?.GetInvocationList().Length ?? 0;

        public override void RaiseFromEditor()
        {
            Raise();
        }
#endif
    }
}
