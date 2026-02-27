using UnityEngine;

public class ControlsMenuAction : UserInterfaceActionStack.UserInterfaceAction
{
    public ControlsMenuAction(ControlsMenu inControlsMenu)
    {
        controlsMenu = inControlsMenu;
    }
    
    private readonly ControlsMenu controlsMenu;
    
    public override bool IsDone()
    {
        return base.IsDone();
    }

    public override void OnBegin(bool bFirstTime)
    {
        controlsMenu.OpenMenu();
    }

    public override void OnEnd()
    {
        controlsMenu.CloseMenu();
    }
}