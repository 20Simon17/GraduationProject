using System;

[Serializable]
public record PlayerDataRecord
{
    public bool IsGrounded
    {
        get => dataStruct.isGrounded;
        set => dataStruct.isGrounded = value;
    }

    public bool IsTouchingGround
    {
        get => dataStruct.isTouchingGround;
        set => dataStruct.isTouchingGround = value;
    }
    
    public bool IsCoyoteTimeActive
    {
        get => dataStruct.isCoyoteTimeActive;
        set => dataStruct.isCoyoteTimeActive = value;
    }
    
    public PlayerDataStruct dataStruct;
}