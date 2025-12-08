using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HantuMove : MonoBehaviour
{
    [Header("Referensi")]
    public Transform player;
    public Animator animator;                 // bool "isCrawl", trigger "Disappear", "Appear"
    public AudioSource audioSource;
    public AudioClip crawlClip;
    public LanternController lanternController;

    [Header("NavMesh")]
    public NavMeshAgent navMeshAgent;
    public float wanderSpeed = 2f;
    public float chaseSpeed = 4f;
    public float wanderRadius = 15f;
    public float idleChance = 0.3f;         // 30% chance to idle when reaching destination
    public float idleDuration = 2f;

    [Header("Spawn System")]
    public Transform[] spawnPoints;         // movepoint 1-4
    public float minSpawnTime = 30f;
    public float maxSpawnTime = 70f;
    public bool startHidden = true;
    public bool debugImmediateSpawn = false; // Set to true for testing
    public float spawnYOffset = 1f;         // Offset to prevent sinking into ground

    [Header("Lantern Detection")]
    public float lanternDetectionRadius = 10f;
    public float gazeDetectionAngle = 30f;  // sudut untuk deteksi tatapan player
    public LayerMask playerLayer;

    [Header("Audio")]
    public float audioTriggerRadius = 30f;
    public float audioMaxDistance = 35f;
    public float audioFadeSpeed = 6f;
    [Range(0f, 1f)] public float audioMaxVolume = 0.9f;

    // --- runtime ---
    private enum GhostState { Hidden, Spawning, Idle, Wandering, Disappearing }
    private GhostState _currentState = GhostState.Hidden;
    private Vector3 _wanderTarget;
    private float _spawnTimer;
    private float _nextSpawnTime;
    private float _idleTimer;
    private bool _isMoving = false;
    private bool _suppressAudio = false;
    
    [Header("Debug Info")]
    [SerializeField] private string debugState;
    [SerializeField] private float debugSpawnTimer;
    [SerializeField] private float debugNextSpawnTime;

    void Awake()
    {
        // Setup NavMesh Agent
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();
        
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = wanderSpeed;
            navMeshAgent.stoppingDistance = 0.5f;
            navMeshAgent.autoBraking = true;
        }

        // Setup spawn points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            var mp = GameObject.Find("Movepoint");
            if (mp != null)
            {
                var list = new List<Transform>();
                for (int i = 0; i < mp.transform.childCount; i++)
                    list.Add(mp.transform.GetChild(i));
                spawnPoints = list.ToArray();
                Debug.Log($"Found {spawnPoints.Length} spawn points from Movepoint object");
            }
            else
            {
                Debug.LogError("Movepoint object not found! Please create a Movepoint parent object with child spawn points.");
                // Try to find any GameObject with "movepoint" in name as fallback
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                var fallbackList = new List<Transform>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.ToLower().Contains("movepoint") || obj.name.ToLower().Contains("spawn"))
                    {
                        fallbackList.Add(obj.transform);
                    }
                }
                if (fallbackList.Count > 0)
                {
                    spawnPoints = fallbackList.ToArray();
                    Debug.Log($"Fallback: Found {spawnPoints.Length} spawn points from objects containing 'movepoint' or 'spawn'");
                }
            }
        }
        else
        {
            Debug.Log($"Using assigned spawn points: {spawnPoints.Length} points");
        }

        // Setup audio
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

        // Find lantern controller if not assigned
        if (lanternController == null)
            lanternController = FindObjectOfType<LanternController>();

        // Start hidden
        if (startHidden && !debugImmediateSpawn)
        {
            _currentState = GhostState.Hidden;
            // gameObject.SetActive(false);   // Fixed: Keep object active so Update runs
            HideGhostVisual(); // Hide visual instead of deactivating
            ScheduleNextSpawn();
        }
        else
        {
            Debug.Log("Starting immediate spawn (debug mode or startHidden=false)");
            SpawnGhost();
        }
    }

    void OnDisable()
    {
        // kalau script dimatikan, hentikan suara supaya tidak terus bunyi
        ForceStopFootstepAudio();
    }

    // --- VISUAL HIDING ---
    void HideGhostVisual()
    {
        // Hide mesh renderer
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;
        
        // Hide all child renderers
        var childRenderers = GetComponentsInChildren<Renderer>();
        foreach (var childRenderer in childRenderers)
            childRenderer.enabled = false;
        
        // Stop NavMeshAgent
        if (navMeshAgent)
            navMeshAgent.isStopped = true;
    }
    
    void ShowGhostVisual()
    {
        // Show mesh renderer
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = true;
        
        // Show all child renderers
        var childRenderers = GetComponentsInChildren<Renderer>();
        foreach (var childRenderer in childRenderers)
            childRenderer.enabled = true;
        
        // Resume NavMeshAgent
        if (navMeshAgent)
            navMeshAgent.isStopped = false;
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
            Debug.LogWarning("No spawn points available!");
            return;
        }

        // Choose random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Debug.Log($"Selected spawn point: {spawnPoint.name} at position {spawnPoint.position}");

        // Calculate spawn position with Y offset
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.y += spawnYOffset;

        // Use NavMeshAgent.Warp for proper NavMesh positioning
        if (navMeshAgent != null)
        {
            navMeshAgent.Warp(spawnPos);
            Debug.Log($"Ghost warped to NavMesh position: {spawnPos}");
        }
        else
        {
            transform.position = spawnPos;
            Debug.Log($"Ghost positioned at: {spawnPos}");
        }
        
        // Show ghost visual and start spawning
        ShowGhostVisual();
        Debug.Log("Ghost visual shown, starting spawn sequence");
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
        
        // Start wandering after spawn
        StartWandering();
    }

    IEnumerator DisappearSequence()
    {
        _currentState = GhostState.Disappearing;
        
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

        // Check for lantern detection and player gaze
        if (_currentState != GhostState.Hidden && _currentState != GhostState.Disappearing)
        {
            if (IsInLanternRadius() && IsPlayerLookingAtGhost())
            {
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
        if (navMeshAgent == null) return;

        // Random chance to idle
        if (Random.value < idleChance)
        {
            StartIdle();
            return;
        }

        // Find random position on NavMesh
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            _wanderTarget = hit.position;
            navMeshAgent.SetDestination(_wanderTarget);
            navMeshAgent.isStopped = false;
        }
    }

    void HandleWandering()
    {
        if (navMeshAgent == null) return;

        _isMoving = navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance;

        // Check if reached destination
        if (!_isMoving && navMeshAgent.hasPath)
        {
            // Random chance to idle or set new target
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

    void StartIdle()
    {
        _currentState = GhostState.Idle;
        _idleTimer = 0f;
        if (navMeshAgent)
            navMeshAgent.isStopped = true;
    }

    // --- DETECTION SYSTEM ---
    bool IsInLanternRadius()
    {
        if (lanternController == null || player == null) return false;
        
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= lanternDetectionRadius && lanternController.IsOn;
    }

    bool IsPlayerLookingAtGhost()
    {
        if (player == null) return false;

        Vector3 directionToGhost = (transform.position - player.position).normalized;
        float dotProduct = Vector3.Dot(player.forward, directionToGhost);
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        return angle <= gazeDetectionAngle;
    }

    // --- ANIMATION & AUDIO ---
    void UpdateAnimator()
    {
        if (animator == null) return;
        
        // Set crawling animation based on movement
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
        // Lantern detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lanternDetectionRadius);
        
        // Wandering radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        
        // Audio radius
        Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, audioTriggerRadius);
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, audioMaxDistance);
    }
}
