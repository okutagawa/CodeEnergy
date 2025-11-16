using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class ProjectNpcProvider
{
    public static List<NPCIdentity> GetAllFromResources()
    {
        var gos = Resources.LoadAll<GameObject>("NPCPrefabs");
        var list = new List<NPCIdentity>();
        foreach (var go in gos)
        {
            var id = go.GetComponent<NPCIdentity>();
            if (id != null) list.Add(id);
            else
            {
                // если в prefab нет NPCIdentity, можно создать временную или пропустить
            }
        }
        return list;
    }
}
