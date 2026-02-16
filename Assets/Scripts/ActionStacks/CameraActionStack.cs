using UnityEngine;
using UnityEngine.InputSystem;

public class CameraActionStack : ActionStack
{
    public abstract class CameraAction : Action
    {
        protected float VerticalRotation;
        protected Transform CameraTransform;
        public virtual void RotateCamera(Vector2 input) { }
    }
    
    private CameraAction currentAction;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        InputManager.Instance.OnFreeCamEvent += FreeCamToggle;
        InputManager.Instance.OnLookEvent += Look;
        
        PushAction(new DefaultCameraAction());
    }

    public override void PushAction(IAction action)
    {
        base.PushAction(action);
        currentAction = action as CameraAction;
    }

    public void UpdateActionStack()
    {
        base.UpdateStack();

        // TODO: How can I make this less expensive?
        if (currentAction != CurrentAction as CameraAction)
        {
            currentAction = (CameraAction) CurrentAction;
        }
    }

    private void OnDisable()
    {
        InputManager.Instance.OnFreeCamEvent -= FreeCamToggle;
        InputManager.Instance.OnLookEvent -= Look;
    }

    private void FreeCamToggle(InputValue value)
    {
        if (value.isPressed && CurrentAction.ToString() != "FreeMoveCameraAction")
        {
            PushAction(new FreeMoveCameraAction());
        }
        else
        {
            (currentAction as FreeMoveCameraAction)?.SetIsDone(true);
        }
    }

    private void Look(InputValue value)
    {
        currentAction?.RotateCamera(value.Get<Vector2>());
    }
}
