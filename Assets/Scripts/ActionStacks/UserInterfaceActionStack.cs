using UnityEngine;
using UnityEngine.InputSystem;

public class UserInterfaceActionStack : ActionStack
{
    public abstract class UserInterfaceAction : Action
    {
        protected UserInterfaceAction()
        {
            
        }

        public virtual void CompleteAction() => ActionCompleted = true;

        public override bool IsDone() => ActionCompleted;
        protected bool ActionCompleted;
        
        public virtual void Cancel() => CompleteAction();
    }
    
    private PauseMenu pauseMenu;
    private OptionsMenu optionsMenu;
    private ControlsMenu controlsMenu;

    private UserInterfaceAction currentAction;

    private void Start()
    {
        PushAction(new DefaultUserInterfaceAction());

        pauseMenu = FindAnyObjectByType<PauseMenu>();
        optionsMenu = FindAnyObjectByType<OptionsMenu>();
        controlsMenu = FindAnyObjectByType<ControlsMenu>();
        
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

    public void UpdateActionStack()
    {
        base.UpdateStack();
        
        if (currentAction != CurrentAction as UserInterfaceAction)
        {
            currentAction = (UserInterfaceAction) CurrentAction;
        }
    }

    private void AddPauseMenuAction(InputValue value)
    {
        if (!value.isPressed || GameManager.Instance.IsGamePaused) return;
        if (currentAction is not DefaultUserInterfaceAction) return;
        
        PushAction(new PauseMenuAction(this, pauseMenu));
    }

    public void AddOptionsMenuAction()
    {
        PushAction(new OptionsMenuAction(optionsMenu));
    }

    public void AddControlsMenuAction()
    {
        PushAction(new ControlsMenuAction());
    }

    private void CancelAction(InputValue value)
    {
        if (!value.isPressed || !GameManager.Instance.IsGamePaused) return;
        
        (CurrentAction as UserInterfaceAction)?.Cancel();
    }

    public void GoToMainMenu()
    {
        while (currentAction is not DefaultUserInterfaceAction)
        {
            currentAction?.CompleteAction();
        }
        PushAction(new MainMenuAction());
    }
}