using System;
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
    // on scroll click pressed, switch to free move camera action
    // on scroll click released, switch back to default camera action
    
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

    private void OnDisable()
    {
        InputManager.Instance.OnFreeCamEvent -= FreeCamToggle;
        InputManager.Instance.OnLookEvent -= Look;
    }

    private void FreeCamToggle(InputValue value)
    {
        if (!value.isPressed) return;
        if (CurrentAction.ToString() != "FreeMoveCameraAction")
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
        if (currentAction == null) return;
        currentAction.RotateCamera(value.Get<Vector2>());
    }
}
