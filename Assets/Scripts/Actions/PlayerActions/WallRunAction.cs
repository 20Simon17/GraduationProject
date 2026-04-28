using UnityEditor.Toolbars;
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

    public override bool IsDone()
    {
        if (playerData.isGrounded) return true;
        if (rb.linearVelocity.y <= staticData.wallRunVerticalCancelVelocity)
        {
            Debug.Log("Wallrun ended due to low vertical velocity");
            return true;
        }
        if (playerData.rightWallNormal == Vector3.zero && playerData.leftWallNormal == Vector3.zero) return true;
        return ActionCompleted;
    }
    
    public override void OnBegin(bool bFirstTime)
    {
        if (!CanEnter()) return;

        if (playerData.rightWallNormal != Vector3.zero)
        {
            if (playerData.previousWallRunWasRight && playerData.rightWallNormal == playerData.previousWallRunNormal)
            {
                Debug.Log("Did not start wallrun due to same wall");
                CompleteAction();
                return;
            }
            
            playerData.previousWallNormal = playerData.rightWallNormal;
            playerData.previousWallRunNormal = playerData.rightWallNormal;
            playerData.previousWallRunWasRight = true;
            direction = LocalGetWallMoveDirection(playerData.rightWallNormal);
        }
        else if (playerData.leftWallNormal != Vector3.zero)
        {
            if (!playerData.previousWallRunWasRight && playerData.leftWallNormal == playerData.previousWallRunNormal)
            {
                //TODO: Can enter the same wall again if X time has passed
                Debug.Log("Did not start wallrun due to same wall");
                CompleteAction();
                return;
            }

            playerData.previousWallNormal = playerData.leftWallNormal;
            playerData.previousWallRunNormal = playerData.leftWallNormal;
            playerData.previousWallRunWasRight = false;
            direction = LocalGetWallMoveDirection(playerData.leftWallNormal);
        }
        else
        {
            Debug.Log("Did not start wallrun due to no wall");
            CompleteAction();
            return;
        }

        Vector3 hVelocity = direction.normalized * HorizontalVelocity.magnitude;
        rb.linearVelocity = new Vector3(hVelocity.x, rb.linearVelocity.y, hVelocity.z);

        cameraActionStack.OnWallRunStateChange(true, playerData.previousWallNormal, direction);
        
        Physics.gravity = Vector3.zero;

        playerData.isWallRunning = true;
        playerData.isHoldingJump = false;
        return;

        Vector3 LocalGetWallMoveDirection(Vector3 inNormal)
        {
            Vector3 wallDirection = Vector3.Cross(inNormal, transform.up).normalized;
            float fDot = Vector3.Dot(HorizontalVelocity.normalized, wallDirection);
            float bDot = Vector3.Dot(HorizontalVelocity.normalized, -wallDirection);
            return fDot > bDot ? wallDirection : -wallDirection;
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y - (staticData.wallRunVerticalVelocityLoss * deltaTime), rb.linearVelocity.z);
    }

    public override void OnEnd()
    {
        cameraActionStack.OnWallRunStateChange(false);

        Physics.gravity = staticData.defaultGravity;
        playerData.isWallRunning = false;
        playerData.previousWallRunDirection = direction;
    }

    private bool CanEnter()
    {
        //TODO: require speed towards the wall as an entry condition
        if (rb.linearVelocity.y <= staticData.wallRunVerticalCancelVelocity)
        {
            CompleteAction();
            return false;
        }
        //TODO: If the player starts with very low velocity, accelerate up towards a default velocity
        if (HorizontalVelocity.magnitude < staticData.wallRunHorizontalEntryCancelVelocity)
        {
            Debug.Log("Did not start wallrun due to low horizontal velocity");
            CompleteAction();
            return false;
        }

        if (Vector3.Dot(transform.forward, HorizontalVelocity.normalized) < Vector3.Dot(-transform.forward, HorizontalVelocity.normalized))
        {
            Debug.Log("Did not start wallrun due to incorrect direction");
            CompleteAction();
            return false;
        }
        return true;
    }
}