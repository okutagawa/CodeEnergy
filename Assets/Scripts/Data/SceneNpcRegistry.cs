using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SceneNpcRegistry : MonoBehaviour
{
    private static SceneNpcRegistry instance;
    public static SceneNpcRegistry Instance
    {
        get
        {
            if (instance == null)
            {
                var found = FindObjectOfType<SceneNpcRegistry>();
                if (found == null)
                {
                    var go = new GameObject("SceneNpcRegistry");
                    instance = go.AddComponent<SceneNpcRegistry>();
                }
                else instance = found;
                instance.BuildIndex();
            }
            return instance;
        }
    }

    private Dictionary<string, NPCIdentity> byGuid = new Dictionary<string, NPCIdentity>();

    // Ѕолее надЄжна€ индексаци€: ищем даже неактивные объекты и логируем
    public void BuildIndex()
    {
        byGuid.Clear();
        // true Ч include inactive
        var all = FindObjectsOfType<NPCIdentity>(true);
        Debug.Log($"[SceneNpcRegistry] BuildIndex: found {all.Length} NPCIdentity components (including inactive).");
        foreach (var n in all)
        {
            if (string.IsNullOrEmpty(n.Guid))
            {
                Debug.LogWarning($"[SceneNpcRegistry] NPC {n.gameObject.name} has empty GUID Ч skip.");
                continue;
            }

            if (!byGuid.ContainsKey(n.Guid))
            {
                byGuid[n.Guid] = n;
                Debug.Log($"[SceneNpcRegistry] Registered NPC: name={n.gameObject.name} displayName={n.DisplayName} guid={n.Guid}");
            }
            else
            {
                Debug.LogWarning($"[SceneNpcRegistry] Duplicate GUID detected: guid={n.Guid} on object {n.gameObject.name}. Existing object: {byGuid[n.Guid].gameObject.name}");
            }
        }
    }

    // ”добный метод дл€ ручного пересоздани€ индекса
    public void RebuildIndex()
    {
        BuildIndex();
    }

    public NPCIdentity FindByGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        byGuid.TryGetValue(guid, out var v);
        return v;
    }

    public NPCIdentity FindByName(string name)
    {
        return FindObjectsOfType<NPCIdentity>(true).FirstOrDefault(n => n.DisplayName == name || n.gameObject.name == name);
    }

    public List<NPCIdentity> GetAll() => FindObjectsOfType<NPCIdentity>(true).ToList();
}
