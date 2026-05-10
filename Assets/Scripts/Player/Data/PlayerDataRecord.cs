using System;
using UnityEngine;

[Serializable]
public record PlayerDataRecord
{
    [Header("Settings")]
    public bool allowCameraShake;

    [Header("Timestamps")]
    public float timeAtLastSlide;
    public float timeAtLastSlam;
    public float timeAtLastJump;
    public float timeAtLastPullGrapple;
    
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

    public bool isJumping;

    public bool CanDoWallAction => !isGrounded && !isWallRunning && !isWallClimbing;
    
    [Space(5)]
    public bool isSliding;
    public bool isSlamming;
    public bool isWallRunning;
    public bool isWallClimbing;
    
    [Space(5)]
    public bool isHoldingJump;
    public bool canWallRunJump;
    public bool canWallClimbJump;
    public Vector3 leftWallNormal;
    public Vector3 rightWallNormal;
    public Vector3 frontWallNormal;
    public Vector3 previousWallNormal;
    public Vector3 previousWallRunDirection;
    public bool previousWallRunWasRight;
    public Vector3 previousWallRunNormal;
    public Vector3 previousWallClimbNormal;
    // public GameObject currentWallObject;
    // public GameObject previousWallObject;

    [Space(5)]
    public bool isInTimeTrial;
    
    [Header("Static Data")]
    public PlayerDataStruct dataStruct;
}