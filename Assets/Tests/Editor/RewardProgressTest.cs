using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class RewardProgressTests
{
    private string _testFolder;
    private GameObject _gameStateObject;
    private GameState _gameState;

    [SetUp]
    public void SetUp()
    {
        _testFolder = Path.Combine(Application.temporaryCachePath, "CodeEnergy_RewardProgressTests");

        if (Directory.Exists(_testFolder))
        {
            Directory.Delete(_testFolder, true);
        }

        Directory.CreateDirectory(_testFolder);

        SaveService.TestSaveFolderOverride = _testFolder;
        SaveService.EnsureWorkingFiles();

        ResetGameStateInstance();

        _gameStateObject = new GameObject("GameState_RewardProgress_TestObject");
        _gameState = _gameStateObject.AddComponent<GameState>();

        SetGameStateInstance(_gameState);
        _gameState.ApplyData(new GameStateData());
    }

    [TearDown]
    public void TearDown()
    {
        if (_gameStateObject != null)
        {
            Object.DestroyImmediate(_gameStateObject);
        }

        ResetGameStateInstance();

        SaveService.TestSaveFolderOverride = null;

        if (Directory.Exists(_testFolder))
        {
            Directory.Delete(_testFolder, true);
        }
    }

    [Test]
    public void MarkTaskStarted_WhenTaskIsNew_ShouldAddTaskToStartedList()
    {
        _gameState.MarkTaskStarted(1);

        var data = _gameState.GetData();

        Assert.Contains(1, data.startedTaskIds);
    }

    [Test]
    public void MarkTaskStarted_WhenTaskAlreadyStarted_ShouldNotDuplicateTask()
    {
        _gameState.MarkTaskStarted(1);
        _gameState.MarkTaskStarted(1);

        var data = _gameState.GetData();

        Assert.AreEqual(1, data.startedTaskIds.Count);
    }

    [Test]
    public void MarkTaskCompleted_WhenTaskCompleted_ShouldAddTaskToCompletedList()
    {
        _gameState.MarkTaskCompleted(2);

        var data = _gameState.GetData();

        Assert.Contains(2, data.completedTaskIds);
    }

    [Test]
    public void MarkTaskCompleted_WhenTaskWasStarted_ShouldRemoveTaskFromStartedList()
    {
        _gameState.MarkTaskStarted(3);
        _gameState.MarkTaskCompleted(3);

        var data = _gameState.GetData();

        Assert.Contains(3, data.completedTaskIds);
        Assert.IsFalse(data.startedTaskIds.Contains(3));
    }

    [Test]
    public void RegisterFailedQuizAttempt_WhenCalledFirstTime_ShouldCreateQuizProgressEntry()
    {
        _gameState.RegisterFailedQuizAttempt(4);

        var data = _gameState.GetData();

        Assert.AreEqual(1, data.quizProgress.Count);
        Assert.AreEqual(4, data.quizProgress[0].taskId);
        Assert.AreEqual(1, data.quizProgress[0].failedAttempts);
    }

    [Test]
    public void RegisterFailedQuizAttempt_WhenCalledTwice_ShouldIncreaseFailedAttempts()
    {
        _gameState.RegisterFailedQuizAttempt(5);
        _gameState.RegisterFailedQuizAttempt(5);

        var attempts = _gameState.GetFailedQuizAttempts(5);

        Assert.AreEqual(2, attempts);
    }

    [Test]
    public void ClearQuizProgress_WhenProgressExists_ShouldRemoveProgressEntry()
    {
        _gameState.RegisterFailedQuizAttempt(6);

        _gameState.ClearQuizProgress(6);

        var attempts = _gameState.GetFailedQuizAttempts(6);

        Assert.AreEqual(0, attempts);
    }

    [Test]
    public void TryAwardTaskStars_WhenTaskIsNotRewarded_ShouldReturnTrue()
    {
        var result = _gameState.TryAwardTaskStars(7, 3, 0);

        Assert.IsTrue(result);
    }

    [Test]
    public void TryAwardTaskStars_WhenTaskIsNotRewarded_ShouldAddRewardEntry()
    {
        _gameState.TryAwardTaskStars(8, 2, 1);

        var data = _gameState.GetData();

        Assert.AreEqual(1, data.taskRewards.Count);
        Assert.AreEqual(8, data.taskRewards[0].taskId);
        Assert.AreEqual(2, data.taskRewards[0].starsAwarded);
        Assert.AreEqual(1, data.taskRewards[0].failedAttemptsBeforeSuccess);
    }

    [Test]
    public void TryAwardTaskStars_WhenTaskIsNotRewarded_ShouldIncreaseTotalStars()
    {
        _gameState.TryAwardTaskStars(9, 2, 0);

        Assert.AreEqual(2, _gameState.GetTotalStars());
    }

    [Test]
    public void TryAwardTaskStars_WhenStarsMoreThanThree_ShouldClampStarsToThree()
    {
        _gameState.TryAwardTaskStars(10, 10, 0);

        var data = _gameState.GetData();

        Assert.AreEqual(3, data.taskRewards[0].starsAwarded);
        Assert.AreEqual(3, data.totalStars);
    }

    [Test]
    public void TryAwardTaskStars_WhenStarsLessThanZero_ShouldClampStarsToZero()
    {
        _gameState.TryAwardTaskStars(11, -5, 0);

        var data = _gameState.GetData();

        Assert.AreEqual(0, data.taskRewards[0].starsAwarded);
        Assert.AreEqual(0, data.totalStars);
    }

    [Test]
    public void TryAwardTaskStars_WhenTaskAlreadyRewarded_ShouldReturnFalse()
    {
        _gameState.TryAwardTaskStars(12, 3, 0);

        var result = _gameState.TryAwardTaskStars(12, 3, 0);

        Assert.IsFalse(result);
    }

    [Test]
    public void TryAwardTaskStars_WhenTaskAlreadyRewarded_ShouldNotAddDuplicateReward()
    {
        _gameState.TryAwardTaskStars(13, 3, 0);
        _gameState.TryAwardTaskStars(13, 3, 0);

        var data = _gameState.GetData();

        Assert.AreEqual(1, data.taskRewards.Count);
        Assert.AreEqual(3, data.totalStars);
    }

    [Test]
    public void TryAwardTaskStars_WhenQuizProgressExists_ShouldRemoveQuizProgress()
    {
        _gameState.RegisterFailedQuizAttempt(14);
        _gameState.RegisterFailedQuizAttempt(14);

        _gameState.TryAwardTaskStars(14, 2, 2);

        var attempts = _gameState.GetFailedQuizAttempts(14);

        Assert.AreEqual(0, attempts);
    }

    [Test]
    public void TrySpendStars_WhenEnoughStars_ShouldSubtractCost()
    {
        _gameState.TryAwardTaskStars(18, 3, 0);

        var result = _gameState.TrySpendStars(2);

        Assert.IsTrue(result);
        Assert.AreEqual(1, _gameState.GetTotalStars());
    }

    [Test]
    public void TrySpendStars_WhenNotEnoughStars_ShouldReturnFalseAndKeepBalance()
    {
        _gameState.TryAwardTaskStars(19, 1, 0);

        var result = _gameState.TrySpendStars(2);

        Assert.IsFalse(result);
        Assert.AreEqual(1, _gameState.GetTotalStars());
    }

    [Test]
    public void IsTaskRewarded_WhenRewardExists_ShouldReturnTrue()
    {
        _gameState.TryAwardTaskStars(15, 1, 0);

        var result = _gameState.IsTaskRewarded(15);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsTaskRewarded_WhenRewardDoesNotExist_ShouldReturnFalse()
    {
        var result = _gameState.IsTaskRewarded(16);

        Assert.IsFalse(result);
    }

    [Test]
    public void TryAwardTaskStars_WhenRewardAdded_ShouldSaveGameStateFile()
    {
        _gameState.TryAwardTaskStars(17, 2, 0);

        var path = SaveService.GetPath(SaveService.GameStateFileName);

        Assert.IsTrue(File.Exists(path));
        Assert.Greater(new FileInfo(path).Length, 0);
    }

    private static void SetGameStateInstance(GameState gameState)
    {
        var backingField = typeof(GameState).GetField(
            "<Instance>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        Assert.IsNotNull(backingField, "Поле GameState.Instance не найдено.");

        backingField.SetValue(null, gameState);
    }

    private static void ResetGameStateInstance()
    {
        var backingField = typeof(GameState).GetField(
            "<Instance>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        if (backingField != null)
        {
            backingField.SetValue(null, null);
        }
    }
}