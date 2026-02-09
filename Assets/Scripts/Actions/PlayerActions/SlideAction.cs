using UnityEngine;

public class SlideAction : PlayerActionStack.PlayerAction
{
    public SlideAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}

    public override void OnBegin(bool bFirstTime)
    {
        data.isSliding = true;
        data.physicsMaterial.dynamicFriction = data.slideFriction;
        transform.localScale = new Vector3(transform.localScale.x, data.slidePlayerScaleY, transform.localScale.z);

        if (!data.isGrounded)
        {
            actionCompleted = true;
            return;
        }
            
        rb.AddForce(-transform.up * 100, ForceMode.Impulse); //Send the player downwards to stick to the ground
        data.timeAtLastSlide = Time.time;
            
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
        Debug.Log("Ended slide");
        
        data.isSliding = false;
        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        transform.localScale = new Vector3(transform.localScale.x, data.defaultPlayerScaleY, transform.localScale.z);
        
        base.OnEnd();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (rb.linearVelocity.magnitude < data.slideFallOfThreshold && rb.linearVelocity.magnitude > data.counterForceSpeedThreshold)
        {
            if(rb.linearVelocity.magnitude < 1f)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                return;
            }
            
            Vector3 relativeVelocity = transform.InverseTransformPoint(rb.linearVelocity);
            rb.AddForce(-relativeVelocity.normalized * data.slideFallOfForce, ForceMode.Acceleration);
        }
        else if (rb.linearVelocity.magnitude <= 0)
        {
            actionCompleted = true;
        }
    }
}
