using System.IO;
using NUnit.Framework;
using UnityEngine;

public class SaveSystemTests
{
    private string _testFolder;

    [SetUp]
    public void SetUp()
    {
        _testFolder = Path.Combine(Application.temporaryCachePath, "CodeEnergy_SaveSystemTests");

        if (Directory.Exists(_testFolder))
        {
            Directory.Delete(_testFolder, true);
        }

        Directory.CreateDirectory(_testFolder);

        SaveService.TestSaveFolderOverride = _testFolder;
        SaveService.EnsureWorkingFiles();
    }

    [TearDown]
    public void TearDown()
    {
        SaveService.TestSaveFolderOverride = null;

        if (Directory.Exists(_testFolder))
        {
            Directory.Delete(_testFolder, true);
        }
    }

    [Test]
    public void Save_WhenGameStateIsValid_ShouldCreateGameStateFile()
    {
        var data = new GameStateData
        {
            totalStars = 5
        };

        data.completedTaskIds.Add(1);
        data.completedTaskIds.Add(2);

        SaveManager.Save(data);

        var path = SaveService.GetPath(SaveService.GameStateFileName);

        Assert.IsTrue(File.Exists(path));
        Assert.Greater(new FileInfo(path).Length, 0);
    }

    [Test]
    public void Load_WhenSaveFileExists_ShouldReturnSavedGameState()
    {
        var data = new GameStateData
        {
            totalStars = 7
        };

        data.completedTaskIds.Add(10);
        data.startedTaskIds.Add(20);

        SaveManager.Save(data);

        var loaded = SaveManager.Load();

        Assert.IsNotNull(loaded);
        Assert.AreEqual(7, loaded.totalStars);
        Assert.Contains(10, loaded.completedTaskIds);
        Assert.Contains(20, loaded.startedTaskIds);
    }

    [Test]
    public void Load_WhenSaveFileDoesNotExist_ShouldReturnNewGameStateData()
    {
        var path = SaveService.GetPath(SaveService.GameStateFileName);

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var loaded = SaveManager.Load();

        Assert.IsNotNull(loaded);
        Assert.AreEqual(0, loaded.totalStars);
        Assert.IsNotNull(loaded.completedTaskIds);
        Assert.IsNotNull(loaded.startedTaskIds);
        Assert.IsNotNull(loaded.taskRewards);
        Assert.IsNotNull(loaded.quizProgress);
    }

    [Test]
    public void Delete_WhenSaveFileExists_ShouldRemoveGameStateFile()
    {
        var data = new GameStateData
        {
            totalStars = 3
        };

        SaveManager.Save(data);

        var path = SaveService.GetPath(SaveService.GameStateFileName);

        Assert.IsTrue(File.Exists(path));

        SaveManager.Delete();

        Assert.IsFalse(File.Exists(path));
    }

    [Test]
    public void GameStateData_WhenCreated_ShouldHaveDefaultValues()
    {
        var data = new GameStateData();

        Assert.AreEqual(2, data.saveVersion);
        Assert.AreEqual(0, data.totalStars);

        Assert.IsNotNull(data.completedTaskIds);
        Assert.IsNotNull(data.startedTaskIds);
        Assert.IsNotNull(data.giverQueues);
        Assert.IsNotNull(data.receiverQueues);
        Assert.IsNotNull(data.taskRewards);
        Assert.IsNotNull(data.quizProgress);

        Assert.AreEqual(0, data.completedTaskIds.Count);
        Assert.AreEqual(0, data.startedTaskIds.Count);
        Assert.AreEqual(0, data.taskRewards.Count);
        Assert.AreEqual(0, data.quizProgress.Count);
    }

    [Test]
    public void SaveService_EnsureWorkingFiles_ShouldCreateRequiredJsonFiles()
    {
        SaveService.EnsureWorkingFiles();

        var coursesPath = SaveService.GetPath(SaveService.CoursesFileName);
        var tasksPath = SaveService.GetPath(SaveService.TasksFileName);
        var gameStatePath = SaveService.GetPath(SaveService.GameStateFileName);

        Assert.IsTrue(File.Exists(coursesPath));
        Assert.IsTrue(File.Exists(tasksPath));
        Assert.IsTrue(File.Exists(gameStatePath));
    }

    [Test]
    public void SaveService_ValidateGameStateJson_WhenJsonIsValid_ShouldReturnOk()
    {
        var json = JsonUtility.ToJson(new GameStateData(), true);

        var result = SaveService.ValidateGameStateJson(json);

        Assert.IsTrue(result.ok);
        Assert.IsNull(result.error);
    }

    [Test]
    public void SaveService_ValidateGameStateJson_WhenJsonIsEmpty_ShouldReturnError()
    {
        var result = SaveService.ValidateGameStateJson("");

        Assert.IsFalse(result.ok);
        Assert.IsNotNull(result.error);
    }

    [Test]
    public void SaveService_ValidateCoursesJson_WhenJsonIsValid_ShouldReturnOk()
    {
        var json = "{\"courses\":[]}";

        var result = SaveService.ValidateCoursesJson(json);

        Assert.IsTrue(result.ok);
        Assert.IsNull(result.error);
    }

    [Test]
    public void SaveService_ValidateTasksJson_WhenJsonIsValid_ShouldReturnOk()
    {
        var json = "{\"tasks\":[]}";

        var result = SaveService.ValidateTasksJson(json);

        Assert.IsTrue(result.ok);
        Assert.IsNull(result.error);
    }

    [Test]
    public void SaveService_BackupFile_WhenFileExists_ShouldCreateBackupFile()
    {
        var data = new GameStateData
        {
            totalStars = 4
        };

        SaveManager.Save(data);

        SaveService.BackupFile(SaveService.GameStateFileName);

        var backupFiles = Directory.GetFiles(
            SaveService.SaveFolder,
            SaveService.GameStateFileName + ".*.bak"
        );

        Assert.IsNotNull(backupFiles);
        Assert.Greater(backupFiles.Length, 0);
    }

    [Test]
    public void SaveService_CreateBackupBundle_ShouldCreateBackupFolderWithFiles()
    {
        var data = new GameStateData
        {
            totalStars = 9
        };

        SaveManager.Save(data);

        var backupFolder = SaveService.CreateBackupBundle();

        Assert.IsTrue(Directory.Exists(backupFolder));
        Assert.IsTrue(File.Exists(Path.Combine(backupFolder, SaveService.CoursesFileName)));
        Assert.IsTrue(File.Exists(Path.Combine(backupFolder, SaveService.TasksFileName)));
        Assert.IsTrue(File.Exists(Path.Combine(backupFolder, SaveService.GameStateFileName)));
    }
}