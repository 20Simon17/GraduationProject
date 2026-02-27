using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuAction : UserInterfaceActionStack.UserInterfaceAction
{
    public OptionsMenuAction(OptionsMenu inOptionsMenu)
    {
        optionsMenu = inOptionsMenu;
    }

    private readonly OptionsMenu optionsMenu;
    
    private Button closeButton;

    public override bool IsDone()
    {
        return base.IsDone();
    }

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            Transform buttonsMenu = optionsMenu.transform.GetChild(2);
            closeButton = buttonsMenu.GetChild(3).GetComponent<Button>();
            
            BindEvents();
            
            optionsMenu.OpenMenu();
            optionsMenu.ShowButtons();
        }
    }

    public override void OnEnd()
    {
        UnbindEvents();
        
        optionsMenu.CloseMenu();
        optionsMenu.HideButtons();
    }
    
    private void BindEvents()
    {
        closeButton.onClick.AddListener(Cancel);
    }

    private void UnbindEvents()
    {
        closeButton.onClick.RemoveAllListeners();
    }
}