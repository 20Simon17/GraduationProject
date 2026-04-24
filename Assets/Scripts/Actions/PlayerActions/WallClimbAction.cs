using UnityEngine;

public class WallClimbAction : PlayerActionStack.PlayerAction
{
    public WallClimbAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData)
        : base(inRb, inTransform, inData) {}

    public override bool IsDone()
    {
        if (rb.linearVelocity.y <= data.wallRunVerticalCancelVelocity || dataRecord.isGrounded)
        {
            return true;
        }
        if (dataRecord.frontWallNormal == Vector3.zero)
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

        if (rb.linearVelocity.y <= 0 || InputManager.Instance.moveDirection == Vector2.zero)
        {
            CompleteAction();
            return;
        }

        Vector3 moveDirection;
        dataRecord.previousWallNormal = dataRecord.frontWallNormal;

        moveDirection = Vector3.Cross(dataRecord.frontWallNormal, -transform.right);
        rb.linearVelocity = moveDirection * (rb.linearVelocity.magnitude * data.percentageConvertedVelocityOnWallClimb);
        
        Physics.gravity = Vector3.zero;
        data.physicsMaterial.dynamicFriction = 0;
        dataRecord.isWallClimbing = true;;
    }

    public override void OnUpdate(float deltaTime)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y - (data.wallClimbVerticalVelocityLoss * deltaTime), rb.linearVelocity.z);
    }

    public override void OnEnd()
    {
        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        Physics.gravity = data.defaultGravity;
        dataRecord.isWallClimbing = false;
    }
}