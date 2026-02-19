using System;
using UnityEngine;

public class SlideAction : PlayerActionStack.PlayerAction
{
    public SlideAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}
    
    private float slideTime;
    private bool exitedEarly;
    
    public override bool IsDone()
    {
        if (!dataRecord.isGrounded && !dataRecord.isCoyoteTimeActive)
        {
            return true;
        }
        if (rb.linearVelocity.magnitude <= 0)
        {
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
        rb.AddForce(-transform.up * 100, ForceMode.Impulse); //Send the player downwards to stick to the ground
        
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
        slideTime += deltaTime;
        
        float progress = Mathf.Min(slideTime / data.timeUntilMaxFriction, 1);
        data.physicsMaterial.dynamicFriction = (float)(1 - Math.Sqrt(1 - Math.Pow(progress, 2))); // ease in circ
    }
}
