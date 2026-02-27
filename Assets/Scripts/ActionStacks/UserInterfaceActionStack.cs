using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    private bool gameIsQuitting;

    private void Start()
    {
        // Get references
        pauseMenu = FindAnyObjectByType<PauseMenu>();
        optionsMenu = FindAnyObjectByType<OptionsMenu>();
        controlsMenu = FindAnyObjectByType<ControlsMenu>();
        
        PushAction(new DefaultUserInterfaceAction());
        
        BindEvents();
    }

    private void BindEvents()
    {
        Application.quitting += QuitGame;
        InputManager.Instance.OnPauseEvent += AddPauseMenuAction;
        InputManager.Instance.OnCancelEvent += CancelAction;
    }

    private void OnDisable()
    {
        Application.quitting -= QuitGame;
        if (gameIsQuitting) return; //TODO: Add a "returning to main menu" exit condition too, add the bool in the game manager
        
        InputManager.Instance.OnPauseEvent -= AddPauseMenuAction;
        InputManager.Instance.OnCancelEvent -= CancelAction;
    }

    private void QuitGame() => gameIsQuitting = true;
    
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
        PushAction(new ControlsMenuAction(controlsMenu));
    }

    private void CancelAction(InputValue value)
    {
        if (!value.isPressed || !GameManager.Instance.IsGamePaused) return;
        
        currentAction?.Cancel();
    }

    public void GoToMainMenu()
    {
        /*while (currentAction is not DefaultUserInterfaceAction)
        {
            currentAction?.CompleteAction();
        }*/
        //PushAction(new MainMenuAction());

        GameManager.Instance.Resume();
        SceneManager.LoadScene(0);
    }
}