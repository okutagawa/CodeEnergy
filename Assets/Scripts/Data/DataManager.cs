using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MyGame.Models;

namespace MyGame.Data
{
    public static class DataManager
    {
        private static string CoursesFilePath => Path.Combine(Application.persistentDataPath, "courses.json");
        private static string TasksFilePath => Path.Combine(Application.persistentDataPath, "tasks.json");

        // Courses
        public static CoursesContainer LoadCourses()
        {
            if (!File.Exists(CoursesFilePath)) return new CoursesContainer();
            try
            {
                var json = File.ReadAllText(CoursesFilePath);
                return JsonUtility.FromJson<CoursesContainer>(json) ?? new CoursesContainer();
            }
            catch
            {
                Debug.LogWarning("DataManager: ошибка чтения courses.json, возвращаю пустой контейнер");
                return new CoursesContainer();
            }
        }

        public static void SaveCourses(CoursesContainer container)
        {
            try
            {
                var json = JsonUtility.ToJson(container, true);
                File.WriteAllText(CoursesFilePath, json);
                Debug.Log("DataManager: courses saved to " + CoursesFilePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("DataManager: ошибка сохранения courses " + ex);
            }
        }

        public static int NextCourseId(CoursesContainer container)
        {
            if (container == null || container.courses == null || container.courses.Count == 0) return 1;
            return container.courses.Max(c => c.id) + 1;
        }

        // Tasks
        public static List<TaskModel> LoadTasks()
        {
            if (!File.Exists(TasksFilePath))
            {
                // fallback default list
                return new List<TaskModel>
                {
                    new TaskModel { id = 1, title = "Task A" },
                    new TaskModel { id = 2, title = "Task B" },
                    new TaskModel { id = 3, title = "Task C" }
                };
            }

            try
            {
                var json = File.ReadAllText(TasksFilePath);
                var wrapper = JsonUtility.FromJson<TaskListWrapper>(json);
                return wrapper?.tasks ?? new List<TaskModel>();
            }
            catch
            {
                Debug.LogWarning("DataManager: ошибка чтения tasks.json, возвращаю дефолтный список");
                return new List<TaskModel>
                {
                    new TaskModel { id = 1, title = "Task A" },
                    new TaskModel { id = 2, title = "Task B" },
                    new TaskModel { id = 3, title = "Task C" }
                };
            }
        }

        public static void SaveTasks(List<TaskModel> tasks)
        {
            try
            {
                var wrapper = new TaskListWrapper { tasks = tasks ?? new List<TaskModel>() };
                var json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(TasksFilePath, json);
                Debug.Log("DataManager: tasks saved to " + TasksFilePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("DataManager: ошибка сохранения tasks " + ex);
            }
        }

        // Helper wrapper because Unity JsonUtility doesn't handle top-level lists.
        [System.Serializable]
        private class TaskListWrapper
        {
            public List<TaskModel> tasks = new List<TaskModel>();
        }
    }
}
