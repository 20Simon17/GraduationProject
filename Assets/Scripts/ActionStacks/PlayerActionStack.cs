using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionStack : ActionStack
{
    public abstract class PlayerAction : Action
    {
        protected PlayerAction(Rigidbody inRb, Transform inTransform, PlayerDataStruct inData)
        {
            rb = inRb;
            transform = inTransform;
            data = inData;
        }

        public void CompleteAction() => actionCompleted = true;

        public override bool IsDone() => actionCompleted;
        protected bool actionCompleted;

        protected readonly Rigidbody rb;
        protected PlayerDataStruct data;
        protected readonly Transform transform;
    }
    
    private PlayerData playerDataComponent;
    private PlayerDataStruct playerData;
    
    private Rigidbody rb;
    
    private PlayerAction currentAction;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerDataComponent = GetComponent<PlayerData>();

        playerData = playerDataComponent.PlayerDataStruct;
        
        PushAction(new DefaultMovementAction(rb, transform, playerData));
        
        BindEvents();
    }

    private void BindEvents()
    {
        InputManager.Instance.OnJumpEvent += AddJumpAction;
        InputManager.Instance.OnCrouchEvent += AddSlideAction;
        InputManager.Instance.OnSlamEvent += AddSlamAction;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnJumpEvent -= AddJumpAction;
        InputManager.Instance.OnCrouchEvent -= AddSlideAction;
        InputManager.Instance.OnSlamEvent -= AddSlamAction;
    }

    protected override void Update()
    {
        base.Update();
        
        GroundCheck();
        
        Debug.Log(playerData.isGrounded);
        
        // TODO: How can I make this less expensive?
        if (currentAction != CurrentAction as PlayerAction)
        {
            currentAction = (PlayerAction) CurrentAction;
        }
    }

    private void GroundCheck()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        Physics.Raycast(ray, out RaycastHit hit, transform.localScale.y / 2 + 0.1f);
        
        Debug.DrawRay(ray.origin, ray.direction * (transform.localScale.y + 0.1f), Color.darkRed);
        
        if (hit.collider != null && hit.transform.CompareTag("Ground") && playerData.isTouchingGround)
        {
            playerData.isGrounded = true;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            playerData.isTouchingGround = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            playerData.isTouchingGround = false;
        }
    }

    private void AddJumpAction(InputValue value)
    {
        if (!value.isPressed || currentAction is JumpAction) return;
        
        //clear every other action except for default movement action
        foreach (IAction action in Stack.Where(action => action is not DefaultMovementAction))
        {
            (action as PlayerAction)?.CompleteAction();
        }
        
        // if current action is zipline action, force a jump through jump action
        // maybe a bool? data.forceJump = true; if (forceJump) perform jump regardless of other conditions.
        
        PushAction(new JumpAction(rb, transform, playerData));
    }

    private void AddSlideAction(InputValue value)
    {
        if (value.isPressed && currentAction is not SlideAction)
        {
            PushAction(new SlideAction(rb, transform, playerData));
        }
        else if (!value.isPressed && currentAction is SlideAction)
        {
            currentAction.CompleteAction();
        }
    }
    
    private void AddSlamAction(InputValue value)
    {
        if (value.isPressed && currentAction is not SlamAction)
        {
            PushAction(new SlamAction(rb, transform, playerData));
        }
        else if (!value.isPressed && currentAction is SlamAction)
        {
            currentAction.CompleteAction();
        }
    }
}
