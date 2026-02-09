using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public PlayerDataSO scriptableObject;
    public PlayerDataStruct PlayerDataStruct;

    private void Awake()
    {
        LoadFromScriptableObject();
    }
    
    [ContextMenu("Load Data")]
    private void LoadFromScriptableObject()
    {
        if (scriptableObject)
        {
            PlayerDataStruct = scriptableObject.playerData;
        }
        else
        {
            Debug.LogError("No scriptable object assigned to player data!");
        }
    }
    
    [ContextMenu("Save Data")]
    private void SaveToScriptableObject()
    {
        if (scriptableObject)
        {
            scriptableObject.playerData = PlayerDataStruct;
        }
        else
        {
            Debug.LogError("No scriptable object assigned to player data!");
        }
    }
}