using System.IO;
using NUnit.Framework;
using UnityEngine;

public class JsonStructureValidationTests
{
    private string _testFolder;

    [SetUp]
    public void SetUp()
    {
        _testFolder = Path.Combine(Application.temporaryCachePath, "CodeEnergy_JsonStructureValidationTests");

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
    public void ValidateCoursesJson_WhenJsonHasCoursesArray_ShouldReturnOk()
    {
        var json = "{\"courses\":[]}";

        var result = SaveService.ValidateCoursesJson(json);

        Assert.IsTrue(result.ok);
        Assert.IsNull(result.error);
    }

    [Test]
    public void ValidateCoursesJson_WhenJsonIsEmpty_ShouldReturnError()
    {
        var result = SaveService.ValidateCoursesJson("");

        Assert.IsFalse(result.ok);
        Assert.IsNotNull(result.error);
    }

    [Test]
    public void ValidateCoursesJson_WhenJsonIsWhitespace_ShouldReturnError()
    {
        var result = SaveService.ValidateCoursesJson("   ");

        Assert.IsFalse(result.ok);
        Assert.IsNotNull(result.error);
    }

    [Test]
    public void ValidateTasksJson_WhenJsonHasTasksArray_ShouldReturnOk()
    {
        var json = "{\"tasks\":[]}";

        var result = SaveService.ValidateTasksJson(json);

        Assert.IsTrue(result.ok);
        Assert.IsNull(result.error);
    }

    [Test]
    public void ValidateTasksJson_WhenJsonIsEmpty_ShouldReturnError()
    {
        var result = SaveService.ValidateTasksJson("");

        Assert.IsFalse(result.ok);
        Assert.IsNotNull(result.error);
    }

    [Test]
    public void ValidateTasksJson_WhenJsonIsWhitespace_ShouldReturnError()
    {
        var result = SaveService.ValidateTasksJson("   ");

        Assert.IsFalse(result.ok);
        Assert.IsNotNull(result.error);
    }

    [Test]
    public void ValidateGameStateJson_WhenJsonIsValid_ShouldReturnOk()
    {
        var json = JsonUtility.ToJson(new GameStateData(), true);

        var result = SaveService.ValidateGameStateJson(json);

        Assert.IsTrue(result.ok);
        Assert.IsNull(result.error);
    }

    [Test]
    public void ValidateGameStateJson_WhenJsonIsEmpty_ShouldReturnError()
    {
        var result = SaveService.ValidateGameStateJson("");

        Assert.IsFalse(result.ok);
        Assert.IsNotNull(result.error);
    }

    [Test]
    public void ValidateGameStateJson_WhenJsonIsWhitespace_ShouldReturnError()
    {
        var result = SaveService.ValidateGameStateJson("   ");

        Assert.IsFalse(result.ok);
        Assert.IsNotNull(result.error);
    }

    [Test]
    public void EnsureWorkingFiles_WhenCalled_ShouldCreateCoursesJson()
    {
        var path = SaveService.GetPath(SaveService.CoursesFileName);

        Assert.IsTrue(File.Exists(path));
    }

    [Test]
    public void EnsureWorkingFiles_WhenCalled_ShouldCreateTasksJson()
    {
        var path = SaveService.GetPath(SaveService.TasksFileName);

        Assert.IsTrue(File.Exists(path));
    }

    [Test]
    public void EnsureWorkingFiles_WhenCalled_ShouldCreateGameStateJson()
    {
        var path = SaveService.GetPath(SaveService.GameStateFileName);

        Assert.IsTrue(File.Exists(path));
    }

    [Test]
    public void LoadFile_WhenFileExists_ShouldReturnJsonContent()
    {
        SaveService.SaveFile(SaveService.CoursesFileName, "{\"courses\":[]}");

        var json = SaveService.LoadFile(SaveService.CoursesFileName);

        Assert.IsNotNull(json);
        Assert.IsTrue(json.Contains("courses"));
    }

    [Test]
    public void LoadFile_WhenFileDoesNotExist_ShouldReturnNull()
    {
        var json = SaveService.LoadFile("missing_file.json");

        Assert.IsNull(json);
    }

    [Test]
    public void SaveFile_WhenJsonIsNull_ShouldCreateEmptyFile()
    {
        SaveService.SaveFile("empty_test.json", null);

        var path = SaveService.GetPath("empty_test.json");

        Assert.IsTrue(File.Exists(path));
        Assert.AreEqual(string.Empty, File.ReadAllText(path));
    }

    [Test]
    public void ImportFile_WhenSourceFileIsMissing_ShouldReturnFalse()
    {
        var missingSourcePath = Path.Combine(_testFolder, "missing_courses.json");

        var result = SaveService.ImportFile(
            missingSourcePath,
            SaveService.CoursesFileName,
            SaveService.ValidateCoursesJson,
            out var error
        );

        Assert.IsFalse(result);
        Assert.IsNotNull(error);
    }

    [Test]
    public void ImportFile_WhenCoursesJsonIsValid_ShouldImportFile()
    {
        var sourcePath = Path.Combine(_testFolder, "valid_courses_import.json");
        File.WriteAllText(sourcePath, "{\"courses\":[]}");

        var result = SaveService.ImportFile(
            sourcePath,
            SaveService.CoursesFileName,
            SaveService.ValidateCoursesJson,
            out var error
        );

        Assert.IsTrue(result);
        Assert.IsNull(error);

        var importedJson = SaveService.LoadFile(SaveService.CoursesFileName);
        Assert.IsTrue(importedJson.Contains("courses"));
    }

    [Test]
    public void ImportFile_WhenTasksJsonIsValid_ShouldImportFile()
    {
        var sourcePath = Path.Combine(_testFolder, "valid_tasks_import.json");
        File.WriteAllText(sourcePath, "{\"tasks\":[]}");

        var result = SaveService.ImportFile(
            sourcePath,
            SaveService.TasksFileName,
            SaveService.ValidateTasksJson,
            out var error
        );

        Assert.IsTrue(result);
        Assert.IsNull(error);

        var importedJson = SaveService.LoadFile(SaveService.TasksFileName);
        Assert.IsTrue(importedJson.Contains("tasks"));
    }

    [Test]
    public void ImportFile_WhenGameStateJsonIsValid_ShouldImportFile()
    {
        var sourcePath = Path.Combine(_testFolder, "valid_gamestate_import.json");
        var json = JsonUtility.ToJson(new GameStateData(), true);
        File.WriteAllText(sourcePath, json);

        var result = SaveService.ImportFile(
            sourcePath,
            SaveService.GameStateFileName,
            SaveService.ValidateGameStateJson,
            out var error
        );

        Assert.IsTrue(result);
        Assert.IsNull(error);

        var importedJson = SaveService.LoadFile(SaveService.GameStateFileName);
        Assert.IsTrue(importedJson.Contains("saveVersion"));
    }

    [Test]
    public void ImportFile_WhenCoursesJsonIsEmpty_ShouldReturnFalse()
    {
        var sourcePath = Path.Combine(_testFolder, "empty_courses_import.json");
        File.WriteAllText(sourcePath, "");

        var result = SaveService.ImportFile(
            sourcePath,
            SaveService.CoursesFileName,
            SaveService.ValidateCoursesJson,
            out var error
        );

        Assert.IsFalse(result);
        Assert.IsNotNull(error);
    }

    [Test]
    public void ExportFile_WhenSourceFileExists_ShouldCreateDestinationFile()
    {
        SaveService.SaveFile(SaveService.CoursesFileName, "{\"courses\":[]}");

        var destinationPath = Path.Combine(_testFolder, "exported_courses.json");

        var result = SaveService.ExportFile(
            SaveService.CoursesFileName,
            destinationPath,
            out var error
        );

        Assert.IsTrue(result);
        Assert.IsNull(error);
        Assert.IsTrue(File.Exists(destinationPath));
    }

    [Test]
    public void ExportFile_WhenSourceFileDoesNotExist_ShouldReturnFalse()
    {
        var destinationPath = Path.Combine(_testFolder, "exported_missing.json");

        var result = SaveService.ExportFile(
            "missing_source.json",
            destinationPath,
            out var error
        );

        Assert.IsFalse(result);
        Assert.IsNotNull(error);
    }
}