using UnityEngine;

[System.Obsolete("LanternToggleExtension is deprecated. Toggle logic is now built into LanternItem.cs (UHFPS). You can safely remove this component.")]
public class LanternToggleExtension : MonoBehaviour
{
    void Start()
    {
        Debug.LogWarning("[LanternToggleExtension] DEPRECATED: This script is no longer needed. Toggle logic is now integrated into LanternItem.cs. Please remove this component.", this);
    }
}
