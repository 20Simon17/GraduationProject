using System;
using UnityEngine;

[Serializable]
public struct PlayerDataStruct
{
    [Header("General Settings")]
    public float velocityHardCap;
    public float defaultPlayerScaleY;
    public float jumpForceScaling;
    public float speedLineSpeedRequirement;
    
    [Header("Ground")]
    public float defaultFriction;

    [Header("Jump")]
    public float coyoteTimeDuration;
    public float jumpForce;
    public float slamJumpTimeFrame;
    public float slideJumpTimeFrame;
    public float pullGrappleJumpTimeFrame;
    public float grappleJumpForwardForce;
    
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
    public float maxWallRunEntryVelocity;
    public float wallRunVerticalCancelVelocity;
    public float wallRunHorizontalEntryCancelVelocity;
    public float wallRunVerticalVelocityLoss;
    public int maxWallRuns;

    [Header("Wall Climb")]
    public float wallClimbCheckDistance;
    public float wallClimbVerticalVelocityLoss;
    public float wallClimbJumpOutwardForce;
    public float percentageConvertedVelocityOnWallClimb;

    [Header("Interaction")]
    public float maxInteractionDistance;

    [Header("Zipline")]
    public float ziplineAutoDropVelocity;
    public float ziplineAccelerationReduction;
}