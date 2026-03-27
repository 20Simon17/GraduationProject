using UnityEngine;

public class ItemBase : MonoBehaviour
{
    public bool CanEquip => isUnlocked && isEnabled && !isEquipped;
    public bool isUnlocked;
    public bool isEnabled;
    public bool isEquipped;
    
    public virtual void EquipItem()
    {
        isEquipped = true;
    }
    
    public virtual void UnequipItem()
    {
        isEquipped = false;
    }
}