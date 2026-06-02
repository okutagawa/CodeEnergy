using System;

public static class WorldEventKey
{
    public const string None = "None";
    public const string FixLanterns = "FixLanterns";
    public const string UnlockDoors = "UnlockDoors";
    public const string StartGenerator = "StartGenerator";
    public const string ActivatePortal = "ActivatePortal";
    public const string CompleteIsland = "CompleteIsland";

    public static readonly string[] All =
    {
        None,
        FixLanterns,
        UnlockDoors,
        StartGenerator,
        ActivatePortal,
        CompleteIsland
    };

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return None;

        var trimmed = value.Trim();
        var index = Array.IndexOf(All, trimmed);
        return index >= 0 ? All[index] : None;
    }

    public static int IndexOf(string value)
    {
        return Array.IndexOf(All, Normalize(value));
    }

    public static string FromIndex(int index)
    {
        return index >= 0 && index < All.Length ? All[index] : None;
    }
}