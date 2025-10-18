using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRefLoader : MonoBehaviour
{
    // Tarik (drag) asset scene .unity ke sini di Inspector (Editor only)
    #if UNITY_EDITOR
    public UnityEditor.SceneAsset sceneAsset;
    #endif

    // Disimpan sebagai path agar tetap bekerja saat build
    [SerializeField] private string scenePath;

    #if UNITY_EDITOR
    // Setiap berubah di Inspector, simpan path scene
    void OnValidate()
    {
        scenePath = sceneAsset ? UnityEditor.AssetDatabase.GetAssetPath(sceneAsset) : null;
    }
    #endif

    // Panggil dari Button OnClick()
    public void Load()
    {
        Time.timeScale = 1f;  // pastikan un-pause

        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogWarning("[SceneRefLoader] Scene belum di-assign.");
            return;
        }

        // scene harus sudah ditambahkan ke Build Settings
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);
        if (buildIndex < 0)
        {
            Debug.LogError("[SceneRefLoader] Scene belum ada di File > Build Settings.");
            return;
        }

        SceneManager.LoadScene(buildIndex);
    }
}
