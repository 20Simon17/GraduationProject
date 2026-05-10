using UnityEngine;

public class SlamAction : PlayerActionStack.PlayerAction
{
    public SlamAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}

    private bool exitedOnCooldown;

    public override bool IsDone()
    {
        if (playerData.isGrounded)
        {
            return true;
        }
        return ActionCompleted;
    }

    public override void OnBegin(bool bFirstTime)
    {
        if (Time.time - playerData.timeAtLastSlam < staticData.slamCooldown)
        {
            CompleteAction();
            exitedOnCooldown = true;
            return;
        }
        
        if (!playerData.isGrounded)
        {
            playerData.isSlamming = true;
            rb.AddForce(-transform.up * staticData.groundSlamForce, ForceMode.VelocityChange);
        }
        else
        {
            playerData.timeAtLastSlam = Time.time;
        }

        ActionCompleted = true;
    }

    public override void OnEnd()
    {
        if (exitedOnCooldown) return;
        
        playerData.isSlamming = false;
        playerData.timeAtLastSlam = Time.time;
    }
}

// if slamming, no air movement (lose all velocity except downwards) ?
// otherwise, air movement as normal
// gives incentive to avoid the slam as it would otherwise always be used to land faster and gain more speed