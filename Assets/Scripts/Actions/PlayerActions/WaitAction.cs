using System;
using UnityEngine;

public class WaitAction : PlayerActionStack.PlayerAction
{
    public WaitAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData, ref Action inFinishCondition)
        : base(inRb, inTransform, inData)
    {
        inFinishCondition += OnEventFinished;
        FinishConditionEvent = inFinishCondition;
    }
    
    private event Action FinishConditionEvent;
    private bool eventHasFinished;

    public override bool IsDone() => ActionCompleted ||eventHasFinished;

    private void OnEventFinished() => eventHasFinished = true;

    public override void OnEnd() => FinishConditionEvent -= OnEventFinished;
}