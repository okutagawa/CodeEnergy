using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveService
{
    public static string SaveFolder => Path.Combine(Application.persistentDataPath, "GameData");

    public static void EnsureSaveFolder()
    {
        if (!Directory.Exists(SaveFolder)) Directory.CreateDirectory(SaveFolder);
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
        File.WriteAllText(path, json);
    }

    public static void BackupFile(string fileName)
    {
        var path = GetPath(fileName);
        if (!File.Exists(path)) return;
        var bakName = $"{fileName}.{DateTime.UtcNow:yyyyMMddHHmmss}.bak";
        var bakPath = Path.Combine(SaveFolder, bakName);
        File.Copy(path, bakPath, true);
        // keep only last 5 backups for this file
        var prefix = fileName + ".";
        var bakFiles = Directory.GetFiles(SaveFolder, prefix + "*.bak")
                                .OrderByDescending(f => f)
                                .Skip(5);
        foreach (var old in bakFiles) try { File.Delete(old); } catch { }
    }

    // Import: validate via provided validator, then write (with backup)
    public static bool ImportFile(string sourcePath, string targetFileName, Func<string, (bool ok, string error)> validator, out string error)
    {
        error = null;
        try
        {
            if (!File.Exists(sourcePath)) { error = "Файл не найден"; return false; }
            var json = File.ReadAllText(sourcePath);

            if (validator != null)
            {
                var (ok, vErr) = validator(json);
                if (!ok) { error = "Валидация не пройдена: " + vErr; return false; }
            }

            SaveFile(targetFileName, json);
            Log($"Imported {Path.GetFileName(sourcePath)} -> {targetFileName}");
            return true;
        }
        catch (Exception ex) { error = ex.Message; return false; }
    }

    public static bool ExportFile(string targetFileName, string destPath, out string error)
    {
        error = null;
        try
        {
            var src = GetPath(targetFileName);
            if (!File.Exists(src)) { error = "Исходный файл не найден"; return false; }
            File.Copy(src, destPath, true);
            Log($"Exported {targetFileName} -> {destPath}");
            return true;
        }
        catch (Exception ex) { error = ex.Message; return false; }
    }

    public static string[] ListBackups(string fileName)
    {
        EnsureSaveFolder();
        var prefix = Path.Combine(SaveFolder, fileName + ".");
        return Directory.GetFiles(SaveFolder, fileName + ".*.bak").OrderByDescending(f => f).ToArray();
    }

    public static bool RestoreBackup(string backupPath, string targetFileName, out string error)
    {
        error = null;
        try
        {
            if (!File.Exists(backupPath)) { error = "Бэкап не найден"; return false; }
            var dest = GetPath(targetFileName);
            BackupFile(targetFileName); // backup current before restore
            File.Copy(backupPath, dest, true);
            Log($"Restored {backupPath} -> {targetFileName}");
            return true;
        }
        catch (Exception ex) { error = ex.Message; return false; }
    }

    private static void Log(string message)
    {
        try
        {
            EnsureSaveFolder();
            var logPath = Path.Combine(SaveFolder, "import_log.txt");
            File.AppendAllText(logPath, $"{DateTime.UtcNow:O} - {message}\n");
        }
        catch { }
    }
}
