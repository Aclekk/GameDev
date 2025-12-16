using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace UHFPS.Runtime
{
    public class CutsceneModule : ManagerModule
    {
        private PlayableDirector currentCutscene;
        private CutsceneTrigger currentTrigger;

        private CinemachineBrain cinemachineBrain;
        private CinemachineBlendDefinition defaultBlend;
        private CinemachineBlenderSettings defaultBlendAsset;

        public override string Name => "Cutscene";

        public override void OnAwake()
        {
            cinemachineBrain = PlayerPresence.PlayerManager.MainCamera.GetComponent<CinemachineBrain>();
        }

        public void PlayCutscene(CutsceneTrigger cutsceneTrigger)
        {
            currentCutscene = cutsceneTrigger.Cutscene;
            currentTrigger = cutsceneTrigger;

            // freeze player and disable game panels
            PlayerPresence.FreezePlayer(true);
            PlayerPresence.PlayerIsUnlocked = false;
            PlayerPresence.PlayerManager.PlayerItems.DeactivateCurrentItem();
            GameManager.DisableAllGamePanels();

            if (cutsceneTrigger.CutsceneType == CutsceneTrigger.CutsceneTypeEnum.CameraCutscene)
            {
                RunCoroutine(OnPlayCameraCutscene());
            }
            else
            {
                defaultBlend = cinemachineBrain.m_DefaultBlend;
                defaultBlendAsset = cinemachineBrain.m_CustomBlends;

                cinemachineBrain.m_DefaultBlend = cutsceneTrigger.BlendDefinition;
                cinemachineBrain.m_CustomBlends = cutsceneTrigger.CustomBlendAsset;

                // activate player cutscene camera and disable player
                cutsceneTrigger.CutscenePlayer.SetCutsceneActive(true);
                PlayerPresence.Player.SetActive(false);

                // start cutscene
                currentCutscene.stopped += OnPlayerCutsceneStopped;
                RunCoroutine(OnPlayPlayerCutscene(true));
            }

            cutsceneTrigger.OnCutsceneStart?.Invoke();
        }

        IEnumerator OnPlayPlayerCutscene(bool blendIn)
        {
            if (currentTrigger.BlendDefinition.m_Style != CinemachineBlendDefinition.Style.Cut)
            {
                if (blendIn && !currentTrigger.WaitForBlendIn)
                    currentCutscene.Play();
                else if (!blendIn)
                {
                    PlayerPresence.Player.SetActive(true);
                    currentTrigger.CutscenePlayer.SetCutsceneActive(false);
                }

                yield return new WaitUntil(() => cinemachineBrain.ActiveBlend != null);
                CinemachineBlend blend = cinemachineBrain.ActiveBlend;

                float blendTarget = blendIn ? currentTrigger.WaitForBlendIn ? currentTrigger.BlendInOffset : 1f : 1f;
                while (blend != null && !blend.IsComplete)
                {
                    if (blend.BlendWeight >= blendTarget)
                        break;

                    yield return null;
                }
            }

            if (blendIn)
            {
                if (currentTrigger.WaitForBlendIn) currentCutscene.Play();
            }
            else
            {
                cinemachineBrain.m_DefaultBlend = defaultBlend;
                cinemachineBrain.m_CustomBlends = defaultBlendAsset;
                OnCutsceneEnd();
            }
        }

        IEnumerator OnPlayCameraCutscene()
        {
            GameObject cutsceneCamera = currentTrigger.CutsceneCamera.gameObject;
            yield return PlayerPresence.SwitchCamera(cutsceneCamera, currentTrigger.CutsceneFadeSpeed);

            currentCutscene.Play();

            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds((float)currentCutscene.duration);
            yield return PlayerPresence.SwitchCamera(null, currentTrigger.CutsceneFadeSpeed);

            OnCutsceneEnd();
        }

        private void OnPlayerCutsceneStopped(PlayableDirector _)
        {
            // apply player position and look
            ApplyPlayerTransform();

            // start blend out player cutscene
            cinemachineBrain.m_DefaultBlend.m_Time = currentTrigger.BlendOutTime;
            RunCoroutine(OnPlayPlayerCutscene(false));

            // dispose stopped event
            currentCutscene.stopped -= OnPlayerCutsceneStopped;
        }

        private void ApplyPlayerTransform()
        {
            Vector3 newPosition = currentTrigger.CutscenePlayer.transform.position;
            Vector3 currentLook = currentTrigger.CutscenePlayer.HeadCamera.transform.eulerAngles;
            Vector3 lookForward = currentTrigger.CutscenePlayer.HeadCamera.transform.forward;

            Vector2 newLook = new(currentLook.y, 0f);
            newPosition += lookForward.normalized * 0.5f;

            PlayerPresence.SetPlayerPositionAndLook(newPosition, newLook);
        }

        private void OnCutsceneEnd()
        {
            // unfreeze player and show main panel
            PlayerPresence.FreezePlayer(false);
            PlayerPresence.PlayerIsUnlocked = true;
            GameManager.ShowPanel(GameManager.PanelType.MainPanel);

            currentTrigger.OnCutsceneEnd?.Invoke();
            currentCutscene = null;
            currentTrigger = null;
        }
    }
}