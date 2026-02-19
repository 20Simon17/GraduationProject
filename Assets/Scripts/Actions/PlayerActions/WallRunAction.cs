using UnityEngine;

public class WallRunAction : PlayerActionStack.PlayerAction
{
    public WallRunAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}

    private Vector3 moveDirection;
    private Vector3 directionToWall;
    
    public override bool IsDone()
    {
        Ray wallRay = new Ray(transform.position, directionToWall);
        if (!Physics.Raycast(wallRay, data.wallRunCheckDistance))
        {
            //return true;
        }
        
        Vector2 horizontalVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude <= 0)
        {
            Debug.Log("No speed during wallrun, forcing jump");
            // force jump here
            return true;
        }
        if (rb.linearVelocity.y <= data.wallRunCancelVerticalVelocity || dataRecord.isGrounded)
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
        }
        
        Ray rRay = new Ray(transform.position, transform.right);
        Ray lRay = new Ray(transform.position, -transform.right);

        if (Physics.Raycast(rRay, out RaycastHit rHit, transform.localScale.y / 2 + 0.1f) &&
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

        else if (Physics.Raycast(lRay, out RaycastHit lHit, transform.localScale.y / 2 + 0.1f) &&
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

        Physics.gravity = data.defaultGravity * data.wallRunGravityMultiplier;
        Vector2 horizontalVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        Vector3 movementVelocity = moveDirection.normalized * horizontalVelocity.magnitude;
        rb.linearVelocity = new Vector3(movementVelocity.x, rb.linearVelocity.y + 5, movementVelocity.z);
        
        data.physicsMaterial.dynamicFriction = 0;
        
        //if y velocity is less than a certain amount, leave unchanged, if it's within a range = set it to 0, if it's above the range, keep it
    }

    public override void OnEnd()
    {
        if (dataRecord.currentWallRuns < data.maxWallRuns) dataRecord.currentWallRuns++;
        data.physicsMaterial.dynamicFriction = data.defaultFriction;
        Physics.gravity = data.defaultGravity;
    }
}