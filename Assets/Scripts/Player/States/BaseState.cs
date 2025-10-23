using UnityEngine;
using UnityEngine.InputSystem;

public class BaseState
{
    protected Player Player;
    
    public virtual void EnableState(Player player, PlayerData playerData)
    {
        Player = player;
    }

    public virtual void DisableState()
    {
        
    }
    
    public virtual void OnMove(InputValue value)
    {

    }

    public virtual void OnJump(InputValue value)
    {
        
    }
    
    public virtual void OnCrouch(InputValue value)
    {
        /*if (value.isPressed)
        {
            Player.SwitchState<SlidingState>();
        }*/
    }
    
    public virtual void OnSlam(InputValue value)
    {
        //Player.SwitchState<SlammingState>();
    }
    
    public virtual void OnInteract(InputValue value)
    {
        
    }

    public virtual void OnPrimaryAction(InputValue value)
    {
        
    }
    
    public virtual void OnSecondaryAction(InputValue value)
    {
        
    }

    public virtual void UpdateState()
    {
        
    }
}