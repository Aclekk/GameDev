using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Exiting Play Mode (Editor)");
#else
        Application.Quit();
        Debug.Log("Quitting Application");
#endif
    }
}
