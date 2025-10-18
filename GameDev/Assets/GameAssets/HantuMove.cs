using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HantuMove : MonoBehaviour
{
    public enum State { Patrol, Chase }
    public enum PathMode { HierarchyOrder, NearestChain }

    [Header("Referensi")]
    public Transform player;
    public Animator animator;                 // bool "isCrawl"
    public AudioSource audioSource;
    public AudioClip crawlClip;

    [Header("Waypoint / Patrol")]
    public Transform waypointsParent;         // drag "Movepoint"
    public Transform[] waypoints;             // auto-terisi
    public PathMode pathMode = PathMode.HierarchyOrder;
    public bool loop = true;
    public float waitAtPoint = 0.2f;
    public float waypointTolerance = 0.6f;

    [Header("Kecepatan")]
    public float patrolSpeed = 1.2f;
    public float chaseSpeed = 2.2f;
    [Range(0.2f, 3f)] public float speedMultiplier = 1f;
    public float rotateSpeed = 8f;

    [Header("Deteksi Player")]
    public float detectionRadius = 8f;
    public float loseRadiusMultiplier = 1.35f;

    [Header("Audio (bunyi hanya saat dekat)")]
    public float audioTriggerRadius = 6f;
    public float audioMaxDistance = 12f;
    public float audioFadeSpeed = 6f;
    [Range(0f, 1f)] public float audioMaxVolume = 0.9f;

    // --- runtime ---
    private int _index;
    private float _waitTimer;
    private State _state = State.Patrol;
    private Vector3 _lastMoveDir;
    private bool _movedThisFrame;
    private const float _eps = 0.0001f;

    // --- untuk blokir audio saat jumpscare/keadaan khusus ---
    [SerializeField] private bool _suppressAudio = false;

    void Awake()
    {
        if (waypointsParent == null)
        {
            var mp = GameObject.Find("Movepoint");
            if (mp) waypointsParent = mp.transform;
        }
        BuildWaypointRoute();

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
    }

    void OnDisable()
    {
        // kalau script dimatikan, hentikan suara supaya tidak terus bunyi
        ForceStopFootstepAudio();
    }

    // --- ROUTE BUILDER ---
    void BuildWaypointRoute()
    {
        if (waypointsParent == null || waypointsParent.childCount == 0)
        {
            waypoints = new Transform[0];
            return;
        }

        if (pathMode == PathMode.HierarchyOrder)
        {
            var list = new List<Transform>();
            for (int i = 0; i < waypointsParent.childCount; i++)
                list.Add(waypointsParent.GetChild(i));
            waypoints = list.ToArray();
        }
        else
        {
            var all = new List<Transform>();
            for (int i = 0; i < waypointsParent.childCount; i++)
                all.Add(waypointsParent.GetChild(i));

            var route = new List<Transform>();
            Transform current = all.OrderBy(t => Vector3.SqrMagnitude(t.position - transform.position)).First();
            route.Add(current);
            all.Remove(current);

            while (all.Count > 0)
            {
                current = all.OrderBy(t => Vector3.SqrMagnitude(t.position - current.position)).First();
                route.Add(current);
                all.Remove(current);
            }
            waypoints = route.ToArray();
        }

        _index = 0;
        _waitTimer = 0f;
    }

    void Update()
    {
        _movedThisFrame = false; // reset flag di awal frame

        float distToPlayer = player ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;

        switch (_state)
        {
            case State.Patrol:
                if (distToPlayer <= detectionRadius) _state = State.Chase;
                PatrolOnly();
                break;

            case State.Chase:
                if (distToPlayer > detectionRadius * loseRadiusMultiplier) _state = State.Patrol;
                ChasePlayer();
                break;
        }

        bool isMoving = _movedThisFrame;
        if (animator) animator.SetBool("isCrawl", isMoving);
        UpdateFootstepAudio(isMoving, distToPlayer);
    }

    void PatrolOnly()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[Mathf.Clamp(_index, 0, waypoints.Length - 1)];
        float dist = Vector3.Distance(Flat(transform.position), Flat(target.position));

        // Sudah sampai → JEDA (TIDAK memanggil MoveTowards!)
        if (dist <= waypointTolerance)
        {
            _lastMoveDir = Vector3.zero;
            _movedThisFrame = false;

            _waitTimer += Time.deltaTime;
            if (_waitTimer >= waitAtPoint)
            {
                _waitTimer = 0f;
                _index++;
                if (_index >= waypoints.Length)
                    _index = loop ? 0 : waypoints.Length - 1;
            }
            return;
        }

        // Belum sampai → bergerak
        MoveTowards(target.position, patrolSpeed * speedMultiplier);
    }

    void ChasePlayer()
    {
        if (!player) return;
        MoveTowards(player.position, chaseSpeed * speedMultiplier);
    }

    void MoveTowards(Vector3 targetPos, float speed)
    {
        Vector3 to = targetPos - transform.position;
        to.y = 0f;
        Vector3 dir = to.normalized;

        if (dir.sqrMagnitude > _eps)
        {
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotateSpeed * Time.deltaTime);
        }

        Vector3 delta = dir * speed * Time.deltaTime;

        if (delta.sqrMagnitude > _eps)
        {
            transform.position += delta;
            _lastMoveDir = delta;
            _movedThisFrame = true;              // bergerak
        }
        else
        {
            _lastMoveDir = Vector3.zero;
        }
    }

    // --- AUDIO: play hanya saat bergerak, stop saat diam; hormati suppress ---
    void UpdateFootstepAudio(bool isMoving, float distToPlayer)
    {
        if (!audioSource || crawlClip == null) return;

        // Saat suppress (mis. jumpscare), paksa diam
        if (_suppressAudio)
        {
            ForceStopFootstepAudio();
            return;
        }

        if (!isMoving || distToPlayer > audioMaxDistance)
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

    // --- API utk skrip lain (mis. HantuJumpscare) ---
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

    Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.gray; Gizmos.DrawWireSphere(transform.position, detectionRadius * loseRadiusMultiplier);
        Gizmos.color = new Color(0f, 1f, 0f, 0.6f); Gizmos.DrawWireSphere(transform.position, audioTriggerRadius);
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f); Gizmos.DrawWireSphere(transform.position, audioMaxDistance);
    }
}
