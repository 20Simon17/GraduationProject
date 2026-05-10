using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerData), typeof(Rigidbody))]
public class PlayerActionStack : ActionStack
{
    public abstract class PlayerAction : Action
    {
        protected PlayerAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData)
        {
            rb = inRb;
            transform = inTransform;
            playerData = inData;
            staticData = inData.dataStruct;
        }
    
        public virtual void CompleteAction() => ActionCompleted = true;
    
        public override bool IsDone() => ActionCompleted;
        protected bool ActionCompleted;
    
        protected readonly Rigidbody rb;
        protected PlayerDataRecord playerData;
        protected PlayerDataStruct staticData;
        protected readonly Transform transform;
    }
    
    public delegate void OnGroundedDelegate();
    public OnGroundedDelegate OnGroundedEvent;
    
    // The data for the player
    private PlayerData playerDataComponent;
    public PlayerDataRecord dataRecord;

    [SerializeField] private ParticleSystem speedLinesParticleSystem;
    
    private Rigidbody rb;
    
    private PlayerAction currentAction;
    
    private Vector3 velocityOnPause;
    private Vector3 gravityOnPause;

    private CameraActionStack cameraActionStack;
    
    private bool gameIsQuitting;
    
    private bool slideBufferActive;
    private bool jumpBufferActive;

    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask wallLayerMask;

    private PhysicsMaterial physicsMaterial;
    
    //TODO: For vaulting (if I decide to add it) I could boxcast in front of the player around mid height to see if an edge is there
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        playerDataComponent = GetComponent<PlayerData>();
        dataRecord = playerDataComponent.dataRecord;

        cameraActionStack = FindFirstObjectByType<CameraActionStack>();

        physicsMaterial = GetComponent<CapsuleCollider>().material;
        
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
        
        // Cap player velocity
        if (rb.linearVelocity.magnitude > dataRecord.dataStruct.velocityHardCap)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * dataRecord.dataStruct.velocityHardCap;
        }

        // Set the collider's friction
        physicsMaterial.dynamicFriction = dataRecord.isGrounded ? dataRecord.dataStruct.defaultFriction : 0;

        // TODO: Separate these from the player script
        HandleSpeedLines();
        HandleCameraShake();

        GroundCheck();
        SlopeCheck();

        CheckWallActions();

        HandleCoyoteTime();
        
        if (currentAction != CurrentAction as PlayerAction)
        {
            currentAction = (PlayerAction) CurrentAction;
        }
    }

    private void CheckWallActions()
    {
        if (WallChecks() && dataRecord.CanDoWallAction)
        {
            if (dataRecord.rightWallNormal != Vector3.zero || dataRecord.leftWallNormal != Vector3.zero)
            {
                if (currentAction is WallRunAction || currentAction is WaitAction) return;
                Debug.Log("Wall detected, adding wallrun action");
                ForceAddWallRunAction();
            }
            else if (dataRecord.frontWallNormal != Vector3.zero)
            {
                if (currentAction is WallClimbAction || currentAction is WaitAction) return;
                Debug.Log("Wall detected, adding wallclimb action");
                ForceAddWallClimbAction();
            }
        }
    }

    private void HandleCoyoteTime()
    {
        if (!dataRecord.isCoyoteTimeActive) return;

        dataRecord.coyoteTime += Time.deltaTime;
        if (dataRecord.coyoteTime >= dataRecord.dataStruct.coyoteTimeDuration)
        {
            dataRecord.isCoyoteTimeActive = false;
            dataRecord.coyoteTime = 0;
        }
    }

    private void HandleSpeedLines()
    {
        //TODO: Separate into separate script for VFX handling
        return;
        // Compare our horizontal velocity to the speed requirement for speedlines
        // if (new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude > dataRecord.dataStruct.speedLineSpeedRequirement)
        // {
        //     if (!speedLinesParticleSystem.isPlaying) speedLinesParticleSystem.Play();
        // }
        // else
        // {
        //     if (speedLinesParticleSystem.isPlaying) speedLinesParticleSystem.Stop();
        // }
    }

    private void HandleCameraShake()
    {
        // TODO: Separate into separate script for VFX handling
        if (!dataRecord.allowCameraShake) return;
        if (rb.linearVelocity.y < dataRecord.dataStruct.cameraShakeSpeedRequirement)
        {
            cameraActionStack.ToggleCameraShake(true);
        }
    }

    private void GroundCheck()
    {
        Ray ray = new Ray(transform.position + transform.up * 0.01f, -transform.up);
        if (Physics.SphereCast(ray, 0.5f, out RaycastHit hit, transform.localScale.y / 2 + 0.1f, groundLayerMask))
        {
            if (currentAction is ZiplineAction) return;
            
            if (!dataRecord.isGrounded)
            {
                OnGroundedEvent?.Invoke();
                OnGrounded();
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
    
    private void SlopeCheck()
    {
        Ray ray = new(transform.position + transform.up * 0.01f, -transform.up);
        if (Physics.SphereCast(ray, 0.5f, out RaycastHit hit, transform.localScale.y / 2 + 0.2f, groundLayerMask))
        {
            if (hit.normal != Vector3.up && dataRecord.isGrounded)
            {
                if (dataRecord.isOnSlope) return;
                
                dataRecord.isOnSlope = true;
                dataRecord.slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                dataRecord.slopeNormal = hit.normal;
            }
            else ResetValues();
        }
        else if (dataRecord.isOnSlope) ResetValues();
        return;
        
        void ResetValues()
        {
            dataRecord.isOnSlope = false;
            dataRecord.slopeAngle = 0;
            dataRecord.slopeNormal = Vector3.zero;
        }
    }

    private bool WallChecks()
    {
        float rayOffset = 0.2f;
        float raySidewaysOffset = 0.5f;

        // When attached to a wall, make sure that wall is still there
        if (dataRecord.isWallClimbing && dataRecord.frontWallNormal != Vector3.zero)
        {
            if (LocalCheckWall(dataRecord.frontWallNormal, dataRecord.dataStruct.wallClimbCheckDistance))
            {
                return true;
            }

            dataRecord.frontWallNormal = Vector3.zero;
            return false;
        }
        else if (dataRecord.isWallRunning)
        {
            if (dataRecord.rightWallNormal != Vector3.zero)
            {
                if (LocalCheckWall(dataRecord.rightWallNormal, dataRecord.dataStruct.wallRunCheckDistance))
                {
                    return true;
                }

                dataRecord.rightWallNormal = Vector3.zero;
                return false;
            }
            else if (dataRecord.leftWallNormal != Vector3.zero)
            {
                if (LocalCheckWall(dataRecord.leftWallNormal, dataRecord.dataStruct.wallRunCheckDistance))
                {
                    return true;
                }

                dataRecord.leftWallNormal = Vector3.zero;
                return false;
            }
        }

        Ray[] lRays = LocalCreateRays(-transform.right, transform.forward);
        Ray[] rRays = LocalCreateRays(transform.right, transform.forward);
        Ray[] fRays = LocalCreateRays(transform.forward, transform.right);

        bool returnValue = false;
        for (int i = 0; i < lRays.Length; i++)
        {
            if (Physics.Raycast(fRays[i], out RaycastHit fHit, dataRecord.dataStruct.wallClimbCheckDistance, wallLayerMask))
            {
                dataRecord.frontWallNormal = fHit.normal;
                returnValue = true;
            }
            else dataRecord.frontWallNormal = Vector3.zero;

            if (Physics.Raycast(lRays[i], out RaycastHit lHit, dataRecord.dataStruct.wallRunCheckDistance, wallLayerMask))
            {
                dataRecord.leftWallNormal = lHit.normal;
                returnValue = true;
            }
            else dataRecord.leftWallNormal = Vector3.zero;

            if (Physics.Raycast(rRays[i], out RaycastHit rHit, dataRecord.dataStruct.wallRunCheckDistance, wallLayerMask))
            {
                dataRecord.rightWallNormal = rHit.normal;
                returnValue = true;
            }
            else dataRecord.rightWallNormal = Vector3.zero;
        }
        return returnValue;

        Ray[] LocalCreateRays(Vector3 direction, Vector3 rayOffsetDirection)
        {
            return new Ray[4]
            {
                new(transform.position + rayOffsetDirection * rayOffset + transform.up * rayOffset + direction * raySidewaysOffset, direction),
                new(transform.position + rayOffsetDirection * rayOffset - transform.up * rayOffset + direction * raySidewaysOffset, direction),
                new(transform.position - rayOffsetDirection * rayOffset + transform.up * rayOffset + direction * raySidewaysOffset, direction),
                new(transform.position - rayOffsetDirection * rayOffset - transform.up * rayOffset + direction * raySidewaysOffset, direction)
            };
        }

        bool LocalCheckWall(Vector3 wallNormal, float rayDistance)
        {
            Ray[] wallRays = LocalCreateRays(-wallNormal, Vector3.Cross(-wallNormal, transform.up));
            foreach (Ray ray in wallRays)
            {
                if (Physics.Raycast(ray, rayDistance))
                {
                    return true;
                }
            }
            return false;
        }
    }

    private void OnGrounded()
    {
        dataRecord.isGrounded = true;
        dataRecord.isJumping = false;
                
        dataRecord.canWallRunJump = false;
        dataRecord.canWallClimbJump = false;
        dataRecord.previousWallRunWasRight = false;
        dataRecord.previousWallNormal = Vector3.zero;
        dataRecord.previousWallRunNormal = Vector3.zero;
        dataRecord.previousWallClimbNormal = Vector3.zero;
        
        if (dataRecord.isWallRunning)
        {
            slideBufferActive = false;
            jumpBufferActive = false;
        }
        
        if (slideBufferActive)
        {
            slideBufferActive = false;
            AddSlideAction();
        }
        else if (jumpBufferActive)
        {
            //TODO: Max buffer time (aka store the time of buffering and compare it here)
            Debug.Log("Jump buffered, adding jump action.");
            Debug.Log(dataRecord.isWallRunning);
            jumpBufferActive = false;
            ForceAddJumpAction();
        }
    }
    
    private void CheckJumpActions(InputValue value)
    {
        dataRecord.isHoldingJump = value.isPressed;

        if (currentAction is WaitAction) return;
        
        if (dataRecord.isWallRunning)
        {
            HandleWallRunAction(value);
            return;
        }

        if (dataRecord.isWallClimbing)
        {
            HandleWallClimbAction(value);
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
        
        if (dataRecord.CanJump)
        {
            AddJumpAction(value);
        }
    }
    
    private void AddJumpAction(InputValue value)
    {
        if (!value.isPressed || currentAction is JumpAction) return;
        
        if (currentAction is SlideAction)
        {
            InsertAction(new JumpAction(rb, transform, dataRecord), 1);
            currentAction.CompleteAction();
        }
        else
        {
            ForceAddJumpAction();
        }
    }
    
    private void ForceAddJumpAction()
    {
        if (currentAction is ZiplineAction)
        {
            currentAction.CompleteAction();
        }
        
        PushAction(new JumpAction(rb, transform, dataRecord));
    }
    
    private void HandleWallRunAction(InputValue value)
    {
        if (!value.isPressed) return;

        if (!dataRecord.isGrounded && currentAction is not WallRunAction && currentAction is not WaitAction)
        {
            Debug.Log("Wall run detected, adding wallrun action");

            PushAction(new WallRunAction(rb, transform, dataRecord, cameraActionStack));
            jumpBufferActive = false;
            slideBufferActive = false;
        }
        else if (currentAction is WallRunAction /* && dataRecord.wallRuns < dataRecord.dataStruct.maxWallRuns */)
        {
            // insert the jump action right below the wallrun action so that it executes immediately after the wallrun action finishes
            InsertAction(new JumpAction(rb, transform, dataRecord), 1);

            currentAction.CompleteAction();
            dataRecord.canWallRunJump = true;
            dataRecord.CanJump = true;
        }
    }

    private void ForceAddWallRunAction()
    {
        if (currentAction is not WallRunAction && !dataRecord.isGrounded)
        {
            PushAction(new WallRunAction(rb, transform, dataRecord, cameraActionStack));
            jumpBufferActive = false;
            slideBufferActive = false;
        }
    }

    private void HandleWallClimbAction(InputValue value)
    {
        if (!value.isPressed) return;

        if (!dataRecord.isGrounded && currentAction is not WallClimbAction && currentAction is not WaitAction)
        {
            PushAction(new WallClimbAction(rb, transform, dataRecord));
            jumpBufferActive = false;
            slideBufferActive = false;
        }
        else if (currentAction is WallClimbAction)
        {
            // insert the jump action right below the wallclimb action so that it executes immediately after the wallclimb action finishes
            InsertAction(new JumpAction(rb, transform, dataRecord), 1);
            
            currentAction.CompleteAction();
            dataRecord.canWallClimbJump = true;
            dataRecord.CanJump = true;
        }
    }

    private void ForceAddWallClimbAction()
    {
        if (currentAction is not WallClimbAction && !dataRecord.isGrounded)
        {
            PushAction(new WallClimbAction(rb, transform, dataRecord));
            jumpBufferActive = false;
            slideBufferActive = false;
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
        PushAction(new SlideAction(rb, transform, dataRecord, this));
    }
    
    private void AddSlamAction(InputValue value)
    {
        if (currentAction is WaitAction || currentAction is WallRunAction) return;
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
    
    public void AddWaitingAction(ref System.Action inFinishCondition)
    {
        PushAction(new WaitAction(rb, transform, dataRecord, ref inFinishCondition));
    }
    
    public void AddZiplineAction(Zipline zipline)
    {
        PushAction(new ZiplineAction(rb, transform, dataRecord, zipline));
    }
}