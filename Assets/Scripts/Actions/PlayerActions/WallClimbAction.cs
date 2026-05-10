using System.Data.Common;
using UnityEngine;

public class WallClimbAction : PlayerActionStack.PlayerAction
{
    public WallClimbAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData)
        : base(inRb, inTransform, inData) {}

    public override bool IsDone()
    {
        if (rb.linearVelocity.y <= staticData.wallRunVerticalCancelVelocity || playerData.isGrounded)
        {
            return true;
        }
        if (playerData.frontWallNormal == Vector3.zero)
        {
            Debug.Log("Wallclimb ended due to no front wall");
            // Make player climb the edge in front of them in this case?
            return true;
        }
        return ActionCompleted;
    }
    
    public override void OnBegin(bool bFirstTime)
    {
        // TODO: Wallclimb can only happen if velocity.y > 0 and X time has passed since the previous one (to prevent permanent wallclimbing on the same wall)
        // Extra prevention: Maximum of X jumps? that doesn't make sense though if the player still has the speed for it. Maybe just allow it? Unless there's buggy behaviour.

        // set a min speed lower than 0 to be a bit more lenient
        if (rb.linearVelocity.y <= staticData.wallClimbRequiredEntryVelocity || InputManager.Instance.moveDirection.y <= 0 || playerData.frontWallNormal == playerData.previousWallClimbNormal)
        {
            CompleteAction();
            return;
        }

        Vector3 moveDirection;
        playerData.previousWallNormal = playerData.frontWallNormal;
        playerData.previousWallClimbNormal = playerData.frontWallNormal;

        moveDirection = Vector3.Cross(playerData.frontWallNormal, -transform.right);
        float convertedSpeed = rb.linearVelocity.magnitude * staticData.percentageConvertedVelocityOnWallClimb;

        if (convertedSpeed < staticData.wallClimbMinimumVelocity)
        {
            convertedSpeed = staticData.wallClimbMinimumVelocity;
        }

        rb.linearVelocity = moveDirection * convertedSpeed;
        
        Physics.gravity = Vector3.zero;
        staticData.physicsMaterial.dynamicFriction = 0;
        playerData.isWallClimbing = true;;
    }

    public override void OnUpdate(float deltaTime)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y - (staticData.wallClimbVerticalVelocityLoss * deltaTime), rb.linearVelocity.z);
    }

    public override void OnEnd()
    {
        staticData.physicsMaterial.dynamicFriction = staticData.defaultFriction;
        Physics.gravity = staticData.defaultGravity;
        playerData.isWallClimbing = false;
    }
}