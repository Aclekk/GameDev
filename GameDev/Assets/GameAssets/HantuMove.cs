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
    public float waypointTolerance = 0.6f;    // dibuat sedikit besar supaya pasti "nyampe"

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

    private int _index;
    private float _waitTimer;
    private State _state = State.Patrol;
    private Vector3 _lastMoveDir;

    void Awake()
    {
        // Cari parent bila kosong
        if (waypointsParent == null)
        {
            var mp = GameObject.Find("Movepoint");
            if (mp) waypointsParent = mp.transform;
        }

        // Bangun rute otomatis sekali di awal
        BuildWaypointRoute();

        // Setup audio 3D
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
            // Urut sesuai urutan child di Hierarchy (tanpa perlu rename)
            var list = new List<Transform>();
            for (int i = 0; i < waypointsParent.childCount; i++)
                list.Add(waypointsParent.GetChild(i));
            waypoints = list.ToArray();
        }
        else // NearestChain
        {
            // Mulai dari child terdekat dengan posisi Hantu, lalu sambung ke tetangga terdekat yang belum dikunjungi
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

        _index = 0;          // mulai dari titik pertama rute
        _waitTimer = 0f;
    }

    void Update()
    {
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

        bool isMoving = _lastMoveDir.sqrMagnitude > 0.0001f;
        if (animator) animator.SetBool("isCrawl", isMoving);
        UpdateFootstepAudio(isMoving, distToPlayer);
    }

    void PatrolOnly()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[Mathf.Clamp(_index, 0, waypoints.Length - 1)];
        MoveTowards(target.position, patrolSpeed * speedMultiplier);

        if (Vector3.Distance(Flat(transform.position), Flat(target.position)) <= waypointTolerance)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= waitAtPoint)
            {
                _waitTimer = 0f;
                _index++;
                if (_index >= waypoints.Length)
                    _index = loop ? 0 : waypoints.Length - 1; // kalau tidak loop, berhenti di terakhir
            }
        }
        else _waitTimer = 0f;
    }

    void ChasePlayer()
    {
        if (!player) return;
        MoveTowards(player.position, chaseSpeed * speedMultiplier);
    }

    void MoveTowards(Vector3 targetPos, float speed)
    {
        Vector3 to = targetPos - transform.position;
        to.y = 0f; // jaga tetap di lantai
        Vector3 dir = to.normalized;

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotateSpeed * Time.deltaTime);
        }

        Vector3 delta = dir * speed * Time.deltaTime;
        transform.position += delta;
        _lastMoveDir = delta;
    }

    // --- AUDIO dekat saja + fade ---
    void UpdateFootstepAudio(bool isMoving, float distToPlayer)
    {
        if (!audioSource || crawlClip == null) return;

        float targetVol = 0f;
        if (isMoving && distToPlayer <= audioMaxDistance)
        {
            if (distToPlayer <= audioTriggerRadius) targetVol = audioMaxVolume;
            else
            {
                float t = Mathf.InverseLerp(audioMaxDistance, audioTriggerRadius, distToPlayer);
                targetVol = Mathf.Lerp(0f, audioMaxVolume, t);
            }
        }

        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVol, audioFadeSpeed * Time.deltaTime);

        if (audioSource.volume > 0.01f)
        {
            if (!audioSource.isPlaying) audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying) audioSource.Stop();
        }
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
