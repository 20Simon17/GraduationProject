using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour
{
    [SerializeField] private GameObject imagesHolder;
    [SerializeField] private GameObject textHolder;
    [SerializeField] private GameObject buttonsHolder;
    
    private readonly List<InventoryItemBase> inventoryItems = new();
    private PlayerInventory playerInventory;

    private Button[] buttons;

    private void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        buttons = buttonsHolder.GetComponentsInChildren<Button>();

        foreach (Button button in buttons)
        {
            if (button.TryGetComponent(out InventoryItemBase inventoryItem))
            {
                inventoryItems.Add(inventoryItem);
            }
        }
            
        LoadItems();
    }

    public void BindEvents()
    {
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(LoadItems);
        }
    }

    public void UnbindEvents()
    {
        foreach (Button button in buttons)
        {
            button.onClick.RemoveListener(LoadItems);
        }
    }

    private void OnDisable() => UnbindEvents();
    
    private void LoadItems()
    {
        for(int i = 0; i < inventoryItems.Count; i++)
        {
            buttons[i].interactable = inventoryItems[i].item.isEnabled;
            buttons[i].GetComponent<Image>().color = inventoryItems[i].item.isEquipped ? Color.green : Color.white;
        }
    }

    public void EquipItem(InventoryItemBase item)
    {
        if (playerInventory.EquipItem(item.item))
        {
            LoadItems();
        }
    }

    public void UnequipItem()
    {
        playerInventory.UnequipItem();
    }

    public void OpenMenu()
    {
        LoadItems();
        imagesHolder.SetActive(true);
        textHolder.SetActive(true);
        buttonsHolder.SetActive(true);
    }

    public void CloseMenu()
    {
        imagesHolder.SetActive(false);
        textHolder.SetActive(false);
        buttonsHolder.SetActive(false);
    }
}