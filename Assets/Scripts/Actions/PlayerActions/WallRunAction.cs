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
    private Vector3 direction;
    private bool exitedEarly;

    public override bool IsDone()
    {
        if (HorizontalVelocity.magnitude <= 0)
        {
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
        if (dataRecord.wallRuns >= data.maxWallRuns || rb.linearVelocity.y <= data.wallRunCancelVerticalVelocity)
        {
            CompleteAction();
            return;
        }

        // TODO: Cancel unless > x speed on entering (check HorizontalVelocity.magnitude)

        if (Vector3.Dot(transform.forward, HorizontalVelocity.normalized) < Vector3.Dot(-transform.forward, HorizontalVelocity.normalized))
        {
            CompleteAction();
            return;
        }
        
        if (dataRecord.rightWallNormal != Vector3.zero)
        {
            if (dataRecord.previousWallRunWasRight && dataRecord.rightWallNormal == dataRecord.previousWallRunNormal)
            {
                CompleteAction();
                return;
            }

            dataRecord.previousWallNormal = dataRecord.rightWallNormal;
            dataRecord.previousWallRunNormal = dataRecord.rightWallNormal;
            dataRecord.previousWallRunWasRight = true;
            direction = GetWallMoveDirection(dataRecord.rightWallNormal);
        }
        else if (dataRecord.leftWallNormal != Vector3.zero)
        {
            if (!dataRecord.previousWallRunWasRight && dataRecord.leftWallNormal == dataRecord.previousWallRunNormal)
            {
                CompleteAction();
                return;
            }

            dataRecord.previousWallNormal = dataRecord.leftWallNormal;
            dataRecord.previousWallRunNormal = dataRecord.leftWallNormal;
            dataRecord.previousWallRunWasRight = false;
            direction = GetWallMoveDirection(dataRecord.leftWallNormal);
        }
        else
        {
            CompleteAction();
            return;
        }

        Vector3 movementVelocity = direction.normalized * HorizontalVelocity.magnitude;

        float extraVerticalVelocity = rb.linearVelocity.y > 0 ? 2 : 0;
        rb.linearVelocity = new Vector3(movementVelocity.x, rb.linearVelocity.y + extraVerticalVelocity, movementVelocity.z);

        cameraActionStack.OnWallRunStateChange(true, dataRecord.previousWallNormal, direction);
        
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
        if (!exitedEarly && dataRecord.wallRuns < data.maxWallRuns)
        {
            dataRecord.wallRuns++;
        }

        cameraActionStack.OnWallRunStateChange(false);

        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        Physics.gravity = data.defaultGravity;
        dataRecord.isWallRunning = false;
        dataRecord.previousWallRunDirection = direction;
    }

    public override void CompleteAction()
    {
        base.CompleteAction();
        exitedEarly = true;
    }
}