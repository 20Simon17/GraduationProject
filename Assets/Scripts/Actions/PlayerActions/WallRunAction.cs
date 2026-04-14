using UnityEngine;

public class WallRunAction : PlayerActionStack.PlayerAction
{
    public WallRunAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData)
        : base(inRb, inTransform, inData) {}

    private Vector3 HorizontalVelocity => new(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    private bool normalWallRun;

    public override bool IsDone()
    {
        if (normalWallRun && HorizontalVelocity.magnitude <= 0)
        {
            Debug.Log("Wallrun ended due to low horizontal velocity");
            return true;
        }
        if (rb.linearVelocity.y <= data.wallRunCancelVerticalVelocity || dataRecord.isGrounded)
        {
            return true;
        }
        if (normalWallRun && dataRecord.rightWallNormal == Vector3.zero && dataRecord.leftWallNormal == Vector3.zero)
        {
            return true;
        }
        if (!normalWallRun && dataRecord.frontWallNormal == Vector3.zero)
        {
            // Make player climb the edge in front of them in this case?
            return true;
        }
        return ActionCompleted;
    }
    
    public override void OnBegin(bool bFirstTime)
    {
        if (dataRecord.currentWallRuns >= data.maxWallRuns)
        {
            CompleteAction();
            return;
        }

        if (rb.linearVelocity.magnitude > data.maxWallRunEntryVelocity)
        {
            // Perform wall jump
        }

        // TODO: max 1 wallrun per side, meaning if the player wallruns on a wall to the right, left is the only remaining option (turn around to stick to same wall)
        // This doesn't make sense because what if there is a case where the player would wallrun on the inner side of an L shaped wall, then there would be 2 of the same direction
        // wallrun. Instead I should limit it to 1 of the same directional wallrun per same wall. (can't do 2 right wallruns on 1 wall)

        // TODO: Wallrun upwards can only happen if velocity.y > 0 and X time has passed since the previous one (to prevent permanent wallclimbing on the same wall)
        // Extra prevention: Maximum of X jumps? that doesn't make sense though if the player still has the speed for it. Maybe just allow it? Unless there's buggy behaviour.

        dataRecord.previousWallRunWasVertical = false;

        Vector3 moveDirection;
        if (dataRecord.frontWallNormal != Vector3.zero && rb.linearVelocity.y > 0)
        {
            dataRecord.previousWallRunWasVertical = true;
            dataRecord.previousWallNormal = dataRecord.frontWallNormal;

            moveDirection = Vector3.Cross(dataRecord.frontWallNormal, -transform.right);
            rb.linearVelocity = moveDirection * (rb.linearVelocity.magnitude * data.percentageConvertedVelocityOnVerticalWallRun);
        }
        else
        {
            if (dataRecord.rightWallNormal != Vector3.zero)
            {
                dataRecord.previousWallNormal = dataRecord.rightWallNormal;
                normalWallRun = true;
                moveDirection = GetWallMoveDirection(dataRecord.rightWallNormal);
            }
            else if (dataRecord.leftWallNormal != Vector3.zero)
            {
                dataRecord.previousWallNormal = dataRecord.leftWallNormal;
                normalWallRun = true;
                moveDirection = GetWallMoveDirection(dataRecord.leftWallNormal);
            }
            else
            {
                CompleteAction();
                return;
            }

            Vector3 movementVelocity = moveDirection.normalized * HorizontalVelocity.magnitude;

            float extraVerticalVelocity = rb.linearVelocity.y > 0 ? 3 : 0;
            rb.linearVelocity = new Vector3(movementVelocity.x, rb.linearVelocity.y + extraVerticalVelocity, movementVelocity.z);
        }
        
        Physics.gravity = Vector3.zero;
        data.physicsMaterial.dynamicFriction = 0;
        dataRecord.isWallRunning = true;
        return;

        Vector3 GetWallMoveDirection(Vector3 inNormal)
        {
            Vector3 wallDirection = Vector3.Cross(inNormal, transform.up);
            float fDot = Vector3.Dot(HorizontalVelocity, wallDirection);
            float bDot = Vector3.Dot(HorizontalVelocity, -wallDirection);
            return fDot > bDot ? wallDirection : -wallDirection;
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y - (data.wallRunVerticalVelocityLoss * deltaTime), rb.linearVelocity.z);
    }

    public override void OnEnd()
    {
        if (dataRecord.currentWallRuns < data.maxWallRuns && dataRecord.frontWallNormal == Vector3.zero)
        {
            dataRecord.currentWallRuns++;
        }

        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        Physics.gravity = data.defaultGravity;
        dataRecord.isWallRunning = false;
        normalWallRun = false;
    }
}