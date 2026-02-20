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

    private bool closingMenu;
    private bool addButtonsOnBegin;

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
            
            pauseMenu.OpenMenu();
            GameManager.Instance.Pause();
        }
        else if (addButtonsOnBegin)
        {
            addButtonsOnBegin = false;
            pauseMenu.ShowButtons();
        }
    }

    public override void OnEnd()
    {
        UnbindEvents();
        
        if (closingMenu)
        {
            Cursor.lockState = CursorLockMode.Locked;
            
            pauseMenu.CloseMenu();
            GameManager.Instance.Resume();
        }
        else
        {
            // animate buttons disappearing here
        }
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
        resumeButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
        controlsButton.onClick.RemoveAllListeners();
        mainMenuButton.onClick.RemoveAllListeners();
    }

    public override void Cancel()
    {
        closingMenu = true;
        CompleteAction();
    }

    private void AddOptionsMenu()
    {
        addButtonsOnBegin = true;
        pauseMenu.HideButtons();
        stack.AddOptionsMenuAction();
    }

    private void AddControlsMenu()
    {
        stack.AddControlsMenuAction();
    }

    private void GoToMainMenu()
    {
        stack.GoToMainMenu();
    }
}