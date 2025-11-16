using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    // –еконструировать индекс Ч вызывает FindObjectsOfType
    public void BuildIndex()
    {
        byGuid.Clear();
        var all = FindObjectsOfType<NPCIdentity>();
        foreach (var n in all)
        {
            if (string.IsNullOrEmpty(n.Guid)) continue;
            if (!byGuid.ContainsKey(n.Guid)) byGuid[n.Guid] = n;
        }
    }

    public NPCIdentity FindByGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        byGuid.TryGetValue(guid, out var v);
        return v;
    }

    public NPCIdentity FindByName(string name)
    {
        return FindObjectsOfType<NPCIdentity>().FirstOrDefault(n => n.DisplayName == name || n.gameObject.name == name);
    }

    public List<NPCIdentity> GetAll() => FindObjectsOfType<NPCIdentity>().ToList();
}
