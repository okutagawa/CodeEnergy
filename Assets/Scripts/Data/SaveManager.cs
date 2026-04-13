using System;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string FilePath => SaveService.GetPath(SaveService.GameStateFileName);

    public static void Save(GameStateData data)
    {
        SaveService.EnsureWorkingFiles();

        if (data == null) return;
        data.lastSavedIso = DateTime.Now.ToString("O");
        try
        {
            var json = JsonUtility.ToJson(data, true);
            var tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(FilePath)) File.Delete(FilePath);
            File.Move(tmp, FilePath);
            Debug.Log($"[SaveManager] Saved gamestate ({json.Length} bytes) to {FilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Save failed: {ex}");
        }
    }

    public static GameStateData Load()
    {
        SaveService.EnsureWorkingFiles();

        try
        {
            if (!File.Exists(FilePath))
            {
                Debug.Log("[SaveManager] No save file found, returning new GameStateData");
                return new GameStateData();
            }

            var json = File.ReadAllText(FilePath);
            var data = JsonUtility.FromJson<GameStateData>(json) ?? new GameStateData();
            Debug.Log($"[SaveManager] Loaded gamestate from {FilePath}");
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Load failed: {ex}");
            return new GameStateData();
        }
    }

    public static void Delete()
    {
        try
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
            Debug.Log("[SaveManager] Save deleted");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Delete failed: {ex}");
        }
    }
}
