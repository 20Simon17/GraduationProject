using UnityEngine;
using UnityEngine.UI;

public class PauseMenuAction : UserInterfaceActionStack.UserInterfaceAction
{
    public PauseMenuAction(UserInterfaceActionStack inStack, PauseMenu inPauseMenu)
    {
        stack = inStack;
        pauseMenu = inPauseMenu;
    }
    
    private readonly UserInterfaceActionStack stack;
    private readonly PauseMenu pauseMenu;

    private Button resumeButton;
    private Button optionsButton;
    private Button controlsButton;
    private Button mainMenuButton;

    public override bool IsDone()
    {
        return base.IsDone();
    }

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            Cursor.lockState = CursorLockMode.None;
            
            Transform buttonsMenu = pauseMenu.transform.GetChild(2);
            resumeButton = buttonsMenu.GetChild(0).GetComponent<Button>();
            optionsButton = buttonsMenu.GetChild(1).GetComponent<Button>();
            controlsButton = buttonsMenu.GetChild(2).GetComponent<Button>();
            mainMenuButton = buttonsMenu.GetChild(3).GetComponent<Button>();

            BindEvents();
            
            pauseMenu.AnimateMenuOpening();
            GameManager.Instance.Pause();
        }
    }

    public override void OnEnd()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        UnbindEvents();
        
        pauseMenu.AnimateMenuClosing();
        GameManager.Instance.Resume();
    }

    private void BindEvents()
    {
        resumeButton.onClick.AddListener(Cancel);
        optionsButton.onClick.AddListener(AddOptionsMenu);
        controlsButton.onClick.AddListener(AddControlsMenu);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void UnbindEvents()
    {
        resumeButton.onClick.RemoveListener(Cancel);
        optionsButton.onClick.RemoveListener(AddOptionsMenu);
        controlsButton.onClick.RemoveListener(AddControlsMenu);
        mainMenuButton.onClick.RemoveListener(GoToMainMenu);
    }

    public override void Cancel()
    {
        CompleteAction();
    }

    private void AddOptionsMenu()
    {
        stack.PushAction(new OptionsMenuAction());
    }

    private void AddControlsMenu()
    {
        stack.PushAction(new ControlsMenuAction());
    }

    private void GoToMainMenu()
    {
        stack.GoToMainMenu();
    }
}