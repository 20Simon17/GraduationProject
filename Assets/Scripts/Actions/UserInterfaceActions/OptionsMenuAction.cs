using UnityEngine;

public class OptionsMenuAction : UserInterfaceActionStack.UserInterfaceAction
{
    public OptionsMenuAction(OptionsMenu inOptionsMenu)
    {
        optionsMenu = inOptionsMenu;
    }

    private readonly OptionsMenu optionsMenu;

    public override bool IsDone()
    {
        return base.IsDone();
    }

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
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
        
    }

    private void UnbindEvents()
    {
        
    }
}