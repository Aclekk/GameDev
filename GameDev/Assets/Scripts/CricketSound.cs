using UnityEngine;

public class CricketSound : MonoBehaviour
{
    private AudioSource audioSource;
    
    [Header("Audio Clip")]
    [Tooltip("Drag dan drop file audio jangkrik di sini")]
    public AudioClip cricketAudioClip;
    
    [Header("3D Sound Settings")]
    [Tooltip("Jarak minimum dimana suara terdengar pada volume maksimal")]
    public float minDistance = 5f;
    
    [Tooltip("Jarak maksimum dimana suara masih bisa terdengar")]
    public float maxDistance = 100f;
    
    [Tooltip("Volume suara (0-1)")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    [Tooltip("Pitch suara (kecepatan playback)")]
    [Range(0.5f, 1.5f)]
    public float pitch = 1f;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        ConfigureAudioSource();
    }

    void ConfigureAudioSource()
    {
        if (cricketAudioClip != null)
        {
            audioSource.clip = cricketAudioClip;
        }
        
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    public void UpdateSoundRadius(float newMaxDistance)
    {
        maxDistance = newMaxDistance;
        audioSource.maxDistance = maxDistance;
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }

    void OnValidate()
    {
        if (audioSource != null)
        {
            ConfigureAudioSource();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, minDistance);
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, maxDistance);
        
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
