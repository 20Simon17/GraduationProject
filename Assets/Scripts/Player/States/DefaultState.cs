using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultState : BaseState
{
    private PlayerMovement _playerMovement;
    private PlayerJump _playerJump;
    
    public override void EnableState(Player player, PlayerData playerData)
    {
        base.EnableState(player, playerData);
        
        _playerMovement = new PlayerMovement(Player.rb, Player.transform, playerData);
        _playerJump = new PlayerJump(Player.rb, Player.transform, playerData);
    }

    public override void DisableState()
    {
        base.DisableState();
    }
    
    public override void UpdateState()
    {
        base.UpdateState();
        
        _playerMovement.UpdateMovement();
    }
    
    public override void OnMove(InputValue value)
    {
        base.OnMove(value);
        
        Vector2 moveInput = value.Get<Vector2>();
    }
    
    public override void OnJump(InputValue value)
    {
        base.OnJump(value);
        
        if (value.isPressed)
        {
            _playerJump.Jump();
        }
    }
}
