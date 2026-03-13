using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerData))]
public class PlayerActionStack : ActionStack
{
    public abstract class PlayerAction : Action
    {
        protected PlayerAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData)
        {
            rb = inRb;
            transform = inTransform;
            dataRecord = inData;
            data = inData.dataStruct;
        }

        public virtual void CompleteAction() => ActionCompleted = true;

        public override bool IsDone() => ActionCompleted;
        protected bool ActionCompleted;

        protected readonly Rigidbody rb;
        protected PlayerDataRecord dataRecord;
        protected PlayerDataStruct data;
        protected readonly Transform transform;
    }

    public delegate void OnGroundedDelegate();
    public OnGroundedDelegate OnGroundedEvent;
    
    // The data for the player
    private PlayerData playerDataComponent;
    public PlayerDataRecord dataRecord;
    
    private Rigidbody rb;
    
    private PlayerAction currentAction;

    private Vector3 velocityOnPause;
    private Vector3 gravityOnPause;

    private bool gameIsQuitting;

    private bool slideBufferActive;
    private bool jumpBufferActive;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        playerDataComponent = GetComponent<PlayerData>();
        dataRecord = playerDataComponent.dataRecord;
        
        GetComponent<CapsuleCollider>().material = dataRecord.dataStruct.physicsMaterial;
        
        PushAction(new DefaultMovementAction(rb, transform, dataRecord));
        
        BindEvents();
    }

    private void BindEvents()
    {
        Application.quitting += QuitGame;
        InputManager.Instance.OnJumpEvent += CheckJumpActions;
        InputManager.Instance.OnCrouchEvent += AddSlideAction;
        InputManager.Instance.OnSlamEvent += AddSlamAction;
    }

    private void OnDisable()
    {
        Application.quitting -= QuitGame;
        if (gameIsQuitting) return;
        
        InputManager.Instance.OnJumpEvent -= CheckJumpActions;
        InputManager.Instance.OnCrouchEvent -= AddSlideAction;
        InputManager.Instance.OnSlamEvent -= AddSlamAction;
    }

    private void QuitGame() => gameIsQuitting = true;

    public void UpdateActionStack()
    {
        base.UpdateStack();
        
        GroundCheck();

        if (dataRecord.isCoyoteTimeActive)
        {
            dataRecord.coyoteTime += Time.deltaTime;

            if (dataRecord.coyoteTime >= dataRecord.dataStruct.coyoteTimeDuration)
            {
                dataRecord.isCoyoteTimeActive = false;
                dataRecord.coyoteTime = 0;
            }
        }
        
        /* Wall run check rays
        Ray rRay = new Ray(transform.position, transform.right);
        Ray lRay = new Ray(transform.position, -transform.right);
        Debug.DrawRay(rRay.origin, rRay.direction * dataRecord.dataStruct.wallRunCheckDistance, Color.darkRed);
        Debug.DrawRay(lRay.origin, lRay.direction * dataRecord.dataStruct.wallRunCheckDistance, Color.darkRed);*/
        
        if (currentAction != CurrentAction as PlayerAction)
        {
            currentAction = (PlayerAction) CurrentAction;
        }
    }

    private void GroundCheck()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        Physics.SphereCast(ray, 0.5f, out RaycastHit hit, transform.localScale.y / 2 + 0.01f);

        if (hit.collider && hit.transform.CompareTag("Ground") && currentAction is not ZiplineAction)
        {
            if (!dataRecord.isGrounded)
            {
                OnGroundedEvent?.Invoke();
                dataRecord.isGrounded = true;

                if (slideBufferActive)
                {
                    slideBufferActive = false;
                    AddSlideAction();
                }
                else if (jumpBufferActive)
                {
                    jumpBufferActive = false;
                    ForceAddJumpAction();
                }
            }
            
            if (hit.normal != Vector3.up && !dataRecord.isOnSlope)
            {
                dataRecord.isOnSlope = true;
                dataRecord.slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            }
            else
            {
                dataRecord.isOnSlope = false;
                dataRecord.slopeAngle = 0;
            }
            
            if (dataRecord.hasJumped && dataRecord.timeAtLastJump != 0 && Time.time - dataRecord.timeAtLastJump > 0.1f)
            {
                dataRecord.hasJumped = false;
                dataRecord.timeAtLastJump = 0;
            }

            if (dataRecord.isCoyoteTimeActive)
            {
                dataRecord.isCoyoteTimeActive = false;
                dataRecord.coyoteTime = 0;
            }
        }
        else if (dataRecord.isGrounded)
        {
            dataRecord.isGrounded = false;
            dataRecord.coyoteTime = 0;
            dataRecord.isCoyoteTimeActive = true;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            dataRecord.isTouchingGround = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            dataRecord.isTouchingGround = false;
        }
    }

    private void CheckJumpActions(InputValue value)
    {
        if (currentAction is WaitAction) return;
        
        if (currentAction is WallRunAction)
        {
            AddWallRunAction(value);
            return;
        }
        
        if (!dataRecord.isGrounded && value.isPressed)
        {
            jumpBufferActive = true;
            
            if (slideBufferActive) slideBufferActive = false;
        }
        else if (jumpBufferActive && !value.isPressed)
        {
            jumpBufferActive = false;
            return;
        }
        
        // Touching ground is true when touching walls as well
        if (dataRecord.CanJump)
        {
            AddJumpAction(value);
        }
        else if (CanWallRun()) AddWallRunAction(value);
    }

    private bool CanWallRun()
    {
        return false;
        /*Ray rRay = new Ray(transform.position, transform.right);
        Ray lRay = new Ray(transform.position, -transform.right);
            
        return Physics.Raycast(rRay, out RaycastHit rHit, dataRecord.dataStruct.wallRunCheckDistance) && rHit.transform.CompareTag("Ground") ||
               Physics.Raycast(lRay, out RaycastHit lHit, dataRecord.dataStruct.wallRunCheckDistance) && lHit.transform.CompareTag("Ground");*/
    }

    private void AddJumpAction(InputValue value)
    {
        if (!value.isPressed || currentAction is JumpAction) return;
        
        ForceAddJumpAction();
    }

    private void ForceAddJumpAction()
    {
        //ClearAllActions();

        if (currentAction is ZiplineAction)
        {
            currentAction.CompleteAction();
        }
        
        PushAction(new JumpAction(rb, transform, dataRecord));
    }

    private void AddWallRunAction(InputValue value)
    {
        if (value.isPressed && currentAction is not WallRunAction && !dataRecord.isGrounded)
        {
            PushAction(new WallRunAction(rb, transform, dataRecord));
        }
        else if (!value.isPressed && currentAction is WallRunAction)
        {
            Debug.Log("Completing wallrun and forcing a jump.");
            dataRecord.canWallRunJump = true;
            currentAction.CompleteAction();
            ForceAddJumpAction();
        }
    }
    
    private void AddSlideAction(InputValue value)
    {
        if (currentAction is WaitAction) return;

        if (!dataRecord.isGrounded && value.isPressed)
        {
            slideBufferActive = true;
            
            if (jumpBufferActive) jumpBufferActive = false;
        }
        else if (slideBufferActive && !value.isPressed)
        {
            slideBufferActive = false;
        }
        else
        {
            if (value.isPressed && currentAction is not SlideAction)
            {
                AddSlideAction();
            }
            else if (!value.isPressed && currentAction is SlideAction)
            {
                currentAction.CompleteAction();
            }
        }
    }

    private void AddSlideAction()
    {
        PushAction(new SlideAction(rb, transform, dataRecord));
    }
    
    private void AddSlamAction(InputValue value)
    {
        if (currentAction is WaitAction) return;
        if (value.isPressed && currentAction is not SlamAction)
        {
            PushAction(new SlamAction(rb, transform, dataRecord));
        }
    }

    public void Pause()
    {
        gravityOnPause = Physics.gravity;
        velocityOnPause = rb.linearVelocity;
        rb.linearVelocity = Vector3.zero;
        Physics.gravity = Vector3.zero;
    }

    public void Resume()
    {
        rb.linearVelocity = velocityOnPause;
        Physics.gravity = gravityOnPause;
    }

    public void ClearAllActions()
    {
        foreach (IAction action in Stack.Where(action => action is not DefaultMovementAction))
        {
            (action as PlayerAction)?.CompleteAction();
        }
    }

    public void AddWaitingAction()
    {
        PushAction(new WaitAction(rb, transform, dataRecord));
    }
    
    public void CompleteCurrentAction() => currentAction.CompleteAction();
    
    public void AddZiplineAction(Zipline zipline)
    {
        PushAction(new ZiplineAction(rb, transform, dataRecord, zipline));
    }
}
