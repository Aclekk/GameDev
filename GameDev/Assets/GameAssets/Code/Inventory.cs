using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private HashSet<string> keys = new HashSet<string>();

    public void AddKey(string keyId)
    {
        if (string.IsNullOrEmpty(keyId)) return;
        if (keys.Add(keyId))
        {
            Debug.Log($"[Inventory] Key added: {keyId}");
        }
    }

    public bool HasKey(string keyId) => keys.Contains(keyId);
}
