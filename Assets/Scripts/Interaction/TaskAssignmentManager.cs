using UnityEngine;
using System.Collections.Generic;
using MyGame.Models;
using MyGame.Data;

public class TaskAssignmentManager : MonoBehaviour
{
    // Отдельные очереди для ролей (ключи — string, это удобнее для SceneNpcRegistry.FindByGuid)
    private readonly Dictionary<string, List<TaskModel>> _giverTasksByNpc = new Dictionary<string, List<TaskModel>>();
    private readonly Dictionary<string, List<TaskModel>> _receiverTasksByNpc = new Dictionary<string, List<TaskModel>>();

    void Start()
    {
        Debug.Log("[TAM] Start: building NPC index");
        SceneNpcRegistry.Instance.BuildIndex();

        var tasks = DataManager.LoadTasks();
        if (tasks == null || tasks.Count == 0)
        {
            Debug.Log("[TAM] no tasks loaded");
            return;
        }

        Debug.Log($"[TAM] Loaded {tasks.Count} task(s) from DataManager");
        // Разделяем задачи по ролям
        foreach (var t in tasks)
        {
            // Защита: если поля GUID у тебя не string, приводим к строке безопасно
            string giverGuidStr = t.giverNpcGuid != null ? t.giverNpcGuid.ToString() : null;
            string receiverGuidStr = t.receiverNpcGuid != null ? t.receiverNpcGuid.ToString() : null;

            Debug.Log($"[TAM] Task id={t.id} giver={giverGuidStr} receiver={receiverGuidStr} title={t.title}");

            if (!string.IsNullOrEmpty(giverGuidStr))
                AddToDictQueue(_giverTasksByNpc, giverGuidStr, t);

            if (!string.IsNullOrEmpty(receiverGuidStr))
                AddToDictQueue(_receiverTasksByNpc, receiverGuidStr, t);
        }

        // Обновим NPC: для каждого GUID, который есть в любой из двух словарей
        var allGuids = new HashSet<string>();
        foreach (var k in _giverTasksByNpc.Keys) allGuids.Add(k);
        foreach (var k in _receiverTasksByNpc.Keys) allGuids.Add(k);

        foreach (var npcGuid in allGuids)
        {
            var giverPeek = PeekFromDict(_giverTasksByNpc, npcGuid);
            var receiverPeek = PeekFromDict(_receiverTasksByNpc, npcGuid);

            Debug.Log($"[TAM] NPC guid={npcGuid} giverPeek={giverPeek?.id} receiverPeek={receiverPeek?.id}");

            var npcObj = SceneNpcRegistry.Instance.FindByGuid(npcGuid);
            if (npcObj == null)
            {
                Debug.LogWarning($"[TAM] SceneNpcRegistry: guid {npcGuid} not found in scene");
                continue;
            }
            var inter = npcObj.GetComponent<NPCInteractable>();
            if (inter == null)
            {
                Debug.LogWarning($"[TAM] NPC {npcGuid} has no NPCInteractable component");
                continue;
            }

            inter.SetAssignedTasks(peekForGiver: giverPeek, peekForReceiver: receiverPeek);
            Debug.Log($"[TAM] registered giverCount={GetCount(_giverTasksByNpc, npcGuid)} receiverCount={GetCount(_receiverTasksByNpc, npcGuid)} for NPC {npcGuid}");
        }
    }

    private void AddToDictQueue(Dictionary<string, List<TaskModel>> dict, string npcGuid, TaskModel task)
    {
        if (string.IsNullOrEmpty(npcGuid) || task == null) return;
        if (!dict.ContainsKey(npcGuid)) dict[npcGuid] = new List<TaskModel>();
        dict[npcGuid].Add(task);
        Debug.Log($"[TAM] Added task {task.id} to {(dict == _giverTasksByNpc ? "GIVER" : "RECEIVER")} queue of {npcGuid} (newCount={dict[npcGuid].Count})");
    }

    private TaskModel PeekFromDict(Dictionary<string, List<TaskModel>> dict, string npcGuid)
    {
        if (string.IsNullOrEmpty(npcGuid)) return null;
        if (!dict.TryGetValue(npcGuid, out var list) || list == null || list.Count == 0) return null;
        return list[0];
    }

    private int GetCount(Dictionary<string, List<TaskModel>> dict, string npcGuid)
    {
        if (string.IsNullOrEmpty(npcGuid)) return 0;
        return dict.TryGetValue(npcGuid, out var list) && list != null ? list.Count : 0;
    }

