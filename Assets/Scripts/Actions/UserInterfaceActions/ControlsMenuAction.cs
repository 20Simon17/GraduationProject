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
        return true;
    }

    public override void OnBegin(bool bFirstTime)
    {
        
    }

    public override void OnEnd()
    {
        
    }
}