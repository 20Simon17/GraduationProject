using UnityEngine;
using UnityEngine.UI;

public class PauseMenuAction : UserInterfaceActionStack.UserInterfaceAction
{
    private UserInterfaceActionStack stack;
    
    private Button resumeButton;
    private Button optionsButton;
    private Button controlsButton;
    private Button mainMenuButton;
    
    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            GameObject pauseMenu = GameObject.Find("PauseMenu");
            Transform buttonsMenu = pauseMenu.transform.GetChild(2);
            resumeButton = buttonsMenu.GetChild(0).GetComponent<Button>();
            optionsButton = buttonsMenu.GetChild(1).GetComponent<Button>();
            controlsButton = buttonsMenu.GetChild(2).GetComponent<Button>();
            mainMenuButton = buttonsMenu.GetChild(3).GetComponent<Button>();
            
            BindEvents();
        }
    }

    public override void OnEnd()
    {
        UnbindEvents();
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
    
    public void SetStackReference(UserInterfaceActionStack actionStack) => stack = actionStack;
}