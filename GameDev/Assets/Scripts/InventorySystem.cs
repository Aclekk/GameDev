using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemId;
    public string itemName;
    public Sprite itemIcon;
    public int quantity;
    public int maxStack = 1;

    public InventoryItem(string id, string name, Sprite icon, int qty = 1, int stack = 1)
    {
        itemId = id;
        itemName = name;
        itemIcon = icon;
        quantity = qty;
        maxStack = stack;
    }
}

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 20;

    private List<InventoryItem> items = new List<InventoryItem>();

    public event Action<InventoryItem> OnItemAdded;
    public event Action<InventoryItem> OnItemRemoved;
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool AddItem(string itemId, string itemName, Sprite icon = null, int quantity = 1, int maxStack = 1)
    {
        InventoryItem existingItem = items.Find(item => item.itemId == itemId && item.quantity < item.maxStack);

        if (existingItem != null)
        {
            existingItem.quantity += quantity;
            OnItemAdded?.Invoke(existingItem);
            OnInventoryChanged?.Invoke();
            Debug.Log($"Added {quantity} {itemName} to inventory. Total: {existingItem.quantity}");
            return true;
        }

        if (items.Count >= maxSlots)
        {
            Debug.LogWarning("Inventory is full!");
            return false;
        }

        InventoryItem newItem = new InventoryItem(itemId, itemName, icon, quantity, maxStack);
        items.Add(newItem);
        OnItemAdded?.Invoke(newItem);
        OnInventoryChanged?.Invoke();
        Debug.Log($"Added {itemName} to inventory");
        return true;
    }

    public bool RemoveItem(string itemId, int quantity = 1)
    {
        InventoryItem item = items.Find(i => i.itemId == itemId);
        if (item == null) return false;

        item.quantity -= quantity;
        if (item.quantity <= 0)
        {
            items.Remove(item);
            OnItemRemoved?.Invoke(item);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(string itemId)
    {
        return items.Exists(item => item.itemId == itemId);
    }

    public int GetItemCount(string itemId)
    {
        InventoryItem item = items.Find(i => i.itemId == itemId);
        return item != null ? item.quantity : 0;
    }

    public List<InventoryItem> GetAllItems()
    {
        return new List<InventoryItem>(items);
    }

    public void ClearInventory()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }
}
