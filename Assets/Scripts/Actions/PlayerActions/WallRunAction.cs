using UnityEngine;

public class WallRunAction : PlayerActionStack.PlayerAction
{
    public WallRunAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData)
        : base(inRb, inTransform, inData) {}

    private Vector3 HorizontalVelocity => new(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    private bool normalWallRun;

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
        if (normalWallRun && dataRecord.rightWallNormal == Vector3.zero && dataRecord.leftWallNormal == Vector3.zero)
        {
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

        // TODO: if speed is towards the same wall the player is looking at, wallrun up and make jump go backwards (out from the wall) <-----------------------------------

        // TODO: max 1 wallrun per side, meaning if the player wallruns on a wall to the right, left is the only remaining option (turn around to stick to same wall)

        // TODO: Wallrun upwards can only happen if velocity.y > 0 and X time has passed since the previous one (to prevent permanent wallclimbing on the same wall)
        // Extra prevention: Maximum of X jumps? that doesn't make sense though if the player still has the speed for it. Maybe just allow it? Unless there's buggy behaviour.

        Vector3 moveDirection = Vector3.zero;
        if (dataRecord.frontWallNormal != Vector3.zero)
        {
            moveDirection = Vector3.Cross(dataRecord.frontWallNormal, -transform.right);
        }
        else if (dataRecord.rightWallNormal != Vector3.zero)
        {
            normalWallRun = true;
            moveDirection = GetWallMoveDirection(dataRecord.rightWallNormal);
        }
        else if (dataRecord.leftWallNormal != Vector3.zero)
        {
            normalWallRun = true;
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