#if HE_SYSCORE

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.PhysKit
{
    [CustomEditor(typeof(VerletSpring))]
    public class VerletSpringEditor : Editor
    {
        private VerletSpring pBody;
        private SerializedProperty transformHierarchies;

        private void OnEnable()
        {
            transformHierarchies = serializedObject.FindProperty(nameof(VerletSpring.transformHierarchies));
        }

        public override void OnInspectorGUI()
        {
            pBody = target as VerletSpring;
            DropAreaGUI("Drop transforms here to construct verlet hierarchies from their children.\nTransforms should have at least 1 child.");
            DrawVerletHierarchy();

            serializedObject.ApplyModifiedProperties();
        }

        private void AddVerletHierarchy(Transform root)
        {
            VerletHierarchy nTree = new VerletHierarchy()
            {
                root = root,
                nodes = new List<VerletParticle>(),
                ignoreList = new List<Transform>(),
                settings = new VerletHierarchySettingsReference(new VerletHierarchySettings())
            };
            nTree.RegisterNodes();
            pBody.transformHierarchies.Add(nTree);
        }

        private void DrawVerletHierarchy()
        {
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < transformHierarchies.arraySize; i++)
            {
                Rect r = GUILayoutUtility.GetRect(0.0f, EditorGUI.GetPropertyHeight(transformHierarchies.GetArrayElementAtIndex(i)), GUILayout.ExpandWidth(true));
                var style = EditorStyles.miniButtonLeft;
                Color sC = GUI.backgroundColor;
                GUI.backgroundColor = new Color(sC.r * 1.25f, sC.g * 0.5f, sC.b * 0.5f, sC.a);
                if (GUI.Button(new Rect(r) { x = r.width, width = 20, height = 15 }, "X", EditorStyles.miniButtonLeft))
                {
                    GUI.backgroundColor = sC;
                    transformHierarchies.DeleteArrayElementAtIndex(i);
                }
                else
                {
                    GUI.backgroundColor = sC;
                    var property = transformHierarchies.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(r, property);
                    if(property.isExpanded)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }
                }
            }
            EditorGUI.indentLevel = il;
        }
        
        private void DropAreaGUI(string message)
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, message);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            // Do On Drag Stuff here
                            if(dragged_object.GetType() == typeof(GameObject))
                            {
                                GameObject go = dragged_object as GameObject;
                                if(go.transform.IsChildOf(pBody.transform))
                                {
                                    Transform tTrans = go.transform;
                                    if(tTrans.childCount > 0 && pBody.transformHierarchies.Count(t => t.root == tTrans) <= 0)
                                    {
                                        AddVerletHierarchy(tTrans);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }
        
    }
}


#endif