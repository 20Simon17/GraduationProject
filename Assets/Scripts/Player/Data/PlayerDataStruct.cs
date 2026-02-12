using System;
using UnityEngine;

[Serializable]
public struct PlayerDataStruct
{
    [Header("Debugging - Values")] 
    public float timeAtLastSlide;
    public float timeAtLastSlam;
    public float timeAtLastJump;
    public float currentSpeed;
    
    //TODO: convert all of these to properties and set "disableMovement" to true in the setters?
    [Header("Debugging - Bools")] 
    public bool isGrounded;
    public bool isTouchingGround;
    private bool canJump;
    public bool isSliding;
    public bool isSlamming;
    public bool canWallRunJump;
    public bool hasJumped;

    public bool CanJump
    {
        get => isGrounded || canJump || (isCoyoteTimeActive && !hasJumped);
        set => canJump = value;
    }

    [Header("General Settings")]
    public float velocityHardCap;
    public float defaultPlayerScaleY;
    public float jumpForceScaling;
    
    [Header("Ground")]
    public float counterForceSpeedThreshold;
    public float defaultFriction;

    [Header("Jump")]
    public bool isCoyoteTimeActive; // get => coyoteTime != 0 && coyoteTime <= coyoteTimeDuration
    public float coyoteTime;
    public float coyoteTimeDuration;
    public float jumpForce;
    public float slamJumpTimeFrame;
    public float slideJumpTimeFrame;
    
    [Header("Slide")]
    public float slideSpeedBoost;
    public float slideSpeed;
    public float slideJumpForceMultiplier;
    public float slideJumpSpeedMultiplier;
    public float slideFriction;
    public float slidePlayerScaleY;
    public float timeUntilMaxFriction;
    public float slideCooldown;
    public float slideSpeedRequirement;

    [Header("Ground Slam")]
    public float groundSlamForce;
    public float slamJumpForceMultiplier;
    public float groundSlamGravityMultiplier;
    public float slamCooldown;

    [Header("Movement")] 
    public Vector3 defaultGravity;
    public PhysicsMaterial physicsMaterial;
    public float maxRunVelocity;
    public float accelerationForce;
    public float decelerationForce;

    public float forwardVelocity;
    public float strafeVelocity;
    public float maxStrafeVelocity;
    public float initialVelocity;
    public float trueVelocity;

    [Header("Wall Run")]
    public float wallRunCheckDistance;
    public float wallRunJumpSpeedBoost;
    public float wallRunGravityMultiplier;
    public float wallRunCancelVerticalVelocity;
    public int currentWallRuns;
    public int maxWallRuns;
}