using UnityEngine;
using UnityEngine.InputSystem;

public class CameraActionStack : ActionStack
{
    public abstract class CameraAction : Action
    {
        protected CameraAction(Transform player, Transform camera)
        {
            PlayerTransform = player;
            CameraTransform = camera;
        }
        
        protected float VerticalRotation;
        protected Transform CameraTransform;
        protected Transform PlayerTransform;
        
        public virtual void RotateCamera(Vector2 input) { }
    }
    
    private CameraAction currentAction;

    private Transform playerTransform;
    private Transform cameraTransform;
    
    private bool gameIsQuitting;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        cameraTransform = transform;
        playerTransform = FindFirstObjectByType<PlayerActionStack>().transform;
        
        PushAction(new DefaultCameraAction(playerTransform, cameraTransform));
        
        BindEvents();
    }

    private void BindEvents()
    {
        Application.quitting += QuitGame;
        InputManager.Instance.OnFreeCamEvent += FreeCamToggle;
        InputManager.Instance.OnLookEvent += Look;
    }
    
    private void OnDisable()
    {
        Application.quitting -= QuitGame;
        if (gameIsQuitting) return;
        
        InputManager.Instance.OnFreeCamEvent -= FreeCamToggle;
        InputManager.Instance.OnLookEvent -= Look;
    }
    
    private void QuitGame() => gameIsQuitting = true;

    public override void PushAction(IAction action)
    {
        base.PushAction(action);
        currentAction = action as CameraAction;
    }

    public void UpdateActionStack()
    {
        base.UpdateStack();

        if (currentAction != CurrentAction as CameraAction)
        {
            currentAction = (CameraAction) CurrentAction;
        }
    }

    private void FreeCamToggle(InputValue value)
    {
        if (value.isPressed && CurrentAction.ToString() != "FreeMoveCameraAction")
        {
            PushAction(new FreeMoveCameraAction(playerTransform, cameraTransform));
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
