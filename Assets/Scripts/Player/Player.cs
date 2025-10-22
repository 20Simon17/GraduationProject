using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private BaseState currentState; 
    
    // Sending the inputs to the current state
    private void OnMove(InputValue value) => currentState.OnMove(value);
    private void OnJump(InputValue value) => currentState.OnJump(value);
    private void OnCrouch(InputValue value) => currentState.OnCrouch(value);
    private void OnSlam(InputValue value) => currentState.OnSlam(value);
    private void OnInteract(InputValue value) => currentState.OnInteract(value);
    private void OnPrimaryAction(InputValue value) => currentState.OnPrimaryAction(value);
    private void OnSecondaryAction(InputValue value) => currentState.OnSecondaryAction(value);
    
    private void SwitchState(BaseState newState)
    {
        currentState?.DisableState();
        currentState = newState;
        currentState?.EnableState();
    }
}