using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    [SerializeField] string sceneName = "GameKEL9";

    public void LoadGameScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Nama scene belum diisi!");
            return;
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}


