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
                Debug.LogWarning("DataManager: ошибка чтени€ courses.json, возвращаю пустой контейнер");
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
                Debug.LogError("DataManager: ошибка сохранени€ courses " + ex);
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
            Debug.Log($"DataManager.LoadTasks called. path={TasksFilePath}");
            try
            {
                if (!File.Exists(TasksFilePath))
                {
                    Debug.Log("DataManager.LoadTasks: file not found, returning empty list");
                    return new List<TaskModel>();
                }

                var json = File.ReadAllText(TasksFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogWarning("DataManager.LoadTasks: file is empty, returning empty list");
                    return new List<TaskModel>();
                }

                var wrapper = JsonUtility.FromJson<TaskListWrapper>(json);
                var count = wrapper?.tasks?.Count ?? 0;
                Debug.Log($"DataManager.LoadTasks: loaded {count} task(s)");
                return wrapper?.tasks ?? new List<TaskModel>();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("DataManager.LoadTasks: exception reading tasks.json -> " + ex);
                return new List<TaskModel>();
            }
        }

        public static void SaveTasks(List<TaskModel> tasks)
        {
            try
            {
                // защита: убедимс€, что директори€ существует
                var dir = Path.GetDirectoryName(TasksFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var safeTasks = tasks ?? new List<TaskModel>();
                Debug.Log($"DataManager.SaveTasks called. tasks.Count={safeTasks.Count}, path={TasksFilePath}");

                var wrapper = new TaskListWrapper { tasks = safeTasks };
                var json = JsonUtility.ToJson(wrapper, true);

                File.WriteAllText(TasksFilePath, json);

                Debug.Log($"DataManager.SaveTasks finished. Written bytes={json.Length}. Time={System.DateTime.Now:O}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("DataManager.SaveTasks: exception saving tasks.json -> " + ex);
            }
        }

        // ¬спомогательна€ оболочка, поскольку Unity JsonUtility не поддерживает списки верхнего уровн€.
        [System.Serializable]
        private class TaskListWrapper
        {
            public List<TaskModel> tasks = new List<TaskModel>();
        }
        public static int GetNextTaskId(List<TaskModel> tasks)
        {
            if (tasks == null || tasks.Count == 0) return 1;
            var max = tasks.Where(t => t != null && t.id > 0).Select(t => t.id).DefaultIfEmpty(0).Max();
            return max + 1;
        }
    }
}
