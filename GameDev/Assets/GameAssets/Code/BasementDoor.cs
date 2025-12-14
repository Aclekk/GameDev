using System.Collections;
using UnityEngine;

public class BasementDoor : MonoBehaviour
{
    [Header("Player Refs")]
    public Transform player;
    public Camera playerCamera;
    public MonoBehaviour playerControllerToDisable;

    [Header("Interact Settings")]
    public float interactRadius = 2.0f;
    public Vector3 interactOffset = Vector3.zero;
    public KeyCode interactKey = KeyCode.E;

    [Header("Teleport Points")]
    public Transform insidePoint;
    public Transform outsideCheckPoint;
    public float checkDistance = 5f;

    [Header("Teleport Settings")]
    public float fadeInDuration = 0.3f;
    public AudioClip doorSound;

    AudioSource audioSource;

    void Start()
    {
        if (!player) player = GameObject.Find("Player")?.transform;
        if (!playerCamera) playerCamera = Camera.main;

        audioSource = GetComponent<AudioSource>();
        if (!audioSource && doorSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        // Hitung center interaksi (bisa digeser XYZ)
        Vector3 interactCenter = transform.position + transform.TransformDirection(interactOffset);

        if (Vector3.Distance(playerCamera.transform.position, interactCenter) <= interactRadius
            && Input.GetKeyDown(interactKey))
        {
            if (IsPlayerOutside())
                StartCoroutine(EnterBasement());
            else
                StartCoroutine(ExitBasement());
        }
    }

    bool IsPlayerOutside()
    {
        if (!outsideCheckPoint) return true;

        return Vector3.Distance(player.position, outsideCheckPoint.position) <= checkDistance;
    }

    IEnumerator EnterBasement()
    {
        if (playerControllerToDisable) playerControllerToDisable.enabled = false;
        if (audioSource && doorSound) audioSource.PlayOneShot(doorSound);

        yield return new WaitForSeconds(0.2f);

        TeleportPlayer(insidePoint);

        yield return new WaitForSeconds(fadeInDuration);
        if (playerControllerToDisable) playerControllerToDisable.enabled = true;
    }

    IEnumerator ExitBasement()
    {
        if (playerControllerToDisable) playerControllerToDisable.enabled = false;
        if (audioSource && doorSound) audioSource.PlayOneShot(doorSound);

        yield return new WaitForSeconds(0.2f);

        TeleportPlayer(outsideCheckPoint);

        yield return new WaitForSeconds(fadeInDuration);
        if (playerControllerToDisable) playerControllerToDisable.enabled = true;
    }

    void TeleportPlayer(Transform target)
    {
        if (!target) return;

        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc)
        {
            cc.enabled = false;
            player.position = target.position;
            player.rotation = target.rotation;
            cc.enabled = true;
        }
        else
        {
            player.position = target.position;
            player.rotation = target.rotation;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Gizmo untuk interaksi
        Gizmos.color = Color.cyan;
        Vector3 interactCenter = transform.position + transform.TransformDirection(interactOffset);
        Gizmos.DrawWireSphere(interactCenter, interactRadius);

        // Gizmo teleport
        if (insidePoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(insidePoint.position, 0.5f);
        }

        if (outsideCheckPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(outsideCheckPoint.position, checkDistance);
        }
    }
}
