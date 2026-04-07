using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

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

    [SerializeField] private LayerMask groundLayerMask;
    
    //TODO: For vaulting (if I decide to add it) I could boxcast in front of the player around mid height to see if an edge is there
    
    private void Start()
    {
        GetComponent<CapsuleCollider>().material = dataRecord.dataStruct.physicsMaterial;
        rb = GetComponent<Rigidbody>();
        
        playerDataComponent = GetComponent<PlayerData>();
        dataRecord = playerDataComponent.dataRecord;
        
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
        SlopeCheck();
        CanWallRun();

        if (rb.linearVelocity.magnitude > dataRecord.dataStruct.velocityHardCap)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * dataRecord.dataStruct.velocityHardCap;
        }
        
        if (dataRecord.isCoyoteTimeActive)
        {
            dataRecord.coyoteTime += Time.deltaTime;

            if (dataRecord.coyoteTime >= dataRecord.dataStruct.coyoteTimeDuration)
            {
                dataRecord.isCoyoteTimeActive = false;
                dataRecord.coyoteTime = 0;
            }
        }
        
        if (currentAction != CurrentAction as PlayerAction)
        {
            currentAction = (PlayerAction) CurrentAction;
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
                dataRecord.isGrounded = true;
                
                dataRecord.canWallRunJump = false;
                dataRecord.currentWallRuns = 0;
                
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
        Ray ray = new Ray(transform.position + transform.up * 0.01f, -transform.up);
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
    
    private void CheckJumpActions(InputValue value)
    {
        if (currentAction is WaitAction) return;
        
        if (currentAction is WallRunAction)
        {
            HandleWallRunAction(value);
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
        else if (CanWallRun()) HandleWallRunAction(value);
    }
    
    private bool CanWallRun()
    {
        Vector3 rHalfExtents = transform.forward * 0.3f + transform.up * 0.5f + transform.right * 0.2f;
        Vector3 lHalfExtents = transform.forward * 0.3f + transform.up * 0.5f - transform.right * 0.2f;
        Vector3 rightCenter = transform.position + transform.right * 0.2f;
        Vector3 leftCenter  = transform.position - transform.right * 0.2f;
        
        //TODO: when done debugging, compress this function.
        
        bool returnValue = false;
        Color leftDebugColor = Color.darkRed;
        Color rightDebugColor = Color.darkRed;

        if (Physics.BoxCast(rightCenter, rHalfExtents, transform.right, Quaternion.identity, dataRecord.dataStruct.wallRunCheckDistance))
        {
            returnValue = true;
            rightDebugColor = Color.green;
        }
        
        if (Physics.BoxCast(leftCenter, lHalfExtents, -transform.right, Quaternion.identity, dataRecord.dataStruct.wallRunCheckDistance))
        {
            returnValue = true;
            leftDebugColor = Color.green;
        }
        
        ExtDebug.DrawBoxCastBox(rightCenter, rHalfExtents, Quaternion.identity, transform.right, dataRecord.dataStruct.wallRunCheckDistance, rightDebugColor);
        ExtDebug.DrawBoxCastBox(leftCenter, lHalfExtents, Quaternion.identity, -transform.right, dataRecord.dataStruct.wallRunCheckDistance, leftDebugColor);

        return returnValue;
    }
    
    private void AddJumpAction(InputValue value)
    {
        if (!value.isPressed || currentAction is JumpAction) return;
        
        ForceAddJumpAction();
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
        
        if (currentAction is not WallRunAction && !dataRecord.isGrounded)
        {
            PushAction(new WallRunAction(rb, transform, dataRecord));
        }
        else if (currentAction is WallRunAction && dataRecord.currentWallRuns < dataRecord.dataStruct.maxWallRuns)
        {
            Debug.Log("Completing wall run and forcing a jump.");
            currentAction.CompleteAction();
            dataRecord.canWallRunJump = true;
            dataRecord.CanJump = true;

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
        PushAction(new SlideAction(rb, transform, dataRecord, this));
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
    
    public void AddWaitingAction(ref System.Action inFinishCondition)
    {
        PushAction(new WaitAction(rb, transform, dataRecord, ref inFinishCondition));
    }
    
    public void AddZiplineAction(Zipline zipline)
    {
        PushAction(new ZiplineAction(rb, transform, dataRecord, zipline));
    }
    
    public void CompleteCurrentAction() => currentAction.CompleteAction();
}