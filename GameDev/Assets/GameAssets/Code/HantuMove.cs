using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UHFPS.Runtime;

public class HantuMove : MonoBehaviour
{
    [Header("Referensi")]
    public Transform player;
    public Animator animator;                 // bool "isCrawl", trigger "Disappear", "Appear"
    public AudioSource audioSource;
    public AudioClip crawlClip;
    public LanternItem lanternItem;

    [Header("NavMesh")]
    public NavMeshAgent navMeshAgent;
    public float wanderSpeed = 2f;
    public float chaseSpeed = 4f;
    public float wanderRadius = 15f;
    public float idleChance = 0.3f;
    public float idleDuration = 2f;

    [Header("Spawn System")]
    public Transform[] spawnPoints;
    public float minSpawnTime = 30f;
    public float maxSpawnTime = 70f;
    public bool startHidden = true;
    public bool debugImmediateSpawn = false;

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 2f;
    public float stuckVelocityThreshold = 0.1f;
    public int maxPathRetries = 5;

    [Header("Lantern Detection")]
    public float maxDetectionRadius = 8f;
    public float minDetectionRadius = 2f;
    public bool useDynamicDetection = true;
    public LayerMask playerLayer;

    private HantuJumpscare _jumpscare;

    [Header("Audio")]
    public float audioTriggerRadius = 30f;
    public float audioMaxDistance = 35f;
    public float audioFadeSpeed = 6f;
    [Range(0f, 1f)] public float audioMaxVolume = 0.9f;

    [Header("Lantern Hit SFX")]
    public AudioClip hitByLanternClip;
    [Range(0f, 1f)] public float hitByLanternVolume = 1f;

    private enum GhostState { Hidden, Spawning, Idle, Wandering, Disappearing }
    private GhostState _currentState = GhostState.Hidden;
    private Vector3 _wanderTarget;
    private float _spawnTimer;
    private float _nextSpawnTime;
    private float _idleTimer;
    private bool _isMoving = false;
    private bool _suppressAudio = false;
    private Collider[] _ghostColliders;
    private float _stuckCheckTimer;
    private Vector3 _lastPosition;
    private int _pathRetryCount;
    
    [Header("Debug Info")]
    [SerializeField] private string debugState;
    [SerializeField] private float debugSpawnTimer;
    [SerializeField] private float debugNextSpawnTime;

    void Awake()
    {
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();
        
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = wanderSpeed;
            navMeshAgent.stoppingDistance = 0.5f;
            navMeshAgent.autoBraking = true;
            navMeshAgent.acceleration = 8f;
            navMeshAgent.angularSpeed = 120f;
            navMeshAgent.radius = 0.5f;
            navMeshAgent.height = 2f;
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            navMeshAgent.avoidancePriority = 50;
        }
        else
        {
            Debug.LogError("NavMeshAgent component tidak ditemukan pada Hantu!");
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            var mp = GameObject.Find("Movepoint");
            if (mp != null)
            {
                var list = new List<Transform>();
                for (int i = 0; i < mp.transform.childCount; i++)
                    list.Add(mp.transform.GetChild(i));
                spawnPoints = list.ToArray();
                Debug.Log($"Found {spawnPoints.Length} spawn points from Movepoint");
            }
        }

        if (audioSource)
        {
            audioSource.clip = crawlClip;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.dopplerLevel = 0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = Mathf.Max(0.1f, audioTriggerRadius * 0.75f);
            audioSource.maxDistance = Mathf.Max(audioTriggerRadius, audioMaxDistance);
            audioSource.volume = 0f;
        }

        if (lanternItem == null)
            lanternItem = FindObjectOfType<LanternItem>();

        _jumpscare = GetComponent<HantuJumpscare>();
        _ghostColliders = GetComponentsInChildren<Collider>(true);

