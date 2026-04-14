using System;
using UnityEngine;

[Serializable]
public struct PlayerDataStruct
{
    [Header("General Settings")]
    public float velocityHardCap;
    public float defaultPlayerScaleY;
    public float jumpForceScaling;
    
    [Header("Ground")]
    //public float counterForceSpeedThreshold;
    public float defaultFriction;
    //public float counterFriction;
    //public float noInputFriction;

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
    //public float timeUntilMaxFriction;
    public float slideCooldown;
    public float slideSpeedRequirement;
    public float slideSpeedLoss;
    public float maxSlideSpeed;

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

    [Header("Wall Run")]
    public float wallRunCheckDistance;
    public float wallRunJumpSpeedBoost;
    public float wallRunGravityMultiplier;
    public float maxWallRunEntryVelocity;
    public float wallRunCancelVerticalVelocity;
    public float wallRunVerticalVelocityLoss;
    public float verticalWallJumpOutwardForce;
    public float forwardWallRunCheckDistanceMultiplier;
    public float percentageConvertedVelocityOnVerticalWallRun;
    public int maxWallRuns;

    [Header("Interaction")]
    public float maxInteractionDistance;

    [Header("Zipline")]
    public float ziplineAutoDropVelocity;
    public float ziplineAccelerationReduction;
}