using System;
using UnityEngine;

public class SlideAction : PlayerActionStack.PlayerAction
{
    public SlideAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}

    
    private float slideTime;
    
    public override bool IsDone()
    {
        if (!dataRecord.IsGrounded && !dataRecord.IsCoyoteTimeActive)
        {
            return true;
        }
        if (rb.linearVelocity.magnitude <= 0)
        {
            return true;
        }
        return actionCompleted;
    }

    public override void OnBegin(bool bFirstTime)
    {
        // TODO: Make the slide go in the direction of movement input, not just forward.

        if (!dataRecord.IsGrounded) return;
        
        dataRecord.dataStruct.isSliding = true;
        data.physicsMaterial.dynamicFriction = data.slideFriction;
        
        slideTime = 0;
        
        transform.localScale = new Vector3(transform.localScale.x, data.slidePlayerScaleY, transform.localScale.z);
        rb.AddForce(-transform.up * 100, ForceMode.Impulse); //Send the player downwards to stick to the ground
        
        dataRecord.dataStruct.timeAtLastSlide = Time.time;
            
        if (rb.linearVelocity.magnitude < data.maxRunVelocity)
        {
            rb.linearVelocity = transform.forward * data.slideSpeed;
        }
        else
        {
            rb.linearVelocity = transform.forward * (rb.linearVelocity.magnitude * data.slideSpeedBoost);
        }
    }

    public override void OnEnd()
    {
        dataRecord.dataStruct.isSliding = false;
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
