using UnityEngine;
using UnityEngine.InputSystem;

public class SlidingState : BaseState
{
    private PlayerSlide _playerSlide;
    private PlayerJump _playerJump;
    
    public override void EnableState(Player player, PlayerData playerData)
    {
        base.EnableState(player, playerData);
        
        _playerSlide = new PlayerSlide(Player.rb, Player.transform, playerData);
        _playerJump = new PlayerJump(Player.rb, Player.transform, playerData);
        
        _playerSlide.StartSlide();
    }

    public override void DisableState()
    {
        _playerSlide.StopSlide();
    }
    
    public override void UpdateState(float fixedDeltaTime)
    {
        base.UpdateState(fixedDeltaTime);
        
        _playerSlide.UpdateSlide();
    }
    
    public override void OnCrouch(InputValue value)
    {
        if (!value.isPressed)
        {
            Player.SwitchState<DefaultState>();
        }
    }

    public override void OnJump(InputValue value)
    {
        if(value.isPressed) _playerJump.Jump();
    }
}