    // Возвращает и удаляет первый элемент очереди для receiver (обычно ConfirmStartTask вызывает это)
    public TaskModel GetNextForReceiver(string receiverNpcGuid)
    {
        if (string.IsNullOrEmpty(receiverNpcGuid)) return null;
        if (!_receiverTasksByNpc.TryGetValue(receiverNpcGuid, out var rList) || rList == null || rList.Count == 0) return null;

        var task = rList[0];
        rList.RemoveAt(0);
        Debug.Log($"[TAM] GetNextForReceiver: removed task {task.id} from receiver {receiverNpcGuid} (remainingReceiver={rList.Count})");

        // Удаляем ту же задачу из очереди giver, если указан giverGuid
        // Приводим giverGuid к строке безопасно
        string giverGuidStr = task.giverNpcGuid != null ? task.giverNpcGuid.ToString() : null;
        if (!string.IsNullOrEmpty(giverGuidStr))
        {
            // task.id может быть int или string — используем object-совместимое сравнение
            bool removed = RemoveTaskFromGiverQueue(giverGuidStr, task.id);
            Debug.Log($"[TAM] RemoveTaskFromGiverQueue giver={giverGuidStr} task={task.id} removed={removed}");
        }

        // Обновляем peek у receiver и у giver (если есть)
        UpdateNpcPeeks(receiverNpcGuid);
        if (!string.IsNullOrEmpty(giverGuidStr))
            UpdateNpcPeeks(giverGuidStr);

        return task;
    }

    // Теперь taskId имеет тип object-совместимый (используем dynamic-совместимость через object)
    // Но лучше — привести к типу, который у тебя в TaskModel.id. Здесь предполагаем, что id — int или string.
    private bool RemoveTaskFromGiverQueue(string giverNpcGuid, object taskId)
    {
        if (string.IsNullOrEmpty(giverNpcGuid) || taskId == null) return false;
        if (!_giverTasksByNpc.TryGetValue(giverNpcGuid, out var gList) || gList == null || gList.Count == 0) return false;

        for (int i = 0; i < gList.Count; i++)
        {
            var candidate = gList[i];
            if (candidate == null) continue;

            // Сравниваем безопасно: если оба числа — сравниваем как int, иначе как string
            // Попробуем привести оба к string и сравнить — это универсально и безопасно
            var candIdStr = candidate.id != null ? candidate.id.ToString() : null;
            var taskIdStr = taskId != null ? taskId.ToString() : null;

            if (!string.IsNullOrEmpty(candIdStr) && candIdStr == taskIdStr)
            {
                gList.RemoveAt(i);
                Debug.Log($"[TAM] RemoveTaskFromGiverQueue: removed task {taskIdStr} from giver {giverNpcGuid} (remainingGiver={gList.Count})");
                return true;
            }
        }
        return false;
    }

    private void UpdateNpcPeeks(string npcGuid)
    {
        if (string.IsNullOrEmpty(npcGuid)) return;
        var giverPeek = PeekFromDict(_giverTasksByNpc, npcGuid);
        var receiverPeek = PeekFromDict(_receiverTasksByNpc, npcGuid);

        var npcObj = SceneNpcRegistry.Instance.FindByGuid(npcGuid);
        if (npcObj == null) { Debug.LogWarning($"[TAM] UpdateNpcPeeks: npc {npcGuid} not found"); return; }
        var inter = npcObj.GetComponent<NPCInteractable>();
        if (inter == null) { Debug.LogWarning($"[TAM] UpdateNpcPeeks: NPCInteractable missing on {npcGuid}"); return; }

        inter.SetAssignedTasks(peekForGiver: giverPeek, peekForReceiver: receiverPeek);
        Debug.Log($"[TAM] UpdateNpcPeeks: npc={npcGuid} giverPeek={giverPeek?.id} receiverPeek={receiverPeek?.id}");
    }

    // Если нужен GetNext для giver — можно добавить аналогично
    public TaskModel GetNextForGiver(string npcGuid)
    {
        if (string.IsNullOrEmpty(npcGuid)) return null;
        if (!_giverTasksByNpc.TryGetValue(npcGuid, out var list) || list == null || list.Count == 0) return null;
        var t = list[0];
        list.RemoveAt(0);
        Debug.Log($"[TAM] GetNextForGiver: removed task {t.id} from giver {npcGuid} (remaining={list.Count})");

        var npcObj = SceneNpcRegistry.Instance.FindByGuid(npcGuid);
        if (npcObj != null)
        {
            var inter = npcObj.GetComponent<NPCInteractable>();
            if (inter != null)
            {
                var newGiverPeek = PeekFromDict(_giverTasksByNpc, npcGuid);
                var newReceiverPeek = PeekFromDict(_receiverTasksByNpc, npcGuid);
                inter.SetAssignedTasks(peekForGiver: newGiverPeek, peekForReceiver: newReceiverPeek);
                Debug.Log($"[TAM] Updated NPC {npcGuid} peek -> giver:{newGiverPeek?.id} receiver:{newReceiverPeek?.id}");
            }
        }

        return t;
    }

    public bool HasReceiverTasks(string npcGuid)
    {
        return !string.IsNullOrEmpty(npcGuid) && _receiverTasksByNpc.TryGetValue(npcGuid, out var list) && list != null && list.Count > 0;
    }

    public bool HasGiverTasks(string npcGuid)
    {
        return !string.IsNullOrEmpty(npcGuid) && _giverTasksByNpc.TryGetValue(npcGuid, out var list) && list != null && list.Count > 0;
    }
}
