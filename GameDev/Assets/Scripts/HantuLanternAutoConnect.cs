using UnityEngine;
using UHFPS.Runtime;

public class HantuLanternAutoConnect : MonoBehaviour
{
    public HantuMove hantuMove;
    
    void Start()
    {
        if (hantuMove == null)
            hantuMove = GetComponent<HantuMove>();
        
        if (hantuMove != null)
        {
            ConnectLantern();
        }
    }
    
    void ConnectLantern()
    {
        LanternItem lanternItem = FindObjectOfType<LanternItem>();
        
        if (lanternItem != null)
        {
            hantuMove.lanternItem = lanternItem;
            Debug.Log("[HantuLanternAutoConnect] ✅ Lantern berhasil terhubung ke Hantu!");
        }
        else
        {
            Debug.LogWarning("[HantuLanternAutoConnect] ⚠️ LanternItem tidak ditemukan! Hantu tidak bisa detect cahaya!");
        }
    }
}
