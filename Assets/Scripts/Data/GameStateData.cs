using System;
using System.Collections.Generic;

[Serializable]
public class GameStateData
{
    public int saveVersion = 2;
    public string lastSavedIso;

    public List<int> completedTaskIds = new List<int>();
    public List<int> startedTaskIds = new List<int>();

    [Serializable]
    public class NpcQueueEntry { public string npcGuid; public List<int> taskIds = new List<int>(); }

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

    [Serializable]
    public struct SerializableVector3 { public float x, y, z; public SerializableVector3(float X, float Y, float Z) { x = X; y = Y; z = Z; } }
}
