using UnityEngine;

public class PlayerActionStack : ActionStack
{
    public abstract class PlayerAction : Action
    {
        protected PlayerAction() {}
        protected PlayerAction(Rigidbody inRb, Transform inTransform, PlayerData inData)
        {
            rb = inRb;
            transform = inTransform;
            data = inData;
        }
        
        protected readonly Rigidbody rb;
        protected readonly PlayerData data;
        protected readonly Transform transform;
    }
    
    private PlayerAction currentAction;
    
    protected override void Update()
    {
        base.Update();

        // TODO: How can I make this less expensive?
        if (currentAction != CurrentAction as PlayerAction)
        {
            currentAction = (PlayerAction) CurrentAction;
        }
    }

    protected override void UpdateActions()
    {
        // do we have actions?
        if (IsEmpty) return;

        // new action?
        while (currentAction == null && m_actionStack.Count > 0)
        {
            // set the current action
            currentAction = m_actionStack[0];

            // call OnBegin
            bool bFirstTime = !m_firstTimeActions.Contains(currentAction);
            m_firstTimeActions.Add(currentAction);
            currentAction.OnBegin(bFirstTime);

            // did OnBegin push or remove another action?
            if (currentAction != null)
            {
                if (m_actionStack.Count > 0 && 
                    currentAction != m_actionStack[0])
                {
                    currentAction = null;
                    UpdateActions();
                    return;
                }
            }
        }

        // call OnUpdate
        if (currentAction != null)
        {
            // update it!
            currentAction.OnUpdate();

            // are we still the current action?
            if (m_actionStack.Count > 0 && currentAction == m_actionStack[0])
            {
                // are we done?
                if (currentAction.IsDone())
                {
                    m_actionStack.RemoveAt(0);
                    currentAction.OnEnd();
                    m_firstTimeActions.Remove(currentAction);
                    currentAction = null;
                }
            }
            else currentAction = null;
        }
    }
}
