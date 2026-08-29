using Game.Data;
using Game.Domain;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Shows only the match fields the chosen trigger reads. The rest still exist on the asset —
    /// they are simply never used, and an enum with no empty state looks like a decision nobody
    /// made. A RoomEntered beat silently matching the Kitchen is the classic adventure-game flag
    /// bug; this inspector is the cheapest place to stop it.
    /// </summary>
    [CustomEditor(typeof(StoryBeatSO))]
    public class StoryBeatSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty id = serializedObject.FindProperty("_id");
            SerializedProperty trigger = serializedObject.FindProperty("_trigger");

            EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(id);
            if (string.IsNullOrWhiteSpace(id.stringValue))
            {
                EditorGUILayout.HelpBox(
                    "A beat needs an id — the director throws on an empty one, and duplicates throw too.",
                    MessageType.Error);
            }
            EditorGUILayout.PropertyField(trigger);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Match", EditorStyles.boldLabel);
            DrawMatchFields((StoryTrigger)trigger.enumValueIndex);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rules", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_repeatable"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_condition"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
            SerializedProperty effects = serializedObject.FindProperty("_effects");
            EditorGUILayout.PropertyField(effects, true);
            if (effects.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "This beat fires and does nothing. Add an effect, or delete the beat.",
                    MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMatchFields(StoryTrigger trigger)
        {
            switch (trigger)
            {
                case StoryTrigger.ClueCollected:
                    DrawWildcardable("_matchClue", "Any clue collected.");
                    break;
                case StoryTrigger.ClueShared:
                    DrawWildcardable("_matchClue", "Any clue.");
                    DrawWildcardable("_matchNpc", "Shared with any NPC.");
                    break;
                case StoryTrigger.ItemInspected:
                    DrawWildcardable("_matchItem", "Any item inspected.");
                    break;
                case StoryTrigger.DialogueFinished:
                    DrawWildcardable("_matchDialogue", "Any conversation ending.");
                    break;
                case StoryTrigger.RoomEntered:
                    DrawOptOutEnum("_matchAnyRoom", "_matchRoom");
                    break;
                case StoryTrigger.DayStarted:
                    SerializedProperty day = serializedObject.FindProperty("_matchDay");
                    EditorGUILayout.PropertyField(day);
                    if (day.intValue <= 0)
                    {
                        EditorGUILayout.LabelField(" ", "Fires on every morning.", EditorStyles.miniLabel);
                    }
                    break;
                case StoryTrigger.PoliceCallResolved:
                    DrawOptOutEnum("_matchAnyOutcome", "_matchOutcome");
                    break;
            }
        }

        /// <summary>An empty asset reference means "any", so say so instead of leaving a blank slot.</summary>
        private void DrawWildcardable(string fieldName, string wildcardHint)
        {
            SerializedProperty field = serializedObject.FindProperty(fieldName);
            EditorGUILayout.PropertyField(field);
            if (field.objectReferenceValue == null)
            {
                EditorGUILayout.LabelField(" ", wildcardHint, EditorStyles.miniLabel);
            }
        }

        /// <summary>The enum only appears once the author has said they want a specific value.</summary>
        private void DrawOptOutEnum(string anyFieldName, string valueFieldName)
        {
            SerializedProperty any = serializedObject.FindProperty(anyFieldName);
            EditorGUILayout.PropertyField(any);
            if (!any.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(valueFieldName));
            }
        }
    }
}
