using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    [Header("Debugging - Values")] 
    public float timeAtLastSlide;
    public float timeAtLastSlam;
    public float currentSpeed;
    
    //TODO: convert all of these to properties and set "disableMovement" to true in the setters?
    [Header("Debugging - Bools")] 
    public bool isGrounded;
    private bool canJump;
    public bool isSliding;
    public bool isSlamming;

    public bool CanJump
    {
        get => isGrounded || canJump;
        set => canJump = value;
    }

    [Header("General Settings")]
    public float velocityHardCap = 150f;
    public float defaultPlayerScaleY = 2.0f;
    public float jumpForceScaling = 100f;
    
    [Header("Ground")]
    public float counterForceSpeedThreshold = 3.0f;
    public float defaultFriction = 0.6f;

    [Header("Jump")]
    public float jumpForce = 7.5f;
    public float slamJumpTimeFrame = 0.2f;
    public float slideJumpTimeFrame = 0.2f;
    
    [Header("Slide")]
    public float slideSpeedBoost = 1.2f;
    public float slideSpeed = 9.0f;
    public float slideJumpForceMultiplier = 0.85f;
    public float slideJumpSpeedMultiplier = 1.15f;
    public float slideFriction = 0.05f;
    public float slidePlayerScaleY = 0.5f;
    public float slideFallOfThreshold = 8f;
    public float slideFallOfForce = 5f;

    [Header("Ground Slam")]
    public float groundSlamForce = 10f;
    public float slamJumpForceMultiplier = 1.3f;
    public float groundSlamGravityMultiplier = 3.0f;
    
    [Header("Movement")]
    public PhysicsMaterial physicsMaterial;
    public float maxRunVelocity = 10f;
    public float accelerationForce = 50f;
    public float decelerationForce = 50f;

    public float forwardVelocity = 0.0f;
    public float strafeVelocity = 0.0f;
    public float initialVelocity = 4.0f;
}