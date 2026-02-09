using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultState : BaseState
{
    private PlayerMovement _playerMovement;
    private PlayerJump _playerJump;
    
    public override void EnableState(Player player, PlayerDataStruct playerData)
    {
        base.EnableState(player, playerData);
        
        _playerMovement = new PlayerMovement(Player.rb, Player.transform, playerData);
        _playerJump = new PlayerJump(Player.rb, Player.transform, playerData);
    }
    
    public override void UpdateState(float fixedDeltaTime)
    {
        base.UpdateState(fixedDeltaTime);
        
        _playerMovement.UpdateMovement(fixedDeltaTime);
    }
    
    /*public override void OnMove(InputValue value)
    {
        base.OnMove(value);
        
        Vector2 moveInput = value.Get<Vector2>();
        _playerMovement.SetMoveDirection(moveInput);
        
        Debug.Log(moveInput);
    }*/
    
    public override void OnJump(InputValue value)
    {
        base.OnJump(value);
        
        //check for wallrunning or walljumping first
        //raycast to the players right and -right to see if there is a wall there
        
        if (value.isPressed)
        {
            _playerJump.Jump();
        }
    }

    public override void OnCrouch(InputValue value)
    {
        Player.SwitchState<SlidingState>(); // switch to slide state when implemented
    }

    public override void OnSlam(InputValue value)
    {
        Player.SwitchState<DefaultState>(); // switch to slam state when implemented
    }
}