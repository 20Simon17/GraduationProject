using UnityEngine.InputSystem;

public class UserInterfaceActionStack : ActionStack
{
    public abstract class UserInterfaceAction : Action
    {
        protected UserInterfaceAction()
        {
            
        }

        public virtual void CompleteAction() => actionCompleted = true;

        public override bool IsDone() => actionCompleted;
        protected bool actionCompleted;
        
        public virtual void Cancel() => CompleteAction();
    }

    private void Start()
    {
        PushAction(new DefaultUserInterfaceAction());

        BindEvents();
    }

    private void BindEvents()
    {
        InputManager.Instance.OnPauseEvent += AddPauseMenuAction;
        InputManager.Instance.OnCancelEvent += CancelAction;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnPauseEvent -= AddPauseMenuAction;
        InputManager.Instance.OnCancelEvent -= CancelAction;
    }

    private void AddPauseMenuAction(InputValue value)
    {
        PushAction(new PauseMenuAction());
        (CurrentAction as PauseMenuAction)?.SetStackReference(this);
    }

    private void CancelAction(InputValue value)
    {
        (CurrentAction as UserInterfaceAction)?.Cancel();
    }

    public void GoToMainMenu()
    {
        while (!(CurrentAction is DefaultUserInterfaceAction))
        {
            (CurrentAction as UserInterfaceAction)?.CompleteAction();
        }
        PushAction(new MainMenuAction());
    }
}