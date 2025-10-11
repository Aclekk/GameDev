using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuPlayOnly : MonoBehaviour
{
    // Ganti sesuai nama scene kamu — kamu bilang "Game"
    [SerializeField] string gameSceneName = "Game";

    public void OnPlay()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("Nama scene belum diisi!");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }
}
