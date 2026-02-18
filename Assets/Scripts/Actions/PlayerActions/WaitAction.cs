using UnityEngine;

public class WaitAction : PlayerActionStack.PlayerAction
{
    public WaitAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}
}