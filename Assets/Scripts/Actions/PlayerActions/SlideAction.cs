using System;
using UnityEngine;

public class SlideAction : PlayerActionStack.PlayerAction
{
    public SlideAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData, PlayerActionStack inPlayer) 
        : base(inRb, inTransform, inData) { player = inPlayer; }
    
    private bool exitedEarly;

    private readonly PlayerActionStack player;
    
    private bool wasOnSlope;
    private Vector3 slopeVelocity;
    private bool doSpeedLoss = true;

    private Vector3 velocityOnEnter;
    
    public override bool IsDone()
    {
        if (!dataRecord.isGrounded && !dataRecord.isCoyoteTimeActive) return true;
        if (rb.linearVelocity.magnitude <= 2) return true;
        return ActionCompleted;
    }

    public override void OnBegin(bool bFirstTime)
    {
        if (!dataRecord.isGrounded) return;
        
        if (Time.time - dataRecord.timeAtLastSlide < data.slideCooldown ||
            rb.linearVelocity.magnitude < data.slideSpeedRequirement)
        {
            exitedEarly = true;
            CompleteAction();
            return;
        }
        
        dataRecord.isSliding = true;
        data.physicsMaterial.dynamicFriction = data.slideFriction;

        player.OnGroundedEvent += SetStartingVelocity;
        velocityOnEnter = rb.linearVelocity;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        
        //TODO: Only collider gets scaled down, model / capsule gets rotated to be flat instead
        transform.localScale = new Vector3(transform.localScale.x, data.slidePlayerScaleY, transform.localScale.z);
        if (rb.linearVelocity.y < 20) rb.AddForce(-transform.up * 25, ForceMode.Impulse); //Send the player downwards to stick to the ground
    }

    public override void OnEnd()
    {
        Physics.gravity = data.defaultGravity;
        if (exitedEarly) return;
        
        player.OnGroundedEvent -= SetStartingVelocity;
        
        dataRecord.isSliding = false;
        dataRecord.timeAtLastSlide = Time.time;
        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        transform.localScale = new Vector3(transform.localScale.x, data.defaultPlayerScaleY, transform.localScale.z);
    }

    public override void OnUpdate(float deltaTime)
    {
        if (dataRecord.isOnSlope)
        {
            if (!wasOnSlope) wasOnSlope = true;
            if (rb.linearVelocity.y < 0)
            {
                doSpeedLoss = false;
                Physics.gravity = Vector3.zero;
            }
            
            rb.linearVelocity = dataRecord.GetSlopeMoveDirection(rb.linearVelocity) * rb.linearVelocity.magnitude;
            slopeVelocity = rb.linearVelocity;
        }
        else if (wasOnSlope)
        {
            wasOnSlope = false;
            doSpeedLoss = true;
            Physics.gravity = data.defaultGravity;
            rb.linearVelocity = new Vector3(slopeVelocity.x, 0, slopeVelocity.z).normalized * slopeVelocity.magnitude;
        }
        else CheckForSlope();
        
        if (doSpeedLoss) DoSpeedFallOff(deltaTime);
    }

    private void CheckForSlope()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.5f) && hit.normal != Vector3.up)
        {
            rb.AddForce(-transform.up * 5, ForceMode.Impulse);
        }
    }

    private void SetStartingVelocity()
    {
        player.OnGroundedEvent -= SetStartingVelocity;
        
        Vector2 moveInput = InputManager.Instance.moveDirection;
        Vector3 slideDirection = transform.rotation * new Vector3(moveInput.x, 0, moveInput.y);
        
        if (velocityOnEnter.magnitude <= data.maxRunVelocity + 2)
        {
            rb.linearVelocity = slideDirection * data.slideSpeed;
        }
        else if (velocityOnEnter.magnitude < data.maxSlideSpeed)
        {
            rb.linearVelocity = slideDirection * (velocityOnEnter.magnitude * data.slideSpeedBoost);
        }
        
        if (dataRecord.isOnSlope)
        {
            rb.linearVelocity = dataRecord.GetSlopeMoveDirection(rb.linearVelocity) * rb.linearVelocity.magnitude;
        }
    }

    private void DoSpeedFallOff(float deltaTime)
    {
        if (rb.linearVelocity.magnitude - (data.slideSpeedLoss * deltaTime) <= 0)
        {
            rb.linearVelocity = Vector3.zero;
        }
        else rb.linearVelocity = rb.linearVelocity.normalized * (rb.linearVelocity.magnitude - (data.slideSpeedLoss * deltaTime));
    }
}