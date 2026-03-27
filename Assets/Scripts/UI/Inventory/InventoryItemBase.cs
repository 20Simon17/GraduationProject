using UnityEngine;
using UnityEngine.UI;

public class InventoryItemBase : MonoBehaviour
{
    [SerializeField] private InventoryMenu inventoryMenu;
    public ItemBase item;
    public string itemName;
    public string itemDescription;
    public Image itemImage;

    public void ToggleEquipped()
    {
        if (item.isEquipped) inventoryMenu.UnequipItem();
        else inventoryMenu.EquipItem(this);
    }
}