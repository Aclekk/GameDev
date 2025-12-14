using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MansionOrganizer : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Mansion Organization")]
    [Tooltip("GameObject root Mansion yang akan diorganize")]
    public Transform mansionRoot;

    [ContextMenu("Organize Mansion Hierarchy")]
    public void OrganizeMansion()
    {
        if (mansionRoot == null)
        {
            Debug.LogError("Mansion Root belum di-assign!");
            return;
        }

        Debug.Log($"Starting organization of {mansionRoot.name}...");

        Transform floorsFolder = GetOrCreateFolder(mansionRoot, "Floors");
        Transform wallsFolder = GetOrCreateFolder(mansionRoot, "Walls");
        Transform windowsFolder = GetOrCreateFolder(mansionRoot, "Windows");
        Transform roofsFolder = GetOrCreateFolder(mansionRoot, "Roofs");
        Transform baseFolder = GetOrCreateFolder(mansionRoot, "Base");
        Transform topFolder = GetOrCreateFolder(mansionRoot, "Top");
        Transform decorationsFolder = GetOrCreateFolder(mansionRoot, "Decorations");
        Transform cornersFolder = GetOrCreateFolder(mansionRoot, "Corners");

        int movedCount = 0;

        Transform[] children = new Transform[mansionRoot.childCount];
        for (int i = 0; i < mansionRoot.childCount; i++)
        {
            children[i] = mansionRoot.GetChild(i);
        }

        foreach (Transform child in children)
        {
            if (child == floorsFolder || child == wallsFolder || child == windowsFolder || 
                child == roofsFolder || child == baseFolder || child == topFolder || 
                child == decorationsFolder || child == cornersFolder)
                continue;

            string childName = child.name;

            if (childName.Contains("Floor"))
            {
                child.SetParent(floorsFolder);
                movedCount++;
            }
            else if (childName.Contains("Wall"))
            {
                child.SetParent(wallsFolder);
                movedCount++;
            }
            else if (childName.Contains("Window"))
            {
                child.SetParent(windowsFolder);
                movedCount++;
            }
            else if (childName.Contains("Roof"))
            {
                child.SetParent(roofsFolder);
                movedCount++;
            }
            else if (childName.Contains("Base"))
            {
                child.SetParent(baseFolder);
                movedCount++;
            }
            else if (childName.Contains("Top"))
            {
                child.SetParent(topFolder);
                movedCount++;
            }
            else if (childName.Contains("Deco") || childName.Contains("Column") || childName.Contains("RoofPiece"))
            {
                child.SetParent(decorationsFolder);
                movedCount++;
            }
            else if (childName.Contains("Cor"))
            {
                child.SetParent(cornersFolder);
                movedCount++;
            }
        }

        EditorUtility.SetDirty(mansionRoot.gameObject);
        Debug.Log($"✅ Mansion organized! Moved {movedCount} objects into {GetNonEmptyFolderCount()} folders.");
    }

    Transform GetOrCreateFolder(Transform parent, string folderName)
    {
        Transform existing = parent.Find(folderName);
        if (existing != null)
            return existing;

        GameObject newFolder = new GameObject(folderName);
        newFolder.transform.SetParent(parent);
        newFolder.transform.localPosition = Vector3.zero;
        newFolder.transform.localRotation = Quaternion.identity;
        newFolder.transform.localScale = Vector3.one;

        return newFolder.transform;
    }

    int GetNonEmptyFolderCount()
    {
        int count = 0;
        foreach (Transform child in mansionRoot)
        {
            if (child.childCount > 0)
                count++;
        }
        return count;
    }

    [ContextMenu("Reset Organization (Flatten)")]
    public void ResetOrganization()
    {
        if (mansionRoot == null)
        {
            Debug.LogError("Mansion Root belum di-assign!");
            return;
        }

        string[] folderNames = { "Floors", "Walls", "Windows", "Roofs", "Base", "Top", "Decorations", "Corners" };

        foreach (string folderName in folderNames)
        {
            Transform folder = mansionRoot.Find(folderName);
            if (folder == null) continue;

            Transform[] children = new Transform[folder.childCount];
            for (int i = 0; i < folder.childCount; i++)
            {
                children[i] = folder.GetChild(i);
            }

            foreach (Transform child in children)
            {
                child.SetParent(mansionRoot);
            }

            DestroyImmediate(folder.gameObject);
        }

        EditorUtility.SetDirty(mansionRoot.gameObject);
        Debug.Log("✅ Mansion hierarchy flattened (reset to original)");
    }
#endif
}
