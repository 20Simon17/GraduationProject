using UnityEngine;

public class SlamAction : PlayerActionStack.PlayerAction
{
    public SlamAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}

    private bool exitedOnCooldown;

    public override bool IsDone()
    {
        if (dataRecord.isGrounded)
        {
            return true;
        }
        return ActionCompleted;
    }

    public override void OnBegin(bool bFirstTime)
    {
        if (Time.time - dataRecord.timeAtLastSlam < data.slamCooldown)
        {
            CompleteAction();
            exitedOnCooldown = true;
            return;
        }
        
        if (!dataRecord.isGrounded)
        {
            dataRecord.isSlamming = true;
            rb.AddForce(-transform.up * data.groundSlamForce, ForceMode.VelocityChange);
        }
        else
        {
            dataRecord.timeAtLastSlam = Time.time;
        }
    }

    public override void OnEnd()
    {
        if (exitedOnCooldown) return;
        
        dataRecord.isSlamming = false;
        dataRecord.timeAtLastSlam = Time.time;
    }
}

// if slamming, no air movement (lose all velocity except downwards) ?
// otherwise, air movement as normal
// gives incentive to avoid the slam as it would otherwise always be used to land faster and gain more speed