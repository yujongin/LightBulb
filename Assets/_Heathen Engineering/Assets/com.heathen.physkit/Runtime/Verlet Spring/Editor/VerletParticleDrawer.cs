#if HE_SYSCORE

using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.PhysKit
{
    [CustomPropertyDrawer(typeof(VerletParticle))]
    public class VerletParticleDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            SerializedProperty root = prop.FindPropertyRelative("target");
            return EditorGUI.GetPropertyHeight(root);            
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty root = prop.FindPropertyRelative("target");

            label.text = (root.objectReferenceValue as Transform).name;

            label = EditorGUI.BeginProperty(pos, label, prop);
            EditorGUI.BeginChangeCheck();
            Rect p = new Rect(pos.x, pos.y, pos.width / 3f, EditorGUI.GetPropertyHeight(root));
            EditorGUI.LabelField(p, label.text);
            EditorGUI.EndChangeCheck();
            EditorGUI.EndProperty();
            prop.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif