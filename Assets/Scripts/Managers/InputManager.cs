using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : Singleton<InputManager>
{
    public Vector2 moveDirection = Vector2.zero;

    private PlayerInput playerInput;
    private AsyncOperationHandle<InputActionAsset> inputActionHandle;

    private bool isPaused;
    
    #region Delegates and Events
    public delegate void OnMoveDelegate(InputValue value);
    public OnMoveDelegate OnMoveEvent;
    
    public delegate void OnLookDelegate(InputValue value);
    public OnLookDelegate OnLookEvent;
    
    public delegate void OnJumpDelegate(InputValue value);
    public OnJumpDelegate OnJumpEvent;
    
    public delegate void OnCrouchDelegate(InputValue value);
    public OnCrouchDelegate OnCrouchEvent;
    
    public delegate void OnSlamDelegate(InputValue value);
    public OnSlamDelegate OnSlamEvent;
    
    public delegate void OnInteractDelegate(InputValue value);
    public OnInteractDelegate OnInteractEvent;
    
    public delegate void OnPrimaryActionDelegate(InputValue value);
    public OnPrimaryActionDelegate OnPrimaryActionEvent;
    
    public delegate void OnSecondaryActionDelegate(InputValue value);
    public OnSecondaryActionDelegate OnSecondaryActionEvent;
    
    public delegate void OnFreeCamDelegate(InputValue value);
    public OnFreeCamDelegate OnFreeCamEvent;
    
    public delegate void OnPauseDelegate(InputValue value);
    public OnPauseDelegate OnPauseEvent;
    
    public delegate void OnCancelDelegate(InputValue value);
    public OnCancelDelegate OnCancelEvent;
    #endregion
    
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.defaultActionMap = "Player";
        
        inputActionHandle = Addressables.LoadAssetAsync<InputActionAsset>("Assets/Inputs/PlayerInputMapping.inputactions");
        inputActionHandle.Completed += InputActionsLoaded;
    }

    private void OnDisable()
    {
        inputActionHandle.Completed -= InputActionsLoaded;
    }

    private void InputActionsLoaded(AsyncOperationHandle<InputActionAsset> inputActionHandle)
    {
        if (inputActionHandle.Status == AsyncOperationStatus.Succeeded)
        {
            playerInput.actions = inputActionHandle.Result;
        }
        else Debug.LogWarning("Failed to load input actions asset");
    }

    private void OnMove(InputValue value)
    {
        OnMoveEvent?.Invoke(value);
        moveDirection = value.Get<Vector2>();
    }

    private void OnLook(InputValue value) => OnLookEvent?.Invoke(value);
    private void OnJump(InputValue value) => OnJumpEvent?.Invoke(value);
    private void OnCrouch(InputValue value) => OnCrouchEvent?.Invoke(value);
    private void OnSlam(InputValue value) => OnSlamEvent?.Invoke(value);
    private void OnInteract(InputValue value) => OnInteractEvent?.Invoke(value);
    private void OnPrimaryAction(InputValue value) => OnPrimaryActionEvent?.Invoke(value);
    private void OnSecondaryAction(InputValue value) => OnSecondaryActionEvent?.Invoke(value);
    private void OnFreeCam(InputValue value) => OnFreeCamEvent?.Invoke(value);
    private void OnPause(InputValue value) => OnPauseEvent?.Invoke(value);
    
    private void OnCancel(InputValue value) => OnCancelEvent?.Invoke(value);

    public void Pause()
    {
        playerInput.SwitchCurrentActionMap("UI");
    }

    public void Resume()
    {
        playerInput.SwitchCurrentActionMap("Player");
    }
    
    public void SwitchActionMap(string actionMapName)
    {
        playerInput.SwitchCurrentActionMap(actionMapName);
    }
    
    public void SwitchToDefaultActionMap()
    {
        playerInput.SwitchCurrentActionMap(playerInput.defaultActionMap);
    }
}