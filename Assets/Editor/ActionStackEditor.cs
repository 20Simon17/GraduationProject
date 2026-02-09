using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActionStack), true)]
public class ActionStackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ActionStack stack = target as ActionStack;
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Action Stack", EditorStyles.boldLabel);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        foreach (ActionStack.IAction evt in stack.Stack)
        {
            string name = "   #" + stack.Stack.IndexOf(evt) + ": " + evt;
            if (evt is Object obj)
            {
                if (GUILayout.Button(name, evt == stack.CurrentAction ? EditorStyles.boldLabel : EditorStyles.label))
                {
                    Selection.activeObject = obj;
                }
            }
            else
            {
                EditorGUILayout.LabelField(name, evt == stack.CurrentAction ? EditorStyles.boldLabel : EditorStyles.label);
            }
        }
        GUILayout.EndVertical();
    }

}

// Credit to Carl Granberg (my teacher) for this editor script
// https://www.linkedin.com/in/carl-granberg-90287048/