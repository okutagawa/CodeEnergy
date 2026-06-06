using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameStateData
{
    public int saveVersion = 3;
    public string lastSavedIso;

    public int selectedCourseId = -1;

    public List<int> completedTaskIds = new List<int>();
    public List<string> completedWorldEvents = new List<string>();
    public List<int> startedTaskIds = new List<int>();

    [Serializable]
    public class NpcQueueEntry
    {
        public string npcGuid;
        public List<int> taskIds = new List<int>();
    }

    public List<NpcQueueEntry> giverQueues = new List<NpcQueueEntry>();
    public List<NpcQueueEntry> receiverQueues = new List<NpcQueueEntry>();

    public int totalStars = 0;

    [Serializable]
    public class TaskRewardEntry
    {
        public int taskId;
        public int starsAwarded;
        public int failedAttemptsBeforeSuccess;
        public long rewardedAtUtcTicks;
    }

    [Serializable]
    public class QuizProgressEntry
    {
        public int taskId;
        public int failedAttempts;
    }

    public List<TaskRewardEntry> taskRewards = new List<TaskRewardEntry>();
    public List<QuizProgressEntry> quizProgress = new List<QuizProgressEntry>();

    public SerializableVector3 playerPosition = new SerializableVector3(0, 0, 0);

    public void Normalize()
    {
        if (saveVersion <= 0) saveVersion = 3;
        if (completedTaskIds == null) completedTaskIds = new List<int>();
        if (completedWorldEvents == null) completedWorldEvents = new List<string>();
        if (startedTaskIds == null) startedTaskIds = new List<int>();
        if (giverQueues == null) giverQueues = new List<NpcQueueEntry>();
        if (receiverQueues == null) receiverQueues = new List<NpcQueueEntry>();
        if (taskRewards == null) taskRewards = new List<TaskRewardEntry>();
        if (quizProgress == null) quizProgress = new List<QuizProgressEntry>();

        completedTaskIds.RemoveAll(id => id < 0);
        completedWorldEvents.RemoveAll(string.IsNullOrWhiteSpace);
        startedTaskIds.RemoveAll(id => id < 0);
        giverQueues.RemoveAll(entry => entry == null || string.IsNullOrWhiteSpace(entry.npcGuid));
        receiverQueues.RemoveAll(entry => entry == null || string.IsNullOrWhiteSpace(entry.npcGuid));
        taskRewards.RemoveAll(entry => entry == null || entry.taskId < 0);
        quizProgress.RemoveAll(entry => entry == null || entry.taskId < 0);

        foreach (var entry in giverQueues)
        {
            if (entry.taskIds == null) entry.taskIds = new List<int>();
            entry.taskIds.RemoveAll(id => id < 0);
        }

        foreach (var entry in receiverQueues)
        {
            if (entry.taskIds == null) entry.taskIds = new List<int>();
            entry.taskIds.RemoveAll(id => id < 0);
        }

        totalStars = Mathf.Max(0, totalStars);
    }

    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }
}