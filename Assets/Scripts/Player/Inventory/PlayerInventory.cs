using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private ItemBase currentItem;
    
    public bool EquipItem(ItemBase item)
    {
        if (!item.CanEquip) return false;
        
        currentItem?.UnequipItem();
        item.EquipItem();
        currentItem = item;
        return true;
    }

    public void UnequipItem()
    {
        currentItem?.UnequipItem();
        currentItem = null;
    }
}