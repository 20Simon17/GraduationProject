using System;
using UnityEngine;

public class SlideAction : PlayerActionStack.PlayerAction
{
    public SlideAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}
    
    private float slideTime;
    private bool exitedEarly;
    
    private bool wasOnSlope;
    private Vector3 slopeVelocity;
    
    public override bool IsDone()
    {
        if (!dataRecord.isGrounded && !dataRecord.isCoyoteTimeActive)
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
            exitedEarly = true;
            CompleteAction();
            return;
        }
        
        slideTime = 0;
        dataRecord.timeAtLastSlide = Time.time;
        
        dataRecord.isSliding = true;
        data.physicsMaterial.dynamicFriction = data.slideFriction;
        
        //TODO: Only collider gets scaled down, model / capsule gets rotated to be flat instead
        transform.localScale = new Vector3(transform.localScale.x, data.slidePlayerScaleY, transform.localScale.z);
        rb.AddForce(-transform.up * 10, ForceMode.Impulse); //Send the player downwards to stick to the ground
        
        Vector2 moveInput = InputManager.Instance.moveDirection;
        Vector3 slideDirection = transform.rotation * new Vector3(moveInput.x, 0, moveInput.y);
        
        if (rb.linearVelocity.magnitude <= data.maxRunVelocity)
        {
            rb.linearVelocity = slideDirection * data.slideSpeed;
        }
        else rb.linearVelocity = slideDirection * (rb.linearVelocity.magnitude * data.slideSpeedBoost);
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
        
        // this should only happen if it's a downhill, otherwise maybe add the slope angle to the dynamic friction?
        if (dataRecord.isOnSlope)
        {
            if (!wasOnSlope) wasOnSlope = true;
            
            slideTime = 0;
            rb.linearVelocity = dataRecord.GetSlopeMoveDirection(rb.linearVelocity) * rb.linearVelocity.magnitude;
            slopeVelocity = rb.linearVelocity;
        }
        else
        {
            if (wasOnSlope /*&& !dataRecord.isCoyoteTimeActive*/)
            {
                wasOnSlope = false;
                Vector3 flatVelocity = new Vector3(slopeVelocity.x, 0, slopeVelocity.z).normalized * slopeVelocity.magnitude;
                rb.linearVelocity = flatVelocity;
                
                //TODO: Fix the transition between slope and ground. Only works sometimes, don't really understand it currently
            }
            
            slideTime += deltaTime;
        }
        
        float progress = Mathf.Min(slideTime / data.timeUntilMaxFriction, 1);
        float lerpProgress = (float)(1 - Math.Sqrt(1 - Math.Pow(progress, 2))); // ease in circ
        data.physicsMaterial.dynamicFriction = Mathf.Lerp(data.slideFriction, data.defaultFriction, lerpProgress);
    }
}
