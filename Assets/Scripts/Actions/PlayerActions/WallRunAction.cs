using UnityEngine;

public class WallRunAction : PlayerActionStack.PlayerAction
{
    public WallRunAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData)
        : base(inRb, inTransform, inData) {}

    private Vector3 moveDirection;
    private Vector3 HorizontalVelocity => new(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    
    // TODO: If the player is wallrunning and reaches a corner, then check if the player is looking
    // towards the wall's normal (roughly opposite of the normal), then continue the wallrunning on the other side of the wall?  (probably waaaaaay over kill for now)

    public override bool IsDone()
    {
        if (HorizontalVelocity.magnitude <= 0) return true;
        if (rb.linearVelocity.y <= data.wallRunCancelVerticalVelocity || dataRecord.isGrounded)
        {
            Debug.Log("Falling too fast or were grounded");
            return true;
        }
        if (dataRecord.rightWallNormal == Vector3.zero && dataRecord.leftWallNormal == Vector3.zero) return true;
        return ActionCompleted;
    }
    
    public override void OnBegin(bool bFirstTime)
    {
        if (dataRecord.currentWallRuns >= data.maxWallRuns)
        {
            Debug.Log("Can't do more wallruns before landing");
            CompleteAction();
            return;
        }

        if (rb.linearVelocity.magnitude > data.maxWallRunEntryVelocity)
        {
            // Perform wall jump
        }

        if (dataRecord.rightWallNormal != Vector3.zero)
        {
            moveDirection = GetWallMoveDirection(dataRecord.rightWallNormal);
        }
        else if (dataRecord.leftWallNormal != Vector3.zero)
        {
            moveDirection = GetWallMoveDirection(dataRecord.leftWallNormal);
        }

        Vector3 movementVelocity = moveDirection.normalized * HorizontalVelocity.magnitude;

        float extraVerticalVelocity = rb.linearVelocity.y > 0 ? 3 : 0;
        rb.linearVelocity = new Vector3(movementVelocity.x, rb.linearVelocity.y + extraVerticalVelocity, movementVelocity.z);

        Physics.gravity = Vector3.zero;
        data.physicsMaterial.dynamicFriction = 0;
        return;

        Vector3 GetWallMoveDirection(Vector3 inNormal)
        {
            Vector3 wallDirection = Vector3.Cross(inNormal, transform.up);
            
            //TODO: Make this take into account the players velocity's direction, not forward.
            if (Vector3.Dot(HorizontalVelocity, wallDirection) > Vector3.Dot(HorizontalVelocity, -wallDirection))
            {
                return wallDirection;
            }
            else return -wallDirection;
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        //TODO: Replace hardcoded value with variable, giga testing to make it feel good
        rb.AddForce(-transform.up * (9.81f * deltaTime), ForceMode.Force);
    }

    public override void OnEnd()
    {
        if (dataRecord.currentWallRuns < data.maxWallRuns) dataRecord.currentWallRuns++;
        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        Physics.gravity = data.defaultGravity;
    }
}