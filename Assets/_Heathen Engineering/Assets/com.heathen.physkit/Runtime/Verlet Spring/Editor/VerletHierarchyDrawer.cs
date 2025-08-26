#if HE_SYSCORE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.PhysKit
{
    [CustomPropertyDrawer(typeof(VerletHierarchy))]
    public class VerletHierarchyDrawer : PropertyDrawer
    {
        int tabPage = 0;
        private GUIStyle popupStyle;
        private readonly string[] popupOptions =
            { "Use Constant", "Use Static", "Use Reference" };

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            SerializedProperty root = prop.FindPropertyRelative("root");
            SerializedProperty ignoreList = prop.FindPropertyRelative("ignoreList");
            SerializedProperty nodes = prop.FindPropertyRelative("nodes");
            SerializedProperty Settings = prop.FindPropertyRelative("settings");
            SerializedProperty settings_mode = prop.FindPropertyRelative("settings.Mode");
            SerializedProperty settings_constantValue = prop.FindPropertyRelative("settings.m_constantValue");
            SerializedProperty settings_variable = prop.FindPropertyRelative("settings.Variable");

            if (prop.isExpanded)
            {
                if (tabPage == 0)
                {
                    if (settings_mode.enumValueIndex != 2)
                        return EditorGUI.GetPropertyHeight(settings_mode) + EditorGUI.GetPropertyHeight(settings_constantValue, settings_constantValue.isExpanded) + 65;
                    else
                        return EditorGUI.GetPropertyHeight(settings_mode) + EditorGUI.GetPropertyHeight(settings_variable) + 65;
                }
                else if (tabPage == 1)
                {
                    return EditorGUI.GetPropertyHeight(root) * nodes.arraySize + 85;
                }
                else
                    return EditorGUI.GetPropertyHeight(ignoreList, ignoreList.isExpanded) + 65;
            }
            else
            {
                return EditorGUI.GetPropertyHeight(root);
            }
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty root = prop.FindPropertyRelative("root");
            SerializedProperty ignoreList = prop.FindPropertyRelative("ignoreList");
            SerializedProperty nodes = prop.FindPropertyRelative("nodes");
            SerializedProperty Settings = prop.FindPropertyRelative("settings");
            

            label.text = (root.objectReferenceValue as Transform).name;

            label = EditorGUI.BeginProperty(pos, label, prop);
            EditorGUI.BeginChangeCheck();
            Rect p = new Rect(pos.x, pos.y, pos.width, EditorGUI.GetPropertyHeight(root));
            prop.isExpanded = EditorGUI.Foldout(p, prop.isExpanded, label.text, true);
            if (prop.isExpanded)
            {
                p.y += p.height;
                EditorGUI.PropertyField(p, root, GUIContent.none);

                int iLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;
                
                float fullW = p.width - 15;
                Rect tabRect = new Rect(p);
                tabRect.x += 15;
                tabRect.y += p.height;
                tabRect.width = fullW / 3;
                
                if (GUI.Toggle(tabRect, tabPage == 0, "Tree Settings", EditorStyles.toolbarButton))
                    tabPage = 0;

                tabRect.x += tabRect.width;
                if (GUI.Toggle(tabRect, tabPage == 1, "Active Nodes (" + nodes.arraySize.ToString() + ")", EditorStyles.toolbarButton))
                    tabPage = 1;
                tabRect.x += tabRect.width;

                if (GUI.Toggle(tabRect, tabPage == 2, "Ignored Children", EditorStyles.toolbarButton))
                    tabPage = 2;

                if(tabPage == 0)
                {
                    nodes.isExpanded = false;
                    ignoreList.isExpanded = false;
                    p.y += (p.height * 2) + 10;
                    p.height = EditorGUI.GetPropertyHeight(Settings);
                    DrawSettings(p, Settings);

                    //EditorGUI.PropertyField(p, Settings, new GUIContent("Settings"));
                    
                }
                if (tabPage == 1)
                {
                    p.y += (p.height * 2) + 10;
                    
                    float w = p.width;
                    p.width = 105;
                    p.x += 5;
                    if (DrawButton(p, "Refresh Nodes"))
                    {
                        var obj = fieldInfo.GetValue(prop.serializedObject.targetObject);
                        VerletHierarchy myDataClass = obj as VerletHierarchy;

                        if (obj is ICollection)
                        {
                            var index = Convert.ToInt32(new string(prop.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                            myDataClass = ((ICollection<VerletHierarchy>)obj).ToList()[index];
                        }
                        else if (obj.GetType().IsArray)
                        {
                            var index = Convert.ToInt32(new string(prop.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                            myDataClass = ((VerletHierarchy[])obj)[index];
                        }

                        myDataClass.RegisterNodes();
                    }
                    p.width = w;
                    
                    p.x -= 5;
                    //p.y += 18;

                    bool children = true;
                    int childCount = nodes.arraySize;
                    for(int i = 0; i < childCount; i++)
                    {
                        var nodecHild = nodes.GetArrayElementAtIndex(i);
                        p.y += p.height;
                        p.height = EditorGUI.GetPropertyHeight(nodecHild);
                        children = EditorGUI.PropertyField(p, nodecHild);
                    }
                }
                if (tabPage == 2)
                {
                    p.y += (p.height * 2) + 10;
                    p.height = EditorGUI.GetPropertyHeight(ignoreList, ignoreList.isExpanded);
                    EditorGUI.PropertyField(p, ignoreList, new GUIContent("Ignored Children"), true);
                }

                EditorGUI.indentLevel = iLevel;
            }
            //EditorGUI.PropertyField(pos, prop, true);
            if (EditorGUI.EndChangeCheck())
            {
                var obj = fieldInfo.GetValue(prop.serializedObject.targetObject);
                VerletHierarchy myDataClass = obj as VerletHierarchy;

                if (obj is ICollection)
                {
                    var index = Convert.ToInt32(new string(prop.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                    myDataClass = ((ICollection<VerletHierarchy>)obj).ToList()[index];
                }
                else if (obj.GetType().IsArray)
                {
                    var index = Convert.ToInt32(new string(prop.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                    myDataClass = ((VerletHierarchy[])obj)[index];
                }

                if (myDataClass.settings.Mode != VariableReferenceType.Referenced || myDataClass.settings.m_variable != null)
                    myDataClass.RegisterNodes();

                prop.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        void DrawSettings(Rect position, SerializedProperty prop)
        {
            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            SerializedProperty mode = prop.FindPropertyRelative("Mode");
            SerializedProperty constantValue = prop.FindPropertyRelative("m_constantValue");
            SerializedProperty variable = prop.FindPropertyRelative("Variable");

            // Calculate rect for configuration button
            Rect buttonRect = new Rect(position);
            buttonRect.y += popupStyle.margin.top;
            buttonRect.width = position.width; 
            buttonRect.height = EditorGUI.GetPropertyHeight(mode);
            
            EditorGUI.PropertyField(buttonRect,
                    mode,
                    new GUIContent("Settings", "Constant: configure settings here.\n\nSatic: configure settings here \n(cannot be changed at run time).\n\nReferenced: Reference a VerletHierarchySettings."));
            

            if (constantValue.hasChildren && mode.enumValueIndex != 2)
            {
                buttonRect.y += EditorGUI.GetPropertyHeight(mode);

                EditorGUI.PropertyField(buttonRect,
                    constantValue,
                    new GUIContent("Configuration"), true);
            }
            else
            {
                buttonRect.y += EditorGUI.GetPropertyHeight(mode);

                EditorGUI.PropertyField(buttonRect, variable, GUIContent.none);
            }
        }
                
        bool DrawButton(Rect r, string label)
        {
            
            Rect p = new Rect(r);
            p.x += 10;
            p.width -= 5;
            if (GUI.Button(p, GUIContent.none))
                return true;
            int iL = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;
            EditorGUI.LabelField(r, label);
            EditorGUI.indentLevel = iL;
            return false;
        }
    }
}


#endif