        if (startHidden && !debugImmediateSpawn)
        {
            Hide();
        }
        else if (debugImmediateSpawn)
        {
            SpawnGhost();
        }
    }

    void OnDisable()
    {
        ForceStopFootstepAudio();
    }

    void Hide()
    {
        _currentState = GhostState.Hidden;
        HideGhostVisual();
        ScheduleNextSpawn();
    }

    void HideGhostVisual()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;
        
        var childRenderers = GetComponentsInChildren<Renderer>();
        foreach (var childRenderer in childRenderers)
            childRenderer.enabled = false;
        
        if (navMeshAgent && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        if (_jumpscare) _jumpscare.enabled = false;

        if (_ghostColliders != null)
        {
            foreach (var c in _ghostColliders)
                c.enabled = false;
        }
    }
    
    void ShowGhostVisual()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = true;
        
        var childRenderers = GetComponentsInChildren<Renderer>();
        foreach (var childRenderer in childRenderers)
            childRenderer.enabled = true;
        
        if (navMeshAgent)
        {
            if (!navMeshAgent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
                {
                    navMeshAgent.enabled = false;
                    transform.position = hit.position;
                    navMeshAgent.enabled = true;
                }
            }
            navMeshAgent.isStopped = false;
        }

        if (_jumpscare) _jumpscare.enabled = true;

        if (_ghostColliders != null)
        {
            foreach (var c in _ghostColliders)
                c.enabled = true;
        }
    }
    
    // --- SPAWN SYSTEM ---
    void ScheduleNextSpawn()
    {
        _spawnTimer = 0f;
        _nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
        Debug.Log($"Next spawn scheduled in {_nextSpawnTime:F1} seconds");
    }

    void SpawnGhost()
    {
        Debug.Log("Attempting to spawn ghost...");
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points available! Add movepoint objects.");
            return;
        }

        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent is null! Cannot spawn ghost.");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Debug.Log($"Selected spawn point: {spawnPoint.name}");

        Vector3 targetPosition = spawnPoint.position;
        NavMeshHit navHit;

        if (NavMesh.SamplePosition(targetPosition, out navHit, 5f, NavMesh.AllAreas))
        {
            targetPosition = navHit.position;
        }
        else
        {
            Debug.LogWarning($"Spawn point {spawnPoint.name} tidak di NavMesh, mencari terdekat...");
            for (float radius = 10f; radius <= 30f; radius += 5f)
            {
                if (NavMesh.SamplePosition(spawnPoint.position, out navHit, radius, NavMesh.AllAreas))
                {
                    targetPosition = navHit.position;
                    Debug.Log($"Found NavMesh dalam radius {radius}m");
                    break;
                }
            }
        }

        navMeshAgent.enabled = false;
        transform.position = targetPosition;
        transform.rotation = spawnPoint.rotation;
        navMeshAgent.enabled = true;

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
        navMeshAgent.velocity = Vector3.zero;

        ShowGhostVisual();
        
        Debug.Log($"Ghost spawned at {transform.position}");
        StartCoroutine(SpawnSequence());
    }

    IEnumerator SpawnSequence()
    {
        _currentState = GhostState.Spawning;
        
        // Play appear animation
        if (animator)
            animator.SetTrigger("Appear");
        
        // Wait for animation to finish
        yield return new WaitForSeconds(1f);
        
        if (navMeshAgent) navMeshAgent.isStopped = false;
        StartWandering();
    }

    IEnumerator DisappearSequence()
    {
        _currentState = GhostState.Disappearing;
        
        // >>> PATCH: matiin jumpscare radius saat kena lantern / mau hilang
        if (_jumpscare) _jumpscare.enabled = false;     // stop HantuJumpscare.Update
        _isMoving = false;                              // paksa anim idle

        // opsional tapi aman: matiin collider selama transisi (biar gak ada trigger aneh)
        if (_ghostColliders != null)
        {
            foreach (var c in _ghostColliders)
                c.enabled = false;
        }
        
        // Stop movement
        if (navMeshAgent)
            navMeshAgent.isStopped = true;
        
        // Play idle animation first
        if (animator)
            animator.SetBool("isCrawl", false);
        
        yield return new WaitForSeconds(1f);
        
        // Play disappear animation
        if (animator)
            animator.SetTrigger("Disappear");
        
        yield return new WaitForSeconds(1f);
        
        // Hide ghost and schedule next spawn
        // gameObject.SetActive(false);       // Fixed: Keep object active so Update runs
        HideGhostVisual();
        _currentState = GhostState.Hidden;
        ScheduleNextSpawn();
    }

    void Update()
    {
        // Update debug info
        debugState = _currentState.ToString();
        debugSpawnTimer = _spawnTimer;
        debugNextSpawnTime = _nextSpawnTime;
        
        // Handle spawn timer when hidden
        if (_currentState == GhostState.Hidden)
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _nextSpawnTime)
            {
                SpawnGhost();
            }
            return;
        }

        bool jumpscareActive = (_jumpscare != null && _jumpscare.IsInProgress);

        // saat jumpscare: jangan boleh despawn gara2 "kamera auto natap"
        if (!jumpscareActive && _currentState != GhostState.Hidden && _currentState != GhostState.Disappearing)
        {
            if (IsInLanternRadius())
            {
                // stop crawl loop biar ga tabrakan sama sfx cahaya
                ForceStopFootstepAudio();

                // bunyi kena cahaya (pakai PlayClipAtPoint biar ga ikut ke-stop)
                if (hitByLanternClip != null)
                    AudioSource.PlayClipAtPoint(hitByLanternClip, transform.position, hitByLanternVolume);

                StartCoroutine(DisappearSequence());
                return;
            }
        }

        // Handle different states
        switch (_currentState)
        {
            case GhostState.Spawning:
                // Spawning animation handled by coroutine
                break;

            case GhostState.Idle:
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= idleDuration)
                {
                    StartWandering();
                }
                break;

            case GhostState.Wandering:
                HandleWandering();
                break;
        }

        UpdateAnimator();
        UpdateFootstepAudio();
    }

    // --- MOVEMENT SYSTEM ---
    void StartWandering()
    {
        _currentState = GhostState.Wandering;
        SetNewWanderTarget();
    }

    void SetNewWanderTarget()
    {
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning("NavMeshAgent tidak ada atau tidak di NavMesh!");
            return;
        }

        for (int i = 0; i < 30; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (navMeshAgent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    _wanderTarget = hit.position;
                    navMeshAgent.isStopped = false;
                    navMeshAgent.SetDestination(_wanderTarget);
                    return;
                }
            }
        }
        
        Debug.LogWarning("Tidak bisa menemukan wander target yang valid setelah 30 percobaan");
    }

    void HandleWandering()
    {
        if (navMeshAgent == null) return;

        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning("Hantu tidak di NavMesh! Mencoba reposisi...");
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.enabled = false;
                transform.position = hit.position;
                navMeshAgent.enabled = true;
                Debug.Log("Hantu direposisi ke NavMesh");
            }
            return;
        }

        _isMoving = navMeshAgent.velocity.sqrMagnitude > 0.1f;

        CheckIfStuck();

        if (!navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude < 0.01f)
                {
                    if (Random.value < idleChance)
                    {
                        StartIdle();
                    }
                    else
                    {
                        SetNewWanderTarget();
                    }
                }
            }
            else if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogWarning("Path invalid, mencari target baru");
                SetNewWanderTarget();
            }
        }
    }

    void CheckIfStuck()
    {
        _stuckCheckTimer += Time.deltaTime;

        if (_stuckCheckTimer >= stuckCheckInterval)
        {
            _stuckCheckTimer = 0f;

            if (navMeshAgent.hasPath && !navMeshAgent.isStopped)
            {
                float distMoved = Vector3.Distance(transform.position, _lastPosition);
                
                if (distMoved < stuckVelocityThreshold)
                {
                    _pathRetryCount++;
                    Debug.LogWarning($"Hantu stuck! (moved only {distMoved}m) Retry #{_pathRetryCount}");

                    if (_pathRetryCount >= maxPathRetries)
                    {
                        Debug.LogWarning("Max retries reached, teleporting to new position");
                        TeleportToRandomNavMeshPosition();
                        _pathRetryCount = 0;
                    }
                    else
                    {
                        SetNewWanderTarget();
                    }
                }
                else
                {
                    _pathRetryCount = 0;
                }
            }

            _lastPosition = transform.position;
        }
    }

    void TeleportToRandomNavMeshPosition()
    {
        Vector3 newPos = FindNavMeshNearPosition(transform.position, wanderRadius * 2f);
        if (newPos != Vector3.zero)
        {
            navMeshAgent.enabled = false;
            transform.position = newPos;
            navMeshAgent.enabled = true;
            SetNewWanderTarget();
            Debug.Log($"Teleported hantu to {newPos}");
        }
    }

    Vector3 FindNavMeshNearPosition(Vector3 center, float radius)
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 randomPoint = center + new Vector3(randomCircle.x, 0f, randomCircle.y);
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        
        NavMeshHit fallbackHit;
        if (NavMesh.SamplePosition(center, out fallbackHit, radius, NavMesh.AllAreas))
        {
            return fallbackHit.position;
        }
        
        return Vector3.zero;
    }

    void StartIdle()
    {
        _currentState = GhostState.Idle;
        _idleTimer = 0f;
        if (navMeshAgent)
            navMeshAgent.isStopped = true;
    }

    // --- DETECTION SYSTEM ---
    public bool IsInLanternRadius()
    {
        if (lanternItem == null || !lanternItem.IsLit()) return false;

        Vector3 origin = player != null ? player.position : transform.position;
        
        if (lanternItem.LanternLight != null)
        {
            origin = lanternItem.LanternLight.transform.position;
        }

        float effectiveRadius = maxDetectionRadius;

        if (useDynamicDetection && lanternItem != null)
        {
            float fuelPercent = lanternItem.lanternFuel;
            float radiusCurve = Mathf.Pow(fuelPercent, 0.5f);
            effectiveRadius = Mathf.Lerp(minDetectionRadius, maxDetectionRadius, radiusCurve);
        }

        if (effectiveRadius <= 0f) return false;

        float distance = Vector3.Distance(transform.position, origin);
        
        bool inRadius = distance <= effectiveRadius;

        return inRadius;
    }
    
    public float GetCurrentDetectionRadius()
    {
        if (lanternItem == null || !lanternItem.IsLit()) return 0f;

        if (useDynamicDetection && lanternItem != null)
        {
            float fuelPercent = lanternItem.lanternFuel;
            float radiusCurve = Mathf.Pow(fuelPercent, 0.5f);
            return Mathf.Lerp(minDetectionRadius, maxDetectionRadius, radiusCurve);
        }

        return maxDetectionRadius;
    }

    
    // --- ANIMATION & AUDIO ---
    void UpdateAnimator()
    {
        if (animator == null) return;

        // Saat spawning / disappearing: paksa idle
        if (_currentState == GhostState.Spawning || _currentState == GhostState.Disappearing)
        {
            animator.SetBool("isCrawl", false);
            return;
        }

        animator.SetBool("isCrawl", _isMoving);
    }

    void UpdateFootstepAudio()
    {
        if (!audioSource || crawlClip == null) return;

        float distToPlayer = player ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;

        // Suppressed audio during special states
        if (_suppressAudio || _currentState == GhostState.Spawning || _currentState == GhostState.Disappearing)
        {
            ForceStopFootstepAudio();
            return;
        }

        if (!_isMoving || distToPlayer > audioMaxDistance)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.volume = 0f;
            return;
        }

        float targetVol = (distToPlayer <= audioTriggerRadius)
            ? audioMaxVolume
            : Mathf.Lerp(0f, audioMaxVolume,
                Mathf.InverseLerp(audioMaxDistance, audioTriggerRadius, distToPlayer));

        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVol, audioFadeSpeed * Time.deltaTime);

        if (!audioSource.isPlaying && audioSource.volume > 0.01f)
            audioSource.Play();
    }

    // --- API untuk skrip lain ---
    public void SuppressCrawlAudio(bool on)
    {
        _suppressAudio = on;
        if (on) ForceStopFootstepAudio();
    }

    public void ForceStopFootstepAudio()
    {
        if (!audioSource) return;
        if (audioSource.isPlaying) audioSource.Stop();
        audioSource.volume = 0f;
    }

    public bool IsGhostActive()
    {
        return _currentState != GhostState.Hidden;
    }

    public void ForceDisappear()
    {
        if (_currentState != GhostState.Hidden && _currentState != GhostState.Disappearing)
        {
            StartCoroutine(DisappearSequence());
        }
    }
    
    [ContextMenu("Test Force Spawn")]
    public void ForceSpawn()
    {
        Debug.Log("Force spawning ghost for testing");
        _currentState = GhostState.Hidden;
        SpawnGhost();
    }

    void OnDrawGizmosSelected()
    {
        float currentRadius = GetCurrentDetectionRadius();
        
        Gizmos.color = Color.yellow;
        if (currentRadius > 0f)
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        else
            Gizmos.DrawWireSphere(transform.position, maxDetectionRadius);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        
        Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, audioTriggerRadius);
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, audioMaxDistance);
    }
}
