// Assets/Scripts/Systems/DataManager.cs
using System.IO;
using System.Text;
using UnityEngine;

public static class DataManager
{
    static DataManager()
    {
        if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
    }

    public static string DataPath => Path.Combine(Application.persistentDataPath, "EduData");
    public static string CoursesFile => Path.Combine(DataPath, "courses.json");
    public static string ProfilesFile => Path.Combine(DataPath, "profiles.json");

    public static CoursesContainer LoadCourses()
    {
        if (!File.Exists(CoursesFile))
        {
            var empty = new CoursesContainer();
            SaveCourses(empty);
            return empty;
        }
        var json = File.ReadAllText(CoursesFile, Encoding.UTF8);
        return JsonUtility.FromJson<CoursesContainer>(json) ?? new CoursesContainer();
    }

    public static void SaveCourses(CoursesContainer data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(CoursesFile, json, Encoding.UTF8);
    }

    public static ProfilesContainer LoadProfiles()
    {
        if (!File.Exists(ProfilesFile))
        {
            var empty = new ProfilesContainer();
            SaveProfiles(empty);
            return empty;
        }
        var json = File.ReadAllText(ProfilesFile, Encoding.UTF8);
        return JsonUtility.FromJson<ProfilesContainer>(json) ?? new ProfilesContainer();
    }

    public static void SaveProfiles(ProfilesContainer data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(ProfilesFile, json, Encoding.UTF8);
    }

    public static int NextCourseId(CoursesContainer cc)
    {
        int max = 0;
        foreach (var c in cc.courses) if (c.id > max) max = c.id;
        return max + 1;
    }

    public static int NextLessonId(Course course)
    {
        int max = 0;
        foreach (var l in course.lessons) if (l.id > max) max = l.id;
        return max + 1;
    }
}
