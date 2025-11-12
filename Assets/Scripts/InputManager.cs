using SR.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : PersistentSingleton<InputManager>
{
    public Vector2 moveDirection = Vector2.zero;
    
    #region Delegates and Events
    public delegate void OnMoveDelegate(InputValue value);
    public OnMoveDelegate OnMoveEvent;
    
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
    #endregion
    
    private void Start() //TODO: Add "pressed" bool to the events so that it can differentiate between press and release..
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.defaultActionMap = "Player";
    }

    private void OnMove(InputValue value)
    {
        OnMoveEvent?.Invoke(value);
        moveDirection = value.Get<Vector2>();
    }
    private void OnJump(InputValue value) => OnJumpEvent?.Invoke(value);
    private void OnCrouch(InputValue value) => OnCrouchEvent?.Invoke(value);
    private void OnSlam(InputValue value) => OnSlamEvent?.Invoke(value);
    private void OnInteract(InputValue value) => OnInteractEvent?.Invoke(value);
    private void OnPrimaryAction(InputValue value) => OnPrimaryActionEvent?.Invoke(value);
    private void OnSecondaryAction(InputValue value) => OnSecondaryActionEvent?.Invoke(value);
}