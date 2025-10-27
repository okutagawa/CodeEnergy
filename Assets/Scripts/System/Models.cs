
using System;
using System.Collections.Generic;
// описание сущностей

[Serializable]
public class Lesson
{
    public int id;
    public string title;
    public int stars; // прогресс (0..3) Ч хранитс€ в Course или в профиле
}

[Serializable]
public class Course
{
    public int id;
    public string name;
    public List<Lesson> lessons = new List<Lesson>();
}

[Serializable]
public class CoursesContainer
{
    public List<Course> courses = new List<Course>();
}

[Serializable]
public class ProgressPair
{
    public string key; // "courseId:lessonId"
    public int value;
}

[Serializable]
public class Profile
{
    public string name;
    public List<ProgressPair> progress = new List<ProgressPair>();

    public void SetProgress(int courseId, int lessonId, int stars)
    {
        string k = $"{courseId}:{lessonId}";
        var p = progress.Find(x => x.key == k);
        if (p == null) progress.Add(new ProgressPair { key = k, value = stars });
        else p.value = stars;
    }

    public int GetProgress(int courseId, int lessonId)
    {
        string k = $"{courseId}:{lessonId}";
        var p = progress.Find(x => x.key == k);
        return p != null ? p.value : 0;
    }
}

[Serializable]
public class ProfilesContainer
{
    public List<Profile> profiles = new List<Profile>();
}
