using System;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public static event Action<int> OnTotalStarsChanged;

    public bool IsAdminMode = false;

    private GameStateData _data;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Debug.Log($"persistentDataPath = {Application.persistentDataPath}");
        LoadState();
    }

    public static void EnsureExists()
    {
        if (Instance != null) return;
        var go = new GameObject("GameState");
        go.AddComponent<GameState>();
    }

    public void LoadState()
    {
        _data = SaveManager.Load() ?? new GameStateData();
        Debug.Log("[GameState] State loaded");
        NotifyTotalStarsChanged();
    }

    public void SaveState()
    {
        if (_data == null) _data = new GameStateData();
        SaveManager.Save(_data);
    }

    public GameStateData GetData()
    {
        if (_data == null) _data = new GameStateData();
        return _data;
    }

    public void ApplyData(GameStateData data)
    {
        _data = data ?? new GameStateData();
        NotifyTotalStarsChanged();
    }

    // ----- task helpers -----
    public void MarkTaskCompleted(int taskId)
    {
        var d = GetData();
        if (!d.completedTaskIds.Contains(taskId)) d.completedTaskIds.Add(taskId);
        d.startedTaskIds.Remove(taskId);
        SaveState();
    }

    public void MarkTaskStarted(int taskId)
    {
        var d = GetData();
        if (!d.startedTaskIds.Contains(taskId)) d.startedTaskIds.Add(taskId);
        SaveState();
    }

    public void SetGiverQueue(string npcGuid, System.Collections.Generic.List<int> taskIds)
    {
        var d = GetData();
        var entry = d.giverQueues.Find(e => e.npcGuid == npcGuid);
        if (entry == null) { entry = new GameStateData.NpcQueueEntry { npcGuid = npcGuid, taskIds = new System.Collections.Generic.List<int>(taskIds) }; d.giverQueues.Add(entry); }
        else { entry.taskIds = new System.Collections.Generic.List<int>(taskIds); }
        SaveState();
    }

    public void SetReceiverQueue(string npcGuid, System.Collections.Generic.List<int> taskIds)
    {
        var d = GetData();
        var entry = d.receiverQueues.Find(e => e.npcGuid == npcGuid);
        if (entry == null) { entry = new GameStateData.NpcQueueEntry { npcGuid = npcGuid, taskIds = new System.Collections.Generic.List<int>(taskIds) }; d.receiverQueues.Add(entry); }
        else { entry.taskIds = new System.Collections.Generic.List<int>(taskIds); }
        SaveState();
    }

    // ----- reward and stars helpers -----
    public int GetTotalStars()
    {
        return Mathf.Max(0, GetData().totalStars);
    }

    public int GetFailedQuizAttempts(int taskId)
    {
        var entry = GetData().quizProgress.Find(e => e.taskId == taskId);
        return entry != null ? Mathf.Max(0, entry.failedAttempts) : 0;
    }

    public void RegisterFailedQuizAttempt(int taskId)
    {
        var d = GetData();
        var entry = d.quizProgress.Find(e => e.taskId == taskId);
        if (entry == null)
        {
            entry = new GameStateData.QuizProgressEntry { taskId = taskId, failedAttempts = 1 };
            d.quizProgress.Add(entry);
        }
        else
        {
            entry.failedAttempts = Mathf.Max(0, entry.failedAttempts) + 1;
        }

        SaveState();
    }

    public void ClearQuizProgress(int taskId)
    {
        var d = GetData();
        d.quizProgress.RemoveAll(e => e.taskId == taskId);
        SaveState();
    }

    public bool IsTaskRewarded(int taskId)
    {
        return GetData().taskRewards.Exists(e => e.taskId == taskId);
    }

    public bool TryAwardTaskStars(int taskId, int stars, int failedAttemptsBeforeSuccess)
    {
        var d = GetData();
        if (d.taskRewards.Exists(e => e.taskId == taskId))
            return false;

        int safeStars = Mathf.Clamp(stars, 0, 3);

        d.taskRewards.Add(new GameStateData.TaskRewardEntry
        {
            taskId = taskId,
            starsAwarded = safeStars,
            failedAttemptsBeforeSuccess = Mathf.Max(0, failedAttemptsBeforeSuccess),
            rewardedAtUtcTicks = DateTime.UtcNow.Ticks
        });

        d.totalStars = Mathf.Max(0, d.totalStars) + safeStars;
        d.quizProgress.RemoveAll(e => e.taskId == taskId);

        SaveState();
        NotifyTotalStarsChanged();
        return true;
    }

    private void NotifyTotalStarsChanged()
    {
        try
        {
            OnTotalStarsChanged?.Invoke(GetTotalStars());
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameState] Exception in OnTotalStarsChanged: {ex}");
        }
    }


    // ----- player position helpers -----
    public void SetPlayerPosition(Vector3 pos)
    {
        var d = GetData();
        d.playerPosition = new GameStateData.SerializableVector3(pos.x, pos.y, pos.z);
        // Не сохраняем автоматически здесь, чтобы дать контролю вызывающему коду:
        // SaveState(); // можно раскомментировать, если нужно немедленно записывать
    }

    public Vector3 GetPlayerPositionVector3()
    {
        var d = GetData();
        var p = d.playerPosition;
        return new Vector3(p.x, p.y, p.z);
    }

    // ----- lifecycle hooks -----
    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // Если есть игрок в сцене, лучше чтобы он сам вызвал SetPlayerPosition перед паузой.
            SaveState();
        }
    }

    void OnApplicationQuit()
    {
        // Рекомендуется, чтобы игрок заранее вызвал SetPlayerPosition(transform.position).
        SaveState();
    }
}
