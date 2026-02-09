using System;
using UnityEngine;

[Serializable]
public class PlayerData : MonoBehaviour
{
    public PlayerDataSO scriptableObject;
    public PlayerDataRecord dataRecord;

    private void Awake()
    {
        LoadFromScriptableObject();
    }
    
    [ContextMenu("Load Data")]
    private void LoadFromScriptableObject()
    {
        if (scriptableObject)
        {
            dataRecord.dataStruct = scriptableObject.dataRecord.dataStruct;
        }
        else throw new NullReferenceException("No scriptable object assigned to player data!");
    }
    
    [ContextMenu("Save Data")]
    private void SaveToScriptableObject()
    {
        if (scriptableObject)
        {
            scriptableObject.dataRecord = dataRecord;
        }
        else throw new NullReferenceException("No scriptable object assigned to player data!");
    }
}