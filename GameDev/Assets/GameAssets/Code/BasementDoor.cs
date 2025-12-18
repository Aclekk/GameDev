using System.Collections;
using UnityEngine;
using TMPro;
using UHFPS.Runtime;

public class BasementDoor : MonoBehaviour
{
    [Header("UI")]
    public GameObject pressEText;

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
    public bool flipInsideRotation180 = false;

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

        SetPrompt(pressEText, false, "Press \"E\" to Open");
    }

    void Update()
    {
        Vector3 interactCenter = transform.position + transform.TransformDirection(interactOffset);
        bool inRange = Vector3.Distance(playerCamera.transform.position, interactCenter) <= interactRadius;

        SetPrompt(pressEText, inRange, "Press \"E\" to Open");

        if (inRange && Input.GetKeyDown(interactKey))
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

        TeleportPlayer(insidePoint, flipInsideRotation180);

        yield return new WaitForSeconds(fadeInDuration);
        if (playerControllerToDisable) playerControllerToDisable.enabled = true;
    }

    IEnumerator ExitBasement()
    {
        if (playerControllerToDisable) playerControllerToDisable.enabled = false;
        if (audioSource && doorSound) audioSource.PlayOneShot(doorSound);

        yield return new WaitForSeconds(0.2f);

        TeleportPlayer(outsideCheckPoint, false);

        yield return new WaitForSeconds(fadeInDuration);
        if (playerControllerToDisable) playerControllerToDisable.enabled = true;
    }

    void TeleportPlayer(Transform target, bool flip180 = false)
    {
        if (!target) return;

        LookController lookController = player.GetComponentInChildren<LookController>();
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc)
        {
            cc.enabled = false;
        }

        player.position = target.position;
        player.rotation = Quaternion.identity;

        if (cc)
        {
            cc.enabled = true;
        }

        if (lookController != null)
        {
            float targetRotation = target.eulerAngles.y;
            if (flip180)
            {
                targetRotation += 180f;
                if (targetRotation >= 360f) targetRotation -= 360f;
            }
            
            lookController.LookRotation = new Vector2(targetRotation, 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 interactCenter = transform.position + transform.TransformDirection(interactOffset);
        Gizmos.DrawWireSphere(interactCenter, interactRadius);

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

    void OnDisable()
    {
        SetPrompt(pressEText, false, "Press \"E\" to Open");
    }

    void SetPrompt(GameObject root, bool visible, string message)
    {
        if (!root) return;

        var tmp = root.GetComponentInChildren<TMP_Text>(true);
        var cg = root.GetComponentInChildren<CanvasGroup>(true);

        if (tmp) tmp.text = message;

        if (cg)
        {
            if (!root.activeSelf) root.SetActive(true);
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }
        else
        {
            root.SetActive(visible);
        }
    }
}
