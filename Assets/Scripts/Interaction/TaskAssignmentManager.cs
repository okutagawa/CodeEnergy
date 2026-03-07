using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyGame.Models;
using MyGame.Data;

public class TaskAssignmentManager : MonoBehaviour
{
    public static TaskAssignmentManager Instance { get; private set; }

    private readonly Dictionary<string, List<TaskModel>> _giverTasksByNpc = new Dictionary<string, List<TaskModel>>();
    private readonly Dictionary<string, List<TaskModel>> _receiverTasksByNpc = new Dictionary<string, List<TaskModel>>();

    public static event Action<TaskModel> OnActiveQuestChanged;

    void Awake()
    {
        Instance = this;
        if (GameState.Instance == null)
        {
            GameState.EnsureExists();
        }
    }

    void Start()
    {
        Debug.Log("[TAM] Start: loading GameState and building NPC index");
        if (GameState.Instance == null)
        {
            Debug.LogWarning("[TAM] GameState.Instance is null at Start()");
        }
        else
        {
            GameState.Instance.LoadState();
        }

        SceneNpcRegistry.Instance.BuildIndex();

        var tasks = DataManager.LoadTasks();
        if (tasks == null || tasks.Count == 0)
        {
            Debug.Log("[TAM] no tasks loaded");
            return;
        }

        Debug.Log($"[TAM] Loaded {tasks.Count} task(s) from DataManager");
        foreach (var t in tasks)
        {
            string giverGuidStr = !string.IsNullOrEmpty(t.giverNpcGuid) ? t.giverNpcGuid : null;
            string receiverGuidStr = !string.IsNullOrEmpty(t.receiverNpcGuid) ? t.receiverNpcGuid : null;

            Debug.Log($"[TAM] Task id={t.id} giver={giverGuidStr} receiver={receiverGuidStr} title={t.title}");

            if (!string.IsNullOrEmpty(giverGuidStr))
                AddToDictQueue(_giverTasksByNpc, giverGuidStr, t);

            if (!string.IsNullOrEmpty(receiverGuidStr))
                AddToDictQueue(_receiverTasksByNpc, receiverGuidStr, t);
        }

        var state = GameState.Instance?.GetData();
        if (state != null)
        {
            ApplySavedQueuesAndCompletedTasks(state, tasks);
        }

        var allGuids = new HashSet<string>();
        foreach (var k in _giverTasksByNpc.Keys) allGuids.Add(k);
        foreach (var k in _receiverTasksByNpc.Keys) allGuids.Add(k);

        foreach (var npcGuid in allGuids)
        {
            var giverPeek = PeekFromDict(_giverTasksByNpc, npcGuid);
            var receiverPeek = PeekFromDict(_receiverTasksByNpc, npcGuid);

            Debug.Log($"[TAM] NPC guid={npcGuid} giverPeek={giverPeek?.id} receiverPeek={receiverPeek?.id}");

            var npcIdentity = SceneNpcRegistry.Instance.FindByGuid(npcGuid);
            if (npcIdentity == null)
            {
                Debug.LogWarning($"[TAM] SceneNpcRegistry: guid {npcGuid} not found in scene");
                continue;
            }

            // »щем NPCInteractable устойчиво: self / parent / children
            var inter = npcIdentity.GetComponent<NPCInteractable>()
                        ?? npcIdentity.GetComponentInParent<NPCInteractable>()
                        ?? npcIdentity.GetComponentInChildren<NPCInteractable>();

            if (inter == null)
            {
                Debug.LogWarning($"[TAM] NPC {npcGuid} has no NPCInteractable component (checked self/parents/children)");
                continue;
            }

            inter.SetAssignedTasks(peekForGiver: giverPeek, peekForReceiver: receiverPeek);
            Debug.Log($"[TAM] registered giverCount={GetCount(_giverTasksByNpc, npcGuid)} receiverCount={GetCount(_receiverTasksByNpc, npcGuid)} for NPC {npcGuid}");
        }

        // ”ведомл€ем UI один раз о глобальном активном задании (не вызываем с null)
        NotifyGlobalActiveQuestChanged();
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

    public TaskModel GetNextForReceiver(string receiverNpcGuid)
    {
        if (string.IsNullOrEmpty(receiverNpcGuid)) return null;
        if (!_receiverTasksByNpc.TryGetValue(receiverNpcGuid, out var rList) || rList == null || rList.Count == 0) return null;

        var task = rList[0];
        rList.RemoveAt(0);
        Debug.Log($"[TAM] GetNextForReceiver: removed task {task.id} from receiver {receiverNpcGuid} (remainingReceiver={rList.Count})");

        string giverGuidStr = !string.IsNullOrEmpty(task.giverNpcGuid) ? task.giverNpcGuid : null;
        if (!string.IsNullOrEmpty(giverGuidStr))
        {
            bool removed = RemoveTaskFromGiverQueue(giverGuidStr, task.id);
            Debug.Log($"[TAM] RemoveTaskFromGiverQueue giver={giverGuidStr} task={task.id} removed={removed}");
        }

        // ќбновл€ем peeks и уведомл€ем UI только если есть активное задание
        UpdateNpcPeeks(receiverNpcGuid);
        if (!string.IsNullOrEmpty(giverGuidStr))
            UpdateNpcPeeks(giverGuidStr);

        GameState.Instance?.MarkTaskStarted(task.id);
        ExportQueuesToGameState();

        return task;
    }

    private bool RemoveTaskFromGiverQueue(string giverNpcGuid, object taskId)
    {
        if (string.IsNullOrEmpty(giverNpcGuid) || taskId == null) return false;
        if (!_giverTasksByNpc.TryGetValue(giverNpcGuid, out var gList) || gList == null || gList.Count == 0) return false;

        for (int i = 0; i < gList.Count; i++)
        {
            var candidate = gList[i];
            if (candidate == null) continue;

            var candIdStr = candidate.id.ToString();
            var taskIdStr = taskId.ToString();

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

        var npcIdentity = SceneNpcRegistry.Instance.FindByGuid(npcGuid);
        if (npcIdentity == null) { Debug.LogWarning($"[TAM] UpdateNpcPeeks: npc {npcGuid} not found"); return; }

        var inter = npcIdentity.GetComponent<NPCInteractable>()
                    ?? npcIdentity.GetComponentInParent<NPCInteractable>()
                    ?? npcIdentity.GetComponentInChildren<NPCInteractable>();

        if (inter == null) { Debug.LogWarning($"[TAM] UpdateNpcPeeks: NPCInteractable missing on {npcGuid}"); return; }

        inter.SetAssignedTasks(peekForGiver: giverPeek, peekForReceiver: receiverPeek);

        var active = receiverPeek ?? giverPeek;
        if (active != null) SafeInvokeActiveQuestChanged(active);

        Debug.Log($"[TAM] UpdateNpcPeeks: npc={npcGuid} giverPeek={giverPeek?.id} receiverPeek={receiverPeek?.id}");
    }

    public TaskModel GetNextForGiver(string npcGuid)
    {
        if (string.IsNullOrEmpty(npcGuid)) return null;
        if (!_giverTasksByNpc.TryGetValue(npcGuid, out var list) || list == null || list.Count == 0) return null;
        var t = list[0];
        list.RemoveAt(0);
        Debug.Log($"[TAM] GetNextForGiver: removed task {t.id} from giver {npcGuid} (remaining={list.Count})");

        var npcIdentity = SceneNpcRegistry.Instance.FindByGuid(npcGuid);
        if (npcIdentity != null)
        {
            var inter = npcIdentity.GetComponent<NPCInteractable>()
                        ?? npcIdentity.GetComponentInParent<NPCInteractable>()
                        ?? npcIdentity.GetComponentInChildren<NPCInteractable>();

            if (inter != null)
            {
                var newGiverPeek = PeekFromDict(_giverTasksByNpc, npcGuid);
                var newReceiverPeek = PeekFromDict(_receiverTasksByNpc, npcGuid);
                inter.SetAssignedTasks(peekForGiver: newGiverPeek, peekForReceiver: newReceiverPeek);

                var active = newReceiverPeek ?? newGiverPeek;
                if (active != null) SafeInvokeActiveQuestChanged(active);

                Debug.Log($"[TAM] Updated NPC {npcGuid} peek -> giver:{newGiverPeek?.id} receiver:{newReceiverPeek?.id}");
            }
        }

        ExportQueuesToGameState();

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

    private void ApplySavedQueuesAndCompletedTasks(GameStateData state, List<TaskModel> allTasks)
    {
        if (state == null) return;

        foreach (var doneId in state.completedTaskIds)
        {
            RemoveTaskFromAllQueuesById(doneId);
        }

        if (state.giverQueues != null && state.giverQueues.Count > 0)
        {
            _giverTasksByNpc.Clear();
            foreach (var entry in state.giverQueues)
            {
                var list = new List<TaskModel>();
                foreach (var tid in entry.taskIds)
                {
                    var t = allTasks.FirstOrDefault(x => x.id == tid);
                    if (t != null) list.Add(t);
                }
                if (list.Count > 0) _giverTasksByNpc[entry.npcGuid] = list;
            }
        }

        if (state.receiverQueues != null && state.receiverQueues.Count > 0)
        {
            _receiverTasksByNpc.Clear();
            foreach (var entry in state.receiverQueues)
            {
                var list = new List<TaskModel>();
                foreach (var tid in entry.taskIds)
                {
                    var t = allTasks.FirstOrDefault(x => x.id == tid);
                    if (t != null) list.Add(t);
                }
                if (list.Count > 0) _receiverTasksByNpc[entry.npcGuid] = list;
            }
        }

        Debug.Log("[TAM] Applied saved queues and removed completed tasks from in-memory queues.");
    }

    private void RemoveTaskFromAllQueuesById(int taskId)
    {
        foreach (var key in _giverTasksByNpc.Keys.ToList())
        {
            var list = _giverTasksByNpc[key];
            list.RemoveAll(t => t != null && t.id == taskId);
            if (list.Count == 0) _giverTasksByNpc.Remove(key);
        }

        foreach (var key in _receiverTasksByNpc.Keys.ToList())
        {
            var list = _receiverTasksByNpc[key];
            list.RemoveAll(t => t != null && t.id == taskId);
            if (list.Count == 0) _receiverTasksByNpc.Remove(key);
        }
    }

    public void ExportQueuesToGameState()
    {
        if (GameState.Instance == null) return;
        Debug.Log("[DEBUG] ExportQueuesToGameState: start");
        var data = GameState.Instance.GetData();
        data.giverQueues.Clear();
        foreach (var kv in _giverTasksByNpc)
        {
            var entry = new GameStateData.NpcQueueEntry { npcGuid = kv.Key, taskIds = kv.Value.Select(t => t.id).ToList() };
            data.giverQueues.Add(entry);
        }
        data.receiverQueues.Clear();
        foreach (var kv in _receiverTasksByNpc)
        {
            var entry = new GameStateData.NpcQueueEntry { npcGuid = kv.Key, taskIds = kv.Value.Select(t => t.id).ToList() };
            data.receiverQueues.Add(entry);
        }
        GameState.Instance.SaveState();
        Debug.Log("[DEBUG] ExportQueuesToGameState: done, giverQueues=" + data.giverQueues.Count + " receiverQueues=" + data.receiverQueues.Count);
    }

    // ----- Helpers for safe notifications -----
    private void NotifyGlobalActiveQuestChanged()
    {
        // find first receiver peek, then giver peek
        foreach (var kv in _receiverTasksByNpc)
        {
            var t = PeekFromDict(_receiverTasksByNpc, kv.Key);
            if (t != null)
            {
                SafeInvokeActiveQuestChanged(t);
                return;
            }
        }
        foreach (var kv in _giverTasksByNpc)
        {
            var t = PeekFromDict(_giverTasksByNpc, kv.Key);
            if (t != null)
            {
                SafeInvokeActiveQuestChanged(t);
                return;
            }
        }
        // nothing active Ч do not invoke with null to avoid hiding UI unexpectedly
    }

    private void SafeInvokeActiveQuestChanged(TaskModel active)
    {
        if (active == null) return;
        try
        {
            OnActiveQuestChanged?.Invoke(active);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TAM] Exception while invoking OnActiveQuestChanged: {ex}");
        }
    }

    // Optional helper for QuestPanel to query current active task
    public TaskModel GetGlobalActiveTask()
    {
        foreach (var kv in _receiverTasksByNpc)
        {
            var t = PeekFromDict(_receiverTasksByNpc, kv.Key);
            if (t != null) return t;
        }
        foreach (var kv in _giverTasksByNpc)
        {
            var t = PeekFromDict(_giverTasksByNpc, kv.Key);
            if (t != null) return t;
        }
        return null;
    }
}
