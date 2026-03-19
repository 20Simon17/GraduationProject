using System;
using UnityEngine;

public class SlideAction : PlayerActionStack.PlayerAction
{
    public SlideAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}
    
    private float slideTime;
    private bool exitedEarly;
    private bool isAboveSlope;
    private bool wasOnSlope = false;
    
    public override bool IsDone()
    {
        if (!dataRecord.isGrounded && !dataRecord.isCoyoteTimeActive && !isAboveSlope)
        {
            Debug.Log("Weren't grounded");
            return true;
        }
        if (rb.linearVelocity.magnitude <= 2)
        {
            Debug.Log("Too slow");
            return true;
        }
        return ActionCompleted;
    }

    public override void OnBegin(bool bFirstTime)
    {
        if (!dataRecord.isGrounded) return;
        
        if (Time.time - dataRecord.timeAtLastSlide < data.slideCooldown ||
            rb.linearVelocity.magnitude < data.slideSpeedRequirement)
        {
            CompleteAction();
            exitedEarly = true;
            return;
        }
        
        dataRecord.isSliding = true;
        data.physicsMaterial.dynamicFriction = data.slideFriction;
        
        slideTime = 0;
        
        //TODO: Only collider gets scaled down, model / capsule gets rotated to be flat instead
        transform.localScale = new Vector3(transform.localScale.x, data.slidePlayerScaleY, transform.localScale.z);
        rb.AddForce(-transform.up * 5, ForceMode.Impulse); //Send the player downwards to stick to the ground
        
        dataRecord.timeAtLastSlide = Time.time;

        Vector2 moveInput = InputManager.Instance.moveDirection;
        Vector3 slideDirection = transform.rotation * new Vector3(moveInput.x, 0, moveInput.y);
        
        if (rb.linearVelocity.magnitude <= data.maxRunVelocity)
        {
            rb.linearVelocity = slideDirection * data.slideSpeed;
            
            if (dataRecord.isOnSlope)
            {
                rb.linearVelocity = dataRecord.GetSlopeMoveDirection(rb.linearVelocity) * rb.linearVelocity.magnitude;
            }
        }
        else
        {
            rb.linearVelocity = slideDirection * (rb.linearVelocity.magnitude * data.slideSpeedBoost);
            
            if (dataRecord.isOnSlope)
            {
                rb.linearVelocity = dataRecord.GetSlopeMoveDirection(rb.linearVelocity) * rb.linearVelocity.magnitude;
            }
        }
    }

    public override void OnEnd()
    {
        if (exitedEarly) return;
        
        dataRecord.isSliding = false;
        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        transform.localScale = new Vector3(transform.localScale.x, data.defaultPlayerScaleY, transform.localScale.z);
    }

    public override void OnUpdate(float deltaTime)
    {
        /*slideTime += deltaTime;
        
        float progress = Mathf.Min(slideTime / data.timeUntilMaxFriction, 1);
        float lerpProgress = (float)(1 - Math.Sqrt(1 - Math.Pow(progress, 2))); // ease in circ
        data.physicsMaterial.dynamicFriction = Mathf.Lerp(data.slideFriction, data.defaultFriction, lerpProgress);*/
        
        if (dataRecord.isOnSlope)
        {
            if (!wasOnSlope)
            {
                wasOnSlope = true;
            }
            
            slideTime = 0;
            data.physicsMaterial.dynamicFriction = data.slideFriction;
            rb.linearVelocity = dataRecord.GetSlopeMoveDirection(rb.linearVelocity) * rb.linearVelocity.magnitude;
            
            // this should only happen if it's a downhill, otherwise maybe add the slope angle to the dynamic friction?
        }
        else
        {
            if (wasOnSlope && !dataRecord.isCoyoteTimeActive)
            {
                wasOnSlope = false;
                Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized * rb.linearVelocity.magnitude;
                rb.linearVelocity = flatVelocity;
                // TODO: fix the line above this, this happens because the ground check doesn't detect the slope anymore and it bugs out
            }
            
            slideTime += deltaTime;
        }
        
        float progress = Mathf.Min(slideTime / data.timeUntilMaxFriction, 1);
        float lerpProgress = (float)(1 - Math.Sqrt(1 - Math.Pow(progress, 2))); // ease in circ
        data.physicsMaterial.dynamicFriction = Mathf.Lerp(data.slideFriction, data.defaultFriction, lerpProgress);
    }
}
