using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionStack : ActionStack
{
    public abstract class PlayerAction : Action
    {
        protected PlayerAction(Rigidbody inRb, Transform inTransform, PlayerData inData)
        {
            rb = inRb;
            transform = inTransform;
            data = inData;
        }

        public virtual void CompleteAction() => actionCompleted = true;

        public override bool IsDone() => actionCompleted;
        protected bool actionCompleted;

        protected readonly Rigidbody rb;
        protected readonly PlayerData data;
        protected readonly Transform transform;
    }
    
    [SerializeField] private PlayerDataSO playerDataObject;
    private PlayerData playerData;
    
    private Rigidbody rb;
    
    private PlayerAction currentAction;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (playerDataObject)
        {
            playerData = playerDataObject.playerData;
        }
        
        PushAction(new DefaultMovementAction(rb, transform, playerData));
        
        BindEvents();
    }

    private void BindEvents()
    {
        InputManager.Instance.OnJumpEvent += AddJumpAction;
        InputManager.Instance.OnCrouchEvent += AddSlideAction;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnJumpEvent -= AddJumpAction;
        InputManager.Instance.OnCrouchEvent -= AddSlideAction;
    }

    protected override void Update()
    {
        base.Update();

        // TODO: How can I make this less expensive?
        if (currentAction != CurrentAction as PlayerAction)
        {
            currentAction = (PlayerAction) CurrentAction;
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
}
