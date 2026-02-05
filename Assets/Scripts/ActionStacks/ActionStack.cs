using System.Collections.Generic;
using UnityEngine;

public class ActionStack : MonoBehaviour
{
    #region Definitions
    public interface IAction
    {
        void OnBegin(bool bFirstTime);
        void OnUpdate();
        void OnEnd();
        bool IsDone();
    }

    public abstract class Action : IAction
    {
        public virtual bool IsDone() { return true; }
        public virtual void OnBegin(bool bFirstTime) { }
        public virtual void OnEnd() { }
        public virtual void OnUpdate() { }

        public override string ToString()
        {
            return GetType().Name;
        }
    }

    public abstract class ActionBehavior : MonoBehaviour, IAction
    {
        public virtual bool IsDone() { return true; }
        public virtual void OnBegin(bool bFirstTime) { }
        public virtual void OnEnd() { }
        public virtual void OnUpdate() { }

        public override string ToString()
        {
            return GetType().Name;
        }
    }

    public abstract class ActionObject : ScriptableObject, IAction
    {
        public virtual bool IsDone() { return true; }
        public virtual void OnBegin(bool bFirstTime) { }
        public virtual void OnEnd() { }
        public virtual void OnUpdate() { }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
    #endregion
    
    private List<IAction>       m_actionStack = new List<IAction>();
    private HashSet<IAction>    m_firstTimeActions = new HashSet<IAction>();
    private IAction             m_currentAction;

    private static ActionStack  sm_main;

    #region Properties

    public List<IAction> Stack => m_actionStack;

    public virtual IAction CurrentAction => m_currentAction;

    public bool IsEmpty => m_currentAction == null && m_actionStack.Count == 0;

    public static ActionStack Main
    {
        get
        {
            if (sm_main == null && Application.isPlaying)
            {
                GameObject go = new GameObject("MainActionStack");
                DontDestroyOnLoad(go);
                sm_main = go.AddComponent<ActionStack>();
            }

            return sm_main;
        }
    }

    #endregion

    public virtual void PushAction(IAction action)
    {
        if (action == null) return;
        
        // is the action already on the stack?
        m_actionStack.RemoveAll(a => a == action);

        // add to top of stack
        m_actionStack.Insert(0, action);

        // reset current action
        if (m_currentAction != null && m_currentAction != action)
        {
            m_currentAction = null;
        }
    }

    protected virtual void Update()
    {
        UpdateActions();
    }

    protected virtual void UpdateActions()
    {
        // do we have actions?
        if (IsEmpty) return;

        // new action?
        while (m_currentAction == null && m_actionStack.Count > 0)
        {
            // set the current action
            m_currentAction = m_actionStack[0];

            // call OnBegin
            bool bFirstTime = !m_firstTimeActions.Contains(m_currentAction);
            m_firstTimeActions.Add(m_currentAction);
            m_currentAction.OnBegin(bFirstTime);

            // did OnBegin push or remove another action?
            if (m_currentAction != null)
            {
                if (m_actionStack.Count > 0 && 
                    m_currentAction != m_actionStack[0])
                {
                    m_currentAction = null;
                    UpdateActions();
                    return;
                }
            }
        }

        // call OnUpdate
        if (m_currentAction != null)
        {
            // update it!
            m_currentAction.OnUpdate();

            // are we still the current action?
            if (m_actionStack.Count > 0 && m_currentAction == m_actionStack[0])
            {
                // are we done?
                if (m_currentAction.IsDone())
                {
                    m_actionStack.RemoveAt(0);
                    m_currentAction.OnEnd();
                    m_firstTimeActions.Remove(m_currentAction);
                    m_currentAction = null;
                }
            }
            else m_currentAction = null;
        }
    }
}
