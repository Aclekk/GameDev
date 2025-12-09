using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasementDoor : MonoBehaviour
{
    [Header("Player Refs")]
    public Transform player;
    public Camera playerCamera;

    [Header("Interact Settings")]
    public float interactRadius = 2.0f;
    public Vector3 interactOffset;   // <<< AREA INTERAKSI BISA DIGESER DI INSPECTOR
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Prompt")]
    public GameObject pressEText;   // <<< UI text "Press E to Open the Door"

    [Header("Audio (optional)")]
    public AudioClip doorSound;
    AudioSource audioSource;

    bool isInRange = false;

    void Start()
    {
        if (!player && GameObject.Find("Player"))
            player = GameObject.Find("Player").transform;

        if (!playerCamera && Camera.main)
            playerCamera = Camera.main;

        if (!player || !playerCamera)
        {
            Debug.LogError("[BasementDoor] Player/Camera belum di-assign.");
            enabled = false;
            return;
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (!audioSource && doorSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // hide prompt on start
        if (pressEText)
            pressEText.SetActive(false);
    }

    void Update()
    {
        Vector3 interactPos = transform.position + interactOffset;
        float distance = HorizontalDistance(playerCamera.transform.position, interactPos);

        bool nowInRange = distance <= interactRadius;

        // 🔥 Show/hide prompt
        if (pressEText)
        {
            if (nowInRange && !isInRange)
                pressEText.SetActive(true);

            if (!nowInRange && isInRange)
                pressEText.SetActive(false);
        }

        isInRange = nowInRange;

        // Tekan E → pindah scene Basement
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(LoadBasementScene());
        }
    }

    IEnumerator LoadBasementScene()
    {
        if (audioSource && doorSound)
            audioSource.PlayOneShot(doorSound);

        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadScene("Basement");
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void OnDisable()
    {
        if (pressEText)
            pressEText.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 interactPos = transform.position + interactOffset;

        Gizmos.DrawWireSphere(interactPos, interactRadius);
        Gizmos.DrawLine(transform.position, interactPos);
    }
}
