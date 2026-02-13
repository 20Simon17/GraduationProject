using System;
using TMPro;
using UnityEngine;

public class PlayerBoolDebugging : MonoBehaviour
{
    public TMP_Text IsGroundedText;
    public TMP_Text CanJumpText;

    private PlayerData playerData;
    private PlayerDataRecord dataRecord;
    
    private void Start()
    {
        playerData = FindFirstObjectByType<PlayerData>();
        dataRecord = playerData.dataRecord;
    }

    private void Update()
    {
        IsGroundedText.color = dataRecord.isGrounded ? Color.green : Color.red;
        CanJumpText.color = dataRecord.CanJump ? Color.green : Color.red;
    }
}
