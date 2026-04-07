using UnityEngine;

public class WallRunAction : PlayerActionStack.PlayerAction
{
    public WallRunAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}

    private Vector3 moveDirection;
    private Vector3 directionToWall;
    
    //TODO: If the player is wallrunning and reaches a corner, then check if the player is looking
    // towards the wall's normal (roughly opposite of the normal), then continue the wallrunning on the other side of the wall?  (probably waaaaaay over kill for now)
    
    public override bool IsDone()
    {
        Vector2 horizontalVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude <= 0) return true;
        if (rb.linearVelocity.y <= data.wallRunCancelVerticalVelocity || dataRecord.isGrounded)
        {
            Debug.Log("Falling too fast or were grounded");
            return true;
        }
        return ActionCompleted;
    }
    
    public override void OnBegin(bool bFirstTime)
    {
        if (dataRecord.currentWallRuns >= data.maxWallRuns)
        {
            Debug.Log("Can't do more wallruns before landing");
            CompleteAction();
        }

        if (rb.linearVelocity.magnitude > data.maxWallRunEntryVelocity)
        {
            // Perform wall jump
        }
        
        // TODO: Store the left and right wall normal values in the data record, don't cast here. If no wall = Vector3.zero.
        Ray rRay = new Ray(transform.position + transform.right * 0.2f, transform.right);
        Ray lRay = new Ray(transform.position - transform.right * 0.2f, -transform.right);

        if (Physics.Raycast(rRay, out RaycastHit rHit, data.wallRunCheckDistance) &&
            rHit.transform.CompareTag("Ground"))
        {
            directionToWall = -rHit.normal;
            
            Vector3 wallDirection = Vector3.Cross(rHit.normal, transform.up);

            if (Vector3.Dot(transform.forward, wallDirection) > 
                Vector3.Dot(transform.forward, -wallDirection))
            {
                moveDirection = wallDirection;
            }
            else moveDirection = -wallDirection;
        }

        else if (Physics.Raycast(lRay, out RaycastHit lHit, data.wallRunCheckDistance) &&
            lHit.transform.CompareTag("Ground"))
        {
            directionToWall = -lHit.normal;
            
            Vector3 wallDirection = Vector3.Cross(rHit.normal, transform.up);

            if (Vector3.Dot(transform.forward, wallDirection) > 
                Vector3.Dot(transform.forward, -wallDirection))
            {
                moveDirection = wallDirection;
            }
            else moveDirection = -wallDirection;
        }

        Vector2 horizontalVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        Vector3 movementVelocity = moveDirection.normalized * horizontalVelocity.magnitude;
        rb.linearVelocity = new Vector3(movementVelocity.x, rb.linearVelocity.y + 5, movementVelocity.z);

        Physics.gravity = Vector3.zero;
        data.physicsMaterial.dynamicFriction = 0;
    }

    public override void OnUpdate(float deltaTime)
    {
        //TODO: Replace "1" with variable, giga testing to make it feel good (maybe just steal gravity = 9.81)
        rb.AddForce(-transform.up * (1 * deltaTime), ForceMode.Force);
    }

    public override void OnEnd()
    {
        if (dataRecord.currentWallRuns < data.maxWallRuns) dataRecord.currentWallRuns++;
        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        Physics.gravity = data.defaultGravity;
    }
}