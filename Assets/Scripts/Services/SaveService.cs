using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MyGame.Models;
using UnityEngine;

public static class SaveService
{
    public const string CoursesFileName = "courses.json";
    public const string TasksFileName = "tasks.json";
    public const string GameStateFileName = "gamestate.json";

    public static string SaveFolder => Path.Combine(Application.persistentDataPath, "GameData");
    public static string BackupsFolder => Path.Combine(SaveFolder, "Backups");
    public static string TransferFolder => Path.Combine(SaveFolder, "Transfer");

    [Serializable]
    private class TaskListWrapper
    {
        public List<TaskModel> tasks = new List<TaskModel>();
    }

    public static void EnsureSaveFolder()
    {
        if (!Directory.Exists(SaveFolder)) Directory.CreateDirectory(SaveFolder);
        if (!Directory.Exists(BackupsFolder)) Directory.CreateDirectory(BackupsFolder);
        if (!Directory.Exists(TransferFolder)) Directory.CreateDirectory(TransferFolder);
    }

    public static void EnsureWorkingFiles()
    {
        EnsureSaveFolder();
        EnsureFileExists(CoursesFileName, JsonUtility.ToJson(new CoursesContainer(), true));
        EnsureFileExists(TasksFileName, JsonUtility.ToJson(new TaskListWrapper(), true));
        EnsureFileExists(GameStateFileName, JsonUtility.ToJson(new GameStateData(), true));
    }

    public static string GetPath(string fileName)
    {
        EnsureSaveFolder();
        return Path.Combine(SaveFolder, fileName);
    }

    public static bool FileExists(string fileName) => File.Exists(GetPath(fileName));

    public static string LoadFile(string fileName)
    {
        var path = GetPath(fileName);
        if (!File.Exists(path)) return null;
        return File.ReadAllText(path);
    }

    public static void SaveFile(string fileName, string json)
    {
        var path = GetPath(fileName);
        BackupFile(fileName);
        File.WriteAllText(path, json ?? string.Empty);
    }

    public static void BackupFile(string fileName)
    {
        var path = GetPath(fileName);
        if (!File.Exists(path)) return;
        var bakName = $"{fileName}.{DateTime.UtcNow:yyyyMMddHHmmss}.bak";
        var bakPath = Path.Combine(SaveFolder, bakName);
        File.Copy(path, bakPath, true);

        var bakFiles = Directory.GetFiles(SaveFolder, fileName + ".*.bak")
            .OrderByDescending(f => f)
            .Skip(10);

        foreach (var old in bakFiles)
        {
            try { File.Delete(old); } catch { }
        }
    }

    public static string CreateBackupBundle()
    {
        EnsureWorkingFiles();
        var bundleFolder = Path.Combine(BackupsFolder, DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(bundleFolder);

        CopyIfExists(CoursesFileName, bundleFolder);
        CopyIfExists(TasksFileName, bundleFolder);
        CopyIfExists(GameStateFileName, bundleFolder);

        Log($"Backup bundle created: {bundleFolder}");
        return bundleFolder;
    }

    public static bool RestoreLatestBackupBundle(out string restoredFrom, out string error)
    {
        restoredFrom = null;
        error = null;

        try
        {
            EnsureSaveFolder();
            var latest = Directory.GetDirectories(BackupsFolder)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(latest))
            {
                error = "Backup folder is empty.";
                return false;
            }

            if (!RestoreFromBundleFile(latest, CoursesFileName) ||
                !RestoreFromBundleFile(latest, TasksFileName) ||
                !RestoreFromBundleFile(latest, GameStateFileName))
            {
                error = "One or more files are missing in the backup bundle.";
                return false;
            }

            restoredFrom = latest;
            Log($"Backup bundle restored: {latest}");
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    // Import: validate via provided validator, then write (with backup)
    public static bool ImportFile(string sourcePath, string targetFileName, Func<string, (bool ok, string error)> validator, out string error)
    {
        error = null;
        try
        {
            if (!File.Exists(sourcePath)) { error = "Source file not found."; return false; }
            var json = File.ReadAllText(sourcePath);

            if (validator != null)
            {
                var validationResult = validator(json);
                if (!validationResult.ok)
                {
                    error = "Invalid JSON: " + validationResult.error;
                    return false;
                }
            }

            SaveFile(targetFileName, json);
            Log($"Imported {Path.GetFileName(sourcePath)} -> {targetFileName}");
            return true;
        }
        catch (Exception ex) { error = ex.Message; return false; }
    }

    public static bool ExportFile(string sourceFileName, string destinationPath, out string error)
    {
        error = null;
        try
        {
            var sourcePath = GetPath(sourceFileName);
            if (!File.Exists(sourcePath)) { error = "Source file does not exist."; return false; }
            var dir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.Copy(sourcePath, destinationPath, true);
            Log($"Exported {sourceFileName} -> {destinationPath}");
            return true;
        }
        catch (Exception ex) { error = ex.Message; return false; }
    }

    public static (bool ok, string error) ValidateCoursesJson(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return (false, "File is empty.");
            var model = JsonUtility.FromJson<CoursesContainer>(json);
            return model != null && model.courses != null
                ? (true, null)
                : (false, "Expected object with 'courses' array.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public static (bool ok, string error) ValidateTasksJson(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return (false, "File is empty.");
            var model = JsonUtility.FromJson<TaskListWrapper>(json);
            return model != null && model.tasks != null
                ? (true, null)
                : (false, "Expected object with 'tasks' array.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public static (bool ok, string error) ValidateGameStateJson(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return (false, "File is empty.");
            var model = JsonUtility.FromJson<GameStateData>(json);
            return model != null
                ? (true, null)
                : (false, "Could not parse GameStateData JSON.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public static string GetTransferPath(string fileName)
    {
        EnsureSaveFolder();
        return Path.Combine(TransferFolder, fileName);
    }

    public static void OpenDataFolder()
    {
        EnsureSaveFolder();

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        Process.Start("explorer.exe", SaveFolder.Replace("/", "\\"));
#else
        Application.OpenURL("file://" + SaveFolder);
#endif
    }

    private static bool RestoreFromBundleFile(string bundleFolder, string fileName)
    {
        var backupPath = Path.Combine(bundleFolder, fileName);
        if (!File.Exists(backupPath)) return false;

        BackupFile(fileName);
        File.Copy(backupPath, GetPath(fileName), true);
        return true;
    }

    private static void EnsureFileExists(string fileName, string defaultJson)
    {
        var path = GetPath(fileName);
        if (File.Exists(path)) return;

        File.WriteAllText(path, defaultJson);
        Log($"Created default file: {fileName}");
    }

    private static void CopyIfExists(string fileName, string destinationFolder)
    {
        var path = GetPath(fileName);
        if (File.Exists(path))
        {
            File.Copy(path, Path.Combine(destinationFolder, fileName), true);
        }
    }

    private static void Log(string message)
    {
        try
        {
            EnsureSaveFolder();
            var logPath = Path.Combine(SaveFolder, "save_service_log.txt");
            File.AppendAllText(logPath, $"{DateTime.UtcNow:O} - {message}\n");
        }
        catch { }
    }
}
