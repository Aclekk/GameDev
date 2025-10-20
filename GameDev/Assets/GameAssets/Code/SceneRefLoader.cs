// SceneRefLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneRefLoader : MonoBehaviour
{
    #if UNITY_EDITOR
    public UnityEditor.SceneAsset sceneAsset;
    #endif

    [SerializeField] private string scenePath;   // ex: "Assets/Scenes/MainMenu.unity"
    [SerializeField] private string sceneName;   // ex: "MainMenu"

    #if UNITY_EDITOR
    void OnValidate()
    {
        if (sceneAsset)
        {
            scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
            sceneName = sceneAsset.name;
        }
        else
        {
            scenePath = null;
            sceneName = null;
        }
    }
    #endif

    public void Load()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(sceneName) && SceneExistsByName(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogWarning("[SceneRefLoader] Scene belum di-assign (name/path).");
            return;
        }

        int buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);
        if (buildIndex < 0)
        {
            Debug.LogError("[SceneRefLoader] Scene belum ada di File > Build Settings.");
            return;
        }

        SceneManager.LoadScene(buildIndex);
    }

    static bool SceneExistsByName(string name)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string n = Path.GetFileNameWithoutExtension(path);
            if (n == name) return true;
        }
        return false;
    }
}
