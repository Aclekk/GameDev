using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuPlayOnly : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Game";

    public void OnPlay()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("Nama scene belum diisi!");
            return;
        }

        PlayerPrefs.DeleteKey("TutorialShown"); // Reset tutorial biar muncul lagi
        SceneManager.LoadScene(gameSceneName);
    }
}
