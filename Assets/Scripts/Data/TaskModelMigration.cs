using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyGame.Models;
using MyGame.Data;

public static class TaskModelMigration
{
    // ѕопытатьс€ разрешить старые string имена в guid, если guid пустые
    public static void MigrateNamesToGuids(List<TaskModel> tasks)
    {
        if (tasks == null || tasks.Count == 0) return;
        // rebuild registry to ensure up?to?date
        SceneNpcRegistry.Instance.BuildIndex();

        foreach (var t in tasks)
        {
            if (string.IsNullOrEmpty(t.giverNpcGuid) && !string.IsNullOrEmpty(t.giverNpcGuid))
            {
                var found = SceneNpcRegistry.Instance.FindByName(t.giverNpcGuid);
                if (found != null) t.giverNpcGuid = found.Guid;
            }

            if (string.IsNullOrEmpty(t.receiverNpcGuid) && !string.IsNullOrEmpty(t.receiverNpcGuid))
            {
                var found = SceneNpcRegistry.Instance.FindByName(t.receiverNpcGuid);
                if (found != null) t.receiverNpcGuid = found.Guid;
            }
        }
    }
}
