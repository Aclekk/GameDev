using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneRefLoader : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Scene Reference (Editor Only)")]
    public UnityEditor.SceneAsset sceneAsset;
#endif

    [SerializeField, Tooltip("Path lengkap ke scene, ex: Assets/Scenes/MainMenu.unity")]
    private string scenePath;

    [SerializeField, Tooltip("Nama scene, ex: MainMenu")]
    private string sceneName;

#if UNITY_EDITOR
    // Otomatis isi nama dan path saat diubah di editor
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

    // 🔹 Fungsi utama untuk load scene
    public void Load()
    {
        Debug.Log($"[SceneRefLoader] Load called. sceneName = {sceneName}, scenePath = {scenePath}");

        // Pastikan game jalan normal (misal dari pause menu)
        Time.timeScale = 1f;

        // 🔸 1. Coba load berdasarkan nama (lebih cepat & aman)
        if (!string.IsNullOrEmpty(sceneName) && SceneExistsByName(sceneName))
        {
            Debug.Log($"[SceneRefLoader] Loading scene by name: {sceneName}");
            SceneManager.LoadScene(sceneName);
            return;
        }

        // 🔸 2. Kalau nama kosong, coba lewat path
        if (!string.IsNullOrEmpty(scenePath))
        {
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);
            Debug.Log($"[SceneRefLoader] buildIndex dari path: {buildIndex}");

            if (buildIndex >= 0)
            {
                SceneManager.LoadScene(buildIndex);
                return;
            }
            else
            {
                Debug.LogError("[SceneRefLoader] Scene belum ditambahkan di File > Build Settings!");
                return;
            }
        }

        // 🔸 3. Kalau dua-duanya kosong → gagal
        Debug.LogWarning("[SceneRefLoader] Scene belum di-assign (name/path kosong). Cek Inspector.");
    }

    // 🔹 Cek apakah scene dengan nama tertentu ada di Build Settings
    static bool SceneExistsByName(string name)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string n = Path.GetFileNameWithoutExtension(path);
            if (n == name)
                return true;
        }
        return false;
    }
}
