using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Draws a story effect showing only the fields its kind actually uses. Enums have no empty
    /// state, so a MoveActor's room and a SetTension's level would otherwise sit there with a
    /// default that looks chosen — and "SetTension: Calm" left by accident does not do nothing,
    /// it actively calms the music.
    /// </summary>
    [CustomPropertyDrawer(typeof(StoryEffectData))]
    public class StoryEffectDataDrawer : PropertyDrawer
    {
        private const string Kind = "_kind";
        private const string ActorId = "_actorId";
        private const string Room = "_room";
        private const string Npc = "_npc";
        private const string Dialogue = "_dialogue";
        private const string Tension = "_tension";
        private const string Flag = "_flag";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lines = 1 + FieldsFor(KindOf(property)).Length;
            if (HasEmptyRequiredField(property))
            {
                lines += 2; // the help box
            }
            return lines * EditorGUIUtility.singleLineHeight
                   + lines * EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect line = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(line, property.FindPropertyRelative(Kind));
            Advance(ref line);

            foreach (string fieldName in FieldsFor(KindOf(property)))
            {
                EditorGUI.PropertyField(line, property.FindPropertyRelative(fieldName));
                Advance(ref line);
            }

            if (HasEmptyRequiredField(property))
            {
                Rect box = new Rect(line.x, line.y, line.width, line.height * 2f);
                EditorGUI.HelpBox(box, "This effect will do nothing: fill the field above.", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }

        private static void Advance(ref Rect line)
        {
            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private static StoryEffectKind KindOf(SerializedProperty property)
        {
            return (StoryEffectKind)property.FindPropertyRelative(Kind).enumValueIndex;
        }

        private static string[] FieldsFor(StoryEffectKind kind)
        {
            switch (kind)
            {
                case StoryEffectKind.ShowActor:
                case StoryEffectKind.HideActor:
                    return new[] { ActorId };
                case StoryEffectKind.MoveActor:
                    return new[] { ActorId, Room };
                case StoryEffectKind.SetNpcDialogue:
                    return new[] { Npc, Dialogue };
                case StoryEffectKind.PlayDialogue:
                    return new[] { Dialogue };
                case StoryEffectKind.SetTension:
                    return new[] { Tension };
                case StoryEffectKind.SetFlag:
                    return new[] { Flag };
                default:
                    return new string[0];
            }
        }

        /// <summary>
        /// Only catches fields that have a real empty state. A room or a tension level cannot be
        /// "unset", which is exactly why this drawer hides them when they are irrelevant.
        /// </summary>
        private static bool HasEmptyRequiredField(SerializedProperty property)
        {
            switch (KindOf(property))
            {
                case StoryEffectKind.ShowActor:
                case StoryEffectKind.HideActor:
                case StoryEffectKind.MoveActor:
                    return IsEmptyString(property, ActorId);
                case StoryEffectKind.SetNpcDialogue:
                    return IsNullReference(property, Npc) || IsNullReference(property, Dialogue);
                case StoryEffectKind.PlayDialogue:
                    return IsNullReference(property, Dialogue);
                case StoryEffectKind.SetFlag:
                    return IsEmptyString(property, Flag);
                default:
                    return false;
            }
        }

        private static bool IsEmptyString(SerializedProperty property, string fieldName)
        {
            return string.IsNullOrWhiteSpace(property.FindPropertyRelative(fieldName).stringValue);
        }

        private static bool IsNullReference(SerializedProperty property, string fieldName)
        {
            return property.FindPropertyRelative(fieldName).objectReferenceValue == null;
        }
    }
}
