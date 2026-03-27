using UnityEngine;

public class InventoryMenuAction : UserInterfaceActionStack.UserInterfaceAction
{
    public InventoryMenuAction(InventoryMenu inInventoryMenu)
    {
        inventoryMenu = inInventoryMenu;
    }

    private InventoryMenu inventoryMenu;
    
    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            
            inventoryMenu.BindEvents();
            inventoryMenu.OpenMenu();
        }
    }

    public override void OnEnd()
    {
        inventoryMenu.UnbindEvents();
        inventoryMenu.CloseMenu();
    }
}
