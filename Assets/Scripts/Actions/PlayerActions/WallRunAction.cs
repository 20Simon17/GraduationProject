using UnityEngine;

public class WallRunAction : PlayerActionStack.PlayerAction
{
    public WallRunAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData)
        : base(inRb, inTransform, inData) {}

    private Vector3 HorizontalVelocity => new(rb.linearVelocity.x, 0, rb.linearVelocity.z);

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

        Vector3 moveDirection = Vector3.zero;
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
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y - (3 * deltaTime), rb.linearVelocity.z);
        //TODO: Replace hardcoded value with variable, giga testing to make it feel good
    }

    public override void OnEnd()
    {
        if (dataRecord.currentWallRuns < data.maxWallRuns) dataRecord.currentWallRuns++;
        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        dataRecord.isWallRunning = false;
        Physics.gravity = data.defaultGravity;
    }
}