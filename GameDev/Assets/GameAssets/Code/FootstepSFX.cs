using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class FootstepSFX : MonoBehaviour
{
    public enum TriggerMode { AnimationEvent, AutoFromVelocity }

    [Header("Mode")]
    public TriggerMode triggerMode = TriggerMode.AnimationEvent;
    public bool cutOffOnStop = true;          // true = hentikan suara seketika saat berhenti

    [Header("Ground Check (uses FeetRay)")]
    public Transform feetRay;                  // drag FeetRay
    public float rayDistance = 1.5f;
    public LayerMask groundMask = ~0;

    [Header("Clips")]
    public AudioClip[] defaultClips;
    public AudioClip[] woodClips;

    [Header("Sound Variations")]
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    public Vector2 volumeRange = new Vector2(0.9f, 1f);

    [Header("Auto From Velocity")]
    public float stepInterval = 0.42f;         // jeda langkah (≈ 0.42 detik untuk jalan)
    public float moveThreshold = 0.1f;         // min kecepatan dianggap bergerak

    [Header("Anti-Double")]
    public float minStepGap = 0.2f;            // cooldown minimal antar langkah

    private AudioSource _src;
    private CharacterController _cc;
    private Rigidbody _rb;

    private float _stepTimer;
    private float _lastStepTime = -999f;

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.loop = false;            // per langkah, bukan loop
        _src.spatialBlend = 1f;

        if (feetRay == null)
        {
            var t = transform.Find("FeetRay");
            if (t) feetRay = t;
        }

        _cc = GetComponentInParent<CharacterController>();
        _rb = GetComponentInParent<Rigidbody>();
    }

   void Update()
{
    bool grounded = IsGrounded();
    float speed = GetHorizontalSpeed();

    if (triggerMode == TriggerMode.AutoFromVelocity)
    {
        // baru mulai jalan → langsung bunyi
        if (grounded && speed > moveThreshold)
        {
            if (_stepTimer == 0f && Time.time - _lastStepTime >= minStepGap)
            {
                PlayFootstep();
                _stepTimer += Time.deltaTime;
                return;
            }

            _stepTimer += Time.deltaTime;
            if (_stepTimer >= stepInterval && Time.time - _lastStepTime >= minStepGap)
            {
                _stepTimer = 0f;
                PlayFootstep();
            }
        }
        else
        {
            // berhenti total → reset dan matikan suara
            if (cutOffOnStop && _src.isPlaying)
                _src.Stop();

            _stepTimer = 0f;
        }
    }
    else
    {
        // animation event mode
        if (cutOffOnStop && (!grounded || speed <= moveThreshold))
        {
            if (_src.isPlaying) _src.Stop();
        }
    }
}


    /// <summary>
    /// Panggil dari Animation Event pada footfall (kaki menyentuh tanah).
    /// Gunakan hanya jika triggerMode = AnimationEvent.
    /// </summary>
    public void PlayFootstep()
    {
        // Anti-double: hormati cooldown
        if (Time.time - _lastStepTime < minStepGap) return;

        AudioClip clip = PickClip(IsOnWood() ? woodClips : defaultClips);
        if (clip == null) return;

        // Jika masih memutar clip lama dan kamu tidak ingin overlap, hentikan dulu.
        if (_src.isPlaying) _src.Stop();

        _src.pitch = Random.Range(pitchRange.x, pitchRange.y);
        _src.volume = Random.Range(volumeRange.x, volumeRange.y);
        _src.clip = clip;
        _src.Play();

        _lastStepTime = Time.time;
    }

    // ---------------- Helpers ----------------

    private bool IsOnWood()
    {
        if (feetRay == null) return false;
        if (Physics.Raycast(feetRay.position, Vector3.down, out RaycastHit hit, rayDistance, groundMask, QueryTriggerInteraction.Ignore))
            return hit.collider != null && hit.collider.CompareTag("wood");
        return false;
    }

    private bool IsGrounded()
    {
        if (feetRay == null) return true;
        return Physics.Raycast(feetRay.position, Vector3.down, rayDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    private float GetHorizontalSpeed()
    {
        if (_cc != null) return new Vector2(_cc.velocity.x, _cc.velocity.z).magnitude;
        if (_rb != null) return new Vector2(_rb.velocity.x, _rb.velocity.z).magnitude;
        return 0f;
    }

    private static AudioClip PickClip(AudioClip[] bank)
    {
        if (bank == null || bank.Length == 0) return null;
        return bank[Random.Range(0, bank.Length)];
    }
}
