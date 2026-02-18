using UnityEngine;

public class WaitingAction : PlayerActionStack.PlayerAction
{
    public WaitingAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}
}
