using UnityEngine;

public class WallRunAction : PlayerActionStack.PlayerAction
{
    public WallRunAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData, CameraActionStack inCameraActionStack)
        : base(inRb, inTransform, inData)
    {
        cameraActionStack = inCameraActionStack;
    }

    private Vector3 HorizontalVelocity => new(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    private CameraActionStack cameraActionStack;

    public override bool IsDone()
    {
        if (HorizontalVelocity.magnitude <= 0)
        {
            Debug.Log("Wallrun ended due to low horizontal velocity");
            return true;
        }
        if (rb.linearVelocity.y <= data.wallRunCancelVerticalVelocity || dataRecord.isGrounded)
        {
            return true;
        }
        if (dataRecord.rightWallNormal == Vector3.zero && dataRecord.leftWallNormal == Vector3.zero)
        {
            return true;
        }
        return ActionCompleted;
    }
    
    public override void OnBegin(bool bFirstTime)
    {
        if (dataRecord.currentWallRuns >= data.maxWallRuns || rb.linearVelocity.y <= data.wallRunCancelVerticalVelocity)
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
        dataRecord.previousWallRunWasVertical = false;

        Vector3 moveDirection;
        if (dataRecord.rightWallNormal != Vector3.zero)
        {
            dataRecord.previousWallNormal = dataRecord.rightWallNormal;
            moveDirection = GetWallMoveDirection(dataRecord.rightWallNormal);
        }
        else if (dataRecord.leftWallNormal != Vector3.zero)
        {
            dataRecord.previousWallNormal = dataRecord.leftWallNormal;
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

        cameraActionStack.OnWallRunStateChange(true, dataRecord.previousWallNormal, moveDirection);
        Debug.Log("wallrun is going in direction " + moveDirection);

        //TODO: FIX ISSUE WITH WALLRUN GOING THE WRONG DIRECTION
        
        Physics.gravity = Vector3.zero;
        data.physicsMaterial.dynamicFriction = 0;
        dataRecord.isWallRunning = true;
        return;

        Vector3 GetWallMoveDirection(Vector3 inNormal)
        {
            Vector3 wallDirection = Vector3.Cross(inNormal, transform.up).normalized;
            float fDot = Vector3.Dot(HorizontalVelocity.normalized, wallDirection);
            float bDot = Vector3.Dot(HorizontalVelocity.normalized, -wallDirection);
            return fDot > bDot ? wallDirection : -wallDirection;
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y - (data.wallRunVerticalVelocityLoss * deltaTime), rb.linearVelocity.z);
    }

    public override void OnEnd()
    {
        if (dataRecord.currentWallRuns < data.maxWallRuns)
        {
            dataRecord.currentWallRuns++;
        }

        cameraActionStack.OnWallRunStateChange(false);

        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        Physics.gravity = data.defaultGravity;
        dataRecord.isWallRunning = false;
    }
}