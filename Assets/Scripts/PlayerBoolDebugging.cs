using System;
using TMPro;
using UnityEngine;

public class PlayerBoolDebugging : MonoBehaviour
{
    public TMP_Text IsGroundedText;
    public TMP_Text CanJumpText;

    private PlayerData playerData;
    
    private void Start()
    {
        playerData = FindFirstObjectByType<PlayerData>();
    }

    private void Update()
    {
        PlayerDataStruct data = playerData.PlayerDataStruct;
        IsGroundedText.color = data.isGrounded ? Color.green : Color.red;
        CanJumpText.color = data.CanJump ? Color.green : Color.red;
    }
}
