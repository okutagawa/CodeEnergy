#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class PrintAllNpcIdentities
{
    [MenuItem("Tools/Print All NPC Identities")]
    public static void PrintAll()
    {
        var all = UnityEngine.Object.FindObjectsOfType<NPCIdentity>(true);
        Debug.Log($"[Tool] Found {all.Length} NPCIdentity components in scene.");
        foreach (var n in all)
        {
            Debug.Log($"[Tool] name={n.gameObject.name} displayName={n.DisplayName} guid={n.Guid} active={n.gameObject.activeInHierarchy} scene={n.gameObject.scene.name}");
        }
    }
}
#endif
