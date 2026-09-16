using UnityEditor;
using UnityEngine;
using Game.Events;

namespace Game.EditorTools
{
    /// <summary>
    /// One inspector for every channel asset: shows how many components are listening and fires the
    /// channel by hand. Replaces the throwaway debug components that would otherwise exist to test
    /// a signal whose real raiser isn't written yet.
    /// </summary>
    [CustomEditor(typeof(EventChannelSO), true)]
    public class EventChannelSOEditor : UnityEditor.Editor
    {
        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter play mode to see listeners and raise this channel.",
                    MessageType.Info);
                return;
            }

            EventChannelSO channel = (EventChannelSO)target;
            int listeners = channel.ListenerCount;

            EditorGUILayout.LabelField("Listeners", listeners.ToString());
            if (listeners == 0)
            {
                EditorGUILayout.HelpBox(
                    "Nothing is listening. Either no component subscribed in OnEnable, or the asset " +
                    "wired into that component is a different one.",
                    MessageType.Warning);
            }

            if (GUILayout.Button("Raise"))
            {
                channel.RaiseFromEditor();
            }
        }
    }
}
