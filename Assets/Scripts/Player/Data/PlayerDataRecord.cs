using System;
using UnityEngine;

[Serializable]
public record PlayerDataRecord
{
    [Header("Timestamps")]
    public float timeAtLastSlide;
    public float timeAtLastSlam;
    public float timeAtLastJump;
    
    [Header("Active values")]
    public bool isCoyoteTimeActive;
    public float coyoteTime;
    
    [Space(5)]
    public bool isGrounded;
    
    public bool isOnSlope;
    public float slopeAngle;
    public Vector3 slopeNormal;
    
    public Vector3 GetSlopeMoveDirection(Vector3 inMoveDirection) => Vector3.ProjectOnPlane(inMoveDirection, slopeNormal).normalized;

    public bool isOnZipline;
    
    [Space(5)]
    public bool hasJumped;
    private bool canJump;
    public bool CanJump
    {
        get => isGrounded || canJump || (isCoyoteTimeActive && !hasJumped);
        set => canJump = value;
    }
    
    [Space(5)]
    public bool isSliding;
    public bool isSlamming;
    
    [Space(5)]
    public int currentWallRuns;
    public bool canWallRunJump;

    [Space(5)] 
    public bool isInTimeTrial;
    
    [Header("Static Data")]
    public PlayerDataStruct dataStruct;
}