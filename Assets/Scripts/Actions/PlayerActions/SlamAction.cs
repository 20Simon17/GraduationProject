using UnityEngine;

public class SlamAction : PlayerActionStack.PlayerAction
{
    public SlamAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}

    public override bool IsDone()
    {
        if (dataRecord.IsGrounded)
        {
            return true;
        }
        return actionCompleted;
    }

    public override void OnBegin(bool bFirstTime)
    {
        //check difference between Time.time and timeAtLastSlam, as a form of cooldown for the slam
        
        if (!dataRecord.IsGrounded)
        {
            dataRecord.dataStruct.isSlamming = true;
            rb.AddForce(-transform.up * data.groundSlamForce, ForceMode.VelocityChange);
        }
        else
        {
            dataRecord.dataStruct.timeAtLastSlam = Time.time;
        }
    }

    public override void OnEnd()
    {
        dataRecord.dataStruct.isSlamming = false;
        dataRecord.dataStruct.timeAtLastSlam = Time.time;
    }
}

// if slamming, no air movement (lose all velocity except downwards) ?
// otherwise, air movement as normal
// gives incentive to avoid the slam as it would otherwise always be used to land faster and gain more speed