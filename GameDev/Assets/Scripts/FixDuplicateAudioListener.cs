using UnityEngine;

[ExecuteAlways]
public class FixDuplicateAudioListener : MonoBehaviour
{
    void Start()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        if (listeners.Length > 1)
        {
            Debug.LogWarning($"[FixDuplicateAudioListener] Ditemukan {listeners.Length} Audio Listeners! Menonaktifkan duplikat...");
            
            for (int i = 1; i < listeners.Length; i++)
            {
                listeners[i].enabled = false;
                Debug.Log($"[FixDuplicateAudioListener] Audio Listener pada '{listeners[i].gameObject.name}' dinonaktifkan");
            }
        }
    }
}
