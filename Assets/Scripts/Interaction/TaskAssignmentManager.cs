using UnityEngine;
using System.Collections.Generic;
using MyGame.Models;
using MyGame.Data;

public class TaskAssignmentManager : MonoBehaviour
{
    void Start()
    {
        // Построим индекс NPC в сцене (если используешь SceneNpcRegistry)
        SceneNpcRegistry.Instance.BuildIndex();

        // Загрузим задачи
        var tasks = DataManager.LoadTasks();
        if (tasks == null || tasks.Count == 0)
        {
            Debug.Log("TaskAssignmentManager: no tasks loaded");
            return;
        }

        // Пройдемся по всем задачам и назначим их на NPC по GUID
        foreach (var t in tasks)
        {
            if (!string.IsNullOrEmpty(t.giverNpcGuid))
            {
                var giver = SceneNpcRegistry.Instance.FindByGuid(t.giverNpcGuid);
                if (giver != null)
                {
                    var inter = giver.GetComponent<NPCInteractable>();
                    if (inter != null)
                    {
                        inter.assignedGiverTask = t;
                        Debug.Log($"Assigned Giver task {t.id} -> {giver.DisplayName}");
                    }
                }
                else Debug.LogWarning($"TaskAssignmentManager: giver guid {t.giverNpcGuid} not found");
            }

            if (!string.IsNullOrEmpty(t.receiverNpcGuid))
            {
                var receiver = SceneNpcRegistry.Instance.FindByGuid(t.receiverNpcGuid);
                if (receiver != null)
                {
                    var inter = receiver.GetComponent<NPCInteractable>();
                    if (inter != null)
                    {
                        inter.assignedReceiverTask = t;
                        Debug.Log($"Assigned Receiver task id={t.id} to NPC {receiver.DisplayName} (guid={receiver.Guid}) textForReceiver='{t.textForReceiver}'");
                    }
                    else Debug.LogWarning($"No NPCInteractable on receiver {receiver.DisplayName} (guid={receiver.Guid})");
                }
            }
        }
    }
}
