using NUnit.Framework;
using UnityEngine;

public class GameStateDataTests
{
    [Test]
    public void GameStateData_WhenCreated_ShouldHaveCorrectDefaultValues()
    {
        var data = new GameStateData();

        Assert.AreEqual(2, data.saveVersion);
        Assert.AreEqual(0, data.totalStars);
        Assert.IsNull(data.lastSavedIso);

        Assert.IsNotNull(data.completedTaskIds);
        Assert.IsNotNull(data.startedTaskIds);
        Assert.IsNotNull(data.giverQueues);
        Assert.IsNotNull(data.receiverQueues);
        Assert.IsNotNull(data.taskRewards);
        Assert.IsNotNull(data.quizProgress);

        Assert.AreEqual(0, data.completedTaskIds.Count);
        Assert.AreEqual(0, data.startedTaskIds.Count);
        Assert.AreEqual(0, data.giverQueues.Count);
        Assert.AreEqual(0, data.receiverQueues.Count);
        Assert.AreEqual(0, data.taskRewards.Count);
        Assert.AreEqual(0, data.quizProgress.Count);

        Assert.AreEqual(0f, data.playerPosition.x);
        Assert.AreEqual(0f, data.playerPosition.y);
        Assert.AreEqual(0f, data.playerPosition.z);
    }

    [Test]
    public void CompletedTaskIds_WhenTaskIsAdded_ShouldContainTaskId()
    {
        var data = new GameStateData();

        data.completedTaskIds.Add(1);
        data.completedTaskIds.Add(2);

        Assert.Contains(1, data.completedTaskIds);
        Assert.Contains(2, data.completedTaskIds);
        Assert.AreEqual(2, data.completedTaskIds.Count);
    }

    [Test]
    public void StartedTaskIds_WhenTaskIsAdded_ShouldContainTaskId()
    {
        var data = new GameStateData();

        data.startedTaskIds.Add(5);

        Assert.Contains(5, data.startedTaskIds);
        Assert.AreEqual(1, data.startedTaskIds.Count);
    }

    [Test]
    public void TotalStars_WhenChanged_ShouldStoreNewValue()
    {
        var data = new GameStateData();

        data.totalStars = 10;

        Assert.AreEqual(10, data.totalStars);
    }

    [Test]
    public void PlayerPosition_WhenChanged_ShouldStoreCoordinates()
    {
        var data = new GameStateData();

        data.playerPosition = new GameStateData.SerializableVector3(1.5f, 2.5f, 3.5f);

        Assert.AreEqual(1.5f, data.playerPosition.x);
        Assert.AreEqual(2.5f, data.playerPosition.y);
        Assert.AreEqual(3.5f, data.playerPosition.z);
    }

    [Test]
    public void TaskRewardEntry_WhenCreated_ShouldStoreRewardData()
    {
        var reward = new GameStateData.TaskRewardEntry
        {
            taskId = 3,
            starsAwarded = 2,
            failedAttemptsBeforeSuccess = 1,
            rewardedAtUtcTicks = 123456789
        };

        Assert.AreEqual(3, reward.taskId);
        Assert.AreEqual(2, reward.starsAwarded);
        Assert.AreEqual(1, reward.failedAttemptsBeforeSuccess);
        Assert.AreEqual(123456789, reward.rewardedAtUtcTicks);
    }

    [Test]
    public void TaskRewards_WhenRewardIsAdded_ShouldContainRewardEntry()
    {
        var data = new GameStateData();

        var reward = new GameStateData.TaskRewardEntry
        {
            taskId = 7,
            starsAwarded = 3,
            failedAttemptsBeforeSuccess = 0,
            rewardedAtUtcTicks = 100
        };

        data.taskRewards.Add(reward);

        Assert.AreEqual(1, data.taskRewards.Count);
        Assert.AreEqual(7, data.taskRewards[0].taskId);
        Assert.AreEqual(3, data.taskRewards[0].starsAwarded);
        Assert.AreEqual(0, data.taskRewards[0].failedAttemptsBeforeSuccess);
    }

    [Test]
    public void QuizProgressEntry_WhenCreated_ShouldStoreFailedAttempts()
    {
        var progress = new GameStateData.QuizProgressEntry
        {
            taskId = 4,
            failedAttempts = 2
        };

        Assert.AreEqual(4, progress.taskId);
        Assert.AreEqual(2, progress.failedAttempts);
    }

