using UnityEngine;

public class SlamAction : PlayerActionStack.PlayerAction
{
    public SlamAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}

    public override bool IsDone()
    {
        return actionCompleted ? actionCompleted : data.isGrounded;
    }

    public override void OnBegin(bool bFirstTime)
    {
        if (!data.isGrounded)
        {
            data.isSlamming = true;
            rb.AddForce(-transform.up * data.groundSlamForce, ForceMode.VelocityChange);
        }
        else
        {
            data.timeAtLastSlam = Time.time;
        }
    }

    public override void OnEnd()
    {
        data.isSlamming = false;
        //Start cooldown for when the next slam could be done?
    }
}

// if slamming, no air movement (lose all velocity except downwards) ?
// otherwise, air movement as normal
// gives incentive to avoid the slam as it would otherwise always be used to land faster and gain more speed