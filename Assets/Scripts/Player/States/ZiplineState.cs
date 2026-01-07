using UnityEngine.InputSystem;

public class ZiplineState : BaseState
{
    private PlayerJump _playerJump;
    
    public override void EnableState(Player player, PlayerData playerData)
    {
        base.EnableState(player, playerData);
        
        _playerJump = new PlayerJump(Player.rb, Player.transform, playerData);
    }

    public override void DisableState()
    {
    }
    
    public override void UpdateState(float fixedDeltaTime)
    {
        base.UpdateState(fixedDeltaTime);
    }

    public override void OnCrouch(InputValue value)
    {
        Player.SwitchState<DefaultState>();
    }

    public override void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            _playerJump.Jump();
            Player.SwitchState<DefaultState>();
        }
    }
}