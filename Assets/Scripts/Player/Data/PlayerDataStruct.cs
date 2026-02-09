using UnityEngine;

public struct PlayerDataStruct
{
    [Header("Debugging - Values")] 
    public float timeAtLastSlide;
    public float timeAtLastSlam;
    public float currentSpeed;
    
    //TODO: convert all of these to properties and set "disableMovement" to true in the setters?
    [Header("Debugging - Bools")] 
    public bool isGrounded;
    public bool isTouchingGround;
    private bool canJump;
    public bool isSliding;
    public bool isSlamming;

    public bool CanJump
    {
        get => isGrounded || canJump;
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
    public float slideFallOfThreshold;
    public float slideFallOfForce;

    [Header("Ground Slam")]
    public float groundSlamForce;
    public float slamJumpForceMultiplier;
    public float groundSlamGravityMultiplier;
    
    [Header("Movement")]
    public PhysicsMaterial physicsMaterial;
    public float maxRunVelocity;
    public float accelerationForce;
    public float decelerationForce;

    public float forwardVelocity;
    public float strafeVelocity;
    public float maxStrafeVelocity;
    public float initialVelocity;
    public float trueVelocity;
}