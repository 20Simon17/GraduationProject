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

        public override void OnEnd()
        {
            base.OnEnd();

            dataRecord.dataStruct = data;
        }

        public void CompleteAction() => actionCompleted = true;

        public override bool IsDone() => actionCompleted;
        protected bool actionCompleted;

        protected readonly Rigidbody rb;
        protected PlayerDataRecord dataRecord;
        protected PlayerDataStruct data;
        protected readonly Transform transform;
    }
    
    private PlayerData playerDataComponent;
    private PlayerDataRecord playerDataRecord;
    private PlayerDataStruct playerDataStruct;
    
    private Rigidbody rb;
    
    private PlayerAction currentAction;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        playerDataComponent = GetComponent<PlayerData>();
        playerDataRecord = playerDataComponent.dataRecord;
        
        PushAction(new DefaultMovementAction(rb, transform, playerDataRecord));
        
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
        
        // TODO: How can I make this less expensive?
        if (currentAction != CurrentAction as PlayerAction)
        {
            currentAction = (PlayerAction) CurrentAction;
        }
    }

    private void GroundCheck()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, transform.localScale.y / 2 + 0.1f))
        {
            if (hit.transform.CompareTag("Ground") && playerDataRecord.dataStruct.isTouchingGround)
            {
                playerDataRecord.dataStruct.isGrounded = true;
            }
        }
        
        Debug.DrawRay(ray.origin, ray.direction * (transform.localScale.y + 0.1f), Color.darkRed);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            playerDataRecord.dataStruct.isTouchingGround = true;
            playerDataRecord.dataStruct.isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            playerDataRecord.dataStruct.isTouchingGround = false;
            playerDataRecord.dataStruct.isGrounded = false;
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
        
        PushAction(new JumpAction(rb, transform, playerDataRecord));
    }

    private void AddSlideAction(InputValue value)
    {
        if (value.isPressed && currentAction is not SlideAction)
        {
            PushAction(new SlideAction(rb, transform, playerDataRecord));
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
            PushAction(new SlamAction(rb, transform, playerDataRecord));
        }
        else if (!value.isPressed && currentAction is SlamAction)
        {
            currentAction.CompleteAction();
        }
    }
}
