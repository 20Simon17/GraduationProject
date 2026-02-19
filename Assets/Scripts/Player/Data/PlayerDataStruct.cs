using System;
using UnityEngine;

[Serializable]
public struct PlayerDataStruct
{
    [Header("Debugging - Values")] 
    public float currentSpeed; //unused currently
    
    [Header("General Settings")]
    public float velocityHardCap;
    public float defaultPlayerScaleY;
    public float jumpForceScaling;
    
    [Header("Ground")]
    public float counterForceSpeedThreshold;
    public float defaultFriction;

    [Header("Jump")]
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
    public int maxWallRuns;

    public float maxInteractionDistance;

    public float ziplineAutoDropVelocity;
}