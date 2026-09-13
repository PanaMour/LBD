using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mirror;

public static class FixSceneIds
{
    [MenuItem("Tools/Mirror/Fix Missing Scene IDs In Open Scene")]
    static void FixSceneIdsInOpenScene()
    {
        NetworkIdentity[] identities = Object.FindObjectsOfType<NetworkIdentity>(true);
        int count = 0;

        foreach (NetworkIdentity identity in identities)
        {
            identity.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
            count++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[FixSceneIds] Re-validated {count} NetworkIdentity object(s) in scene '{EditorSceneManager.GetActiveScene().name}'. Save the scene now (Ctrl+S).");
    }
}
