using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Cinemachine;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class CutsceneTrigger : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum TriggerTypeEnum { Trigger, Interact, Event }
        public enum CutsceneTypeEnum { CameraCutscene, PlayerCutscene }

        public TriggerTypeEnum TriggerType;
        public CutsceneTypeEnum CutsceneType;
        public PlayableDirector Cutscene;
        public CutscenePlayer CutscenePlayer;

        public CinemachineVirtualCamera CutsceneCamera;
        public float CutsceneFadeSpeed;

        public CinemachineBlendDefinition BlendDefinition;
        [Tooltip("This is the asset that contains custom settings for blends between specific virtual cameras in your scene.")]
        public CinemachineBlenderSettings CustomBlendAsset;

        [Tooltip("Wait for the dialogue to finish before starting the cutscene.")]
        public bool WaitForDialogue = true;
        [Tooltip("Wait for the camera to blend into cutscene camera before starting the cutscene.")]
        public bool WaitForBlendIn = true;

        [Tooltip("The time offset at which the cutscene starts during the camera blend.")]
        [Range(0f, 1f)] public float BlendInOffset = 1f;
        [Tooltip("The time at which the cutscene camera blends in to player camera.")]
        public float BlendOutTime = 1f;

        public UnityEvent OnCutsceneStart;
        public UnityEvent OnCutsceneEnd;

        private DialogueSystem dialogueSystem;
        private CutsceneModule cutscene;
        private bool isPlayed;

        private void Awake()
        {
            cutscene = GameManager.Module<CutsceneModule>();
            dialogueSystem = DialogueSystem.Instance;
        }

        private void Start()
        {
            // rebuild playable graph to ensure seamless transition
            if (Cutscene != null) Cutscene.RebuildGraph();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType != TriggerTypeEnum.Trigger)
                return;

            if (other.CompareTag("Player"))
                TriggerCutscene();
        }

        public void InteractStart()
        {
            if (TriggerType != TriggerTypeEnum.Interact)
                return;

            TriggerCutscene();
        }

        public void TriggerCutscene()
        {
            if (Cutscene == null || isPlayed || (WaitForDialogue && dialogueSystem.IsPlaying))
                return;

            cutscene.PlayCutscene(this);
            OnCutsceneStart?.Invoke();
            isPlayed = true;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isPlayed), isPlayed }
            };
        }

        public void OnLoad(JToken data)
        {
            isPlayed = (bool)data[nameof(isPlayed)];
        }
    }
}