    [Test]
    public void QuizProgress_WhenProgressIsAdded_ShouldContainProgressEntry()
    {
        var data = new GameStateData();

        var progress = new GameStateData.QuizProgressEntry
        {
            taskId = 8,
            failedAttempts = 1
        };

        data.quizProgress.Add(progress);

        Assert.AreEqual(1, data.quizProgress.Count);
        Assert.AreEqual(8, data.quizProgress[0].taskId);
        Assert.AreEqual(1, data.quizProgress[0].failedAttempts);
    }

    [Test]
    public void NpcQueueEntry_WhenCreated_ShouldStoreNpcGuidAndTaskIds()
    {
        var queueEntry = new GameStateData.NpcQueueEntry
        {
            npcGuid = "drone"
        };

        queueEntry.taskIds.Add(1);
        queueEntry.taskIds.Add(2);

        Assert.AreEqual("drone", queueEntry.npcGuid);
        Assert.Contains(1, queueEntry.taskIds);
        Assert.Contains(2, queueEntry.taskIds);
        Assert.AreEqual(2, queueEntry.taskIds.Count);
    }

    [Test]
    public void GiverQueues_WhenQueueIsAdded_ShouldContainNpcQueue()
    {
        var data = new GameStateData();

        var queueEntry = new GameStateData.NpcQueueEntry
        {
            npcGuid = "drone"
        };

        queueEntry.taskIds.Add(10);

        data.giverQueues.Add(queueEntry);

        Assert.AreEqual(1, data.giverQueues.Count);
        Assert.AreEqual("drone", data.giverQueues[0].npcGuid);
        Assert.Contains(10, data.giverQueues[0].taskIds);
    }

    [Test]
    public void ReceiverQueues_WhenQueueIsAdded_ShouldContainNpcQueue()
    {
        var data = new GameStateData();

        var queueEntry = new GameStateData.NpcQueueEntry
        {
            npcGuid = "terminal"
        };

        queueEntry.taskIds.Add(15);

        data.receiverQueues.Add(queueEntry);

        Assert.AreEqual(1, data.receiverQueues.Count);
        Assert.AreEqual("terminal", data.receiverQueues[0].npcGuid);
        Assert.Contains(15, data.receiverQueues[0].taskIds);
    }

    [Test]
    public void GameStateData_WhenSerializedToJson_ShouldContainProgressData()
    {
        var data = new GameStateData
        {
            totalStars = 6,
            playerPosition = new GameStateData.SerializableVector3(1f, 2f, 3f)
        };

        data.completedTaskIds.Add(1);
        data.startedTaskIds.Add(2);

        var json = JsonUtility.ToJson(data, true);

        Assert.IsNotNull(json);
        Assert.IsTrue(json.Contains("\"totalStars\": 6"));
        Assert.IsTrue(json.Contains("\"completedTaskIds\""));
        Assert.IsTrue(json.Contains("\"startedTaskIds\""));
        Assert.IsTrue(json.Contains("\"playerPosition\""));
    }

    [Test]
    public void GameStateData_WhenDeserializedFromJson_ShouldRestoreProgressData()
    {
        var data = new GameStateData
        {
            totalStars = 12,
            playerPosition = new GameStateData.SerializableVector3(4f, 5f, 6f)
        };

        data.completedTaskIds.Add(11);
        data.startedTaskIds.Add(22);

        var json = JsonUtility.ToJson(data, true);
        var restored = JsonUtility.FromJson<GameStateData>(json);

        Assert.IsNotNull(restored);
        Assert.AreEqual(12, restored.totalStars);
        Assert.Contains(11, restored.completedTaskIds);
        Assert.Contains(22, restored.startedTaskIds);
        Assert.AreEqual(4f, restored.playerPosition.x);
        Assert.AreEqual(5f, restored.playerPosition.y);
        Assert.AreEqual(6f, restored.playerPosition.z);
    }

    [Test]
    public void GameStateData_WhenRewardDataSerializedAndDeserialized_ShouldRestoreReward()
    {
        var data = new GameStateData();

        data.taskRewards.Add(new GameStateData.TaskRewardEntry
        {
            taskId = 20,
            starsAwarded = 3,
            failedAttemptsBeforeSuccess = 2,
            rewardedAtUtcTicks = 500
        });

        var json = JsonUtility.ToJson(data, true);
        var restored = JsonUtility.FromJson<GameStateData>(json);

        Assert.IsNotNull(restored);
        Assert.AreEqual(1, restored.taskRewards.Count);
        Assert.AreEqual(20, restored.taskRewards[0].taskId);
        Assert.AreEqual(3, restored.taskRewards[0].starsAwarded);
        Assert.AreEqual(2, restored.taskRewards[0].failedAttemptsBeforeSuccess);
        Assert.AreEqual(500, restored.taskRewards[0].rewardedAtUtcTicks);
    }
}