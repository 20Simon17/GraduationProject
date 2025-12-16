using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralMesh), true)]
public class ProceduralMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        ProceduralMesh proceduralMesh = (ProceduralMesh)target;
        
        GUILayout.Space(20);
        if (GUILayout.Button("Regenerate Mesh"))
        {
            proceduralMesh.UpdateMesh();
        }
    }
}
