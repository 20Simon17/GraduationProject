using System;
using UnityEngine;

public class SlideAction : PlayerActionStack.PlayerAction
{
    public SlideAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}
    
    private float slideTime;
    private bool exitedEarly;
    private bool isAboveSlope;
    private Vector3 defaultGravity = Physics.gravity;
    private float slopeGravityMultiplier = 2;
    private bool wasOnSlope = false;
    
    public override bool IsDone()
    {
        if (!dataRecord.isGrounded && !dataRecord.isCoyoteTimeActive && !isAboveSlope)
        {
            Debug.Log("Weren't grounded");
            return true;
        }
        if (rb.linearVelocity.magnitude <= 0)
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
        Vector3 slideDirection = transform.rotation * new Vector3(moveInput.x, 0, moveInput.y); //replaced transform.forward with this for directional slide
        
        if (rb.linearVelocity.magnitude < data.maxRunVelocity)
        {
            rb.linearVelocity =  slideDirection * data.slideSpeed;
        }
        else
        {
            rb.linearVelocity = slideDirection * (rb.linearVelocity.magnitude * data.slideSpeedBoost);
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
            if (!wasOnSlope) wasOnSlope = true;
            
            slideTime = 0;
            data.physicsMaterial.dynamicFriction = data.slideFriction;
            rb.linearVelocity = dataRecord.GetSlopeMoveDirection(rb.linearVelocity) * rb.linearVelocity.magnitude;
            
            // this should only happen if it's a downhill, otherwise maybe add the slope angle to the dynamic friction?
        }
        else
        {
            if (wasOnSlope)
            {
                /*wasOnSlope = false;
                rb.linearVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up).normalized * rb.linearVelocity.magnitude;*/
                // TODO: fix the line above this, this happens because the ground check doesn't detect the slope anymore and it bugs out
            }
            
            slideTime += deltaTime;
        
            float progress = Mathf.Min(slideTime / data.timeUntilMaxFriction, 1);
            float lerpProgress = (float)(1 - Math.Sqrt(1 - Math.Pow(progress, 2))); // ease in circ
            data.physicsMaterial.dynamicFriction = Mathf.Lerp(data.slideFriction, data.defaultFriction, lerpProgress);
        }
    }
}
