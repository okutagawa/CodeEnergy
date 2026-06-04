using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MyGame.Models;

namespace MyGame.Data
{
    public static class DataManager
    {
        private static string CoursesFilePath => SaveService.GetPath(SaveService.CoursesFileName);
        private static string TasksFilePath => SaveService.GetPath(SaveService.TasksFileName);

        // Courses
        public static CoursesContainer LoadCourses()
        {
            SaveService.EnsureWorkingFiles();

            if (!File.Exists(CoursesFilePath)) return new CoursesContainer();
            try
            {
                var json = File.ReadAllText(CoursesFilePath);
                var container = JsonUtility.FromJson<CoursesContainer>(json) ?? new CoursesContainer();
                NormalizeCourseDefaults(container);
                return container;
            }
            catch
            {
                Debug.LogWarning("DataManager: failed to parse courses.json, returning empty container");
                return new CoursesContainer();
            }
        }

        public static void SaveCourses(CoursesContainer container)
        {
            SaveService.EnsureWorkingFiles();

            try
            {
                var safeContainer = container ?? new CoursesContainer();
                NormalizeCourseDefaults(safeContainer);
                var json = JsonUtility.ToJson(safeContainer, true);
                SaveService.SaveFile(SaveService.CoursesFileName, json);
                Debug.Log("DataManager: courses saved to " + CoursesFilePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("DataManager: error while saving courses " + ex);
            }
        }

        public static void NormalizeCourseDefaults(CoursesContainer container)
        {
            if (container?.courses == null) return;

            foreach (var course in container.courses)
            {
                if (course == null) continue;
                if (course.taskIds == null) course.taskIds = new List<int>();
                if (course.finalTest == null) course.finalTest = new FinalTestModel();
                NormalizeFinalTestDefaults(course.finalTest);
            }
        }

        public static void NormalizeFinalTestDefaults(FinalTestModel finalTest)
        {
            if (finalTest == null) return;
            if (finalTest.title == null) finalTest.title = string.Empty;
            if (finalTest.questions == null) finalTest.questions = new List<FinalTestQuestionModel>();

            foreach (var question in finalTest.questions)
            {
                if (question == null) continue;
                if (question.questionText == null) question.questionText = string.Empty;
                if (question.answers == null) question.answers = new List<string>();
                if (question.correctAnswerIndexes == null) question.correctAnswerIndexes = new List<int>();

                if (question.answerCount <= 0)
                    question.answerCount = question.answers.Count > 0 ? question.answers.Count : 4;

                question.answerCount = Mathf.Clamp(question.answerCount, 2, 5);
                question.correctAnswerIndexes = question.correctAnswerIndexes
                    .Where(index => index >= 0 && index < question.answerCount)
                    .Distinct()
                    .ToList();
            }

            var questionCount = finalTest.questions.Count;
            if (questionCount <= 0)
                finalTest.requiredCorrectAnswers = 0;
            else
                finalTest.requiredCorrectAnswers = Mathf.Clamp(finalTest.requiredCorrectAnswers <= 0 ? 1 : finalTest.requiredCorrectAnswers, 1, questionCount);
        }

        public static int NextCourseId(CoursesContainer container)
        {
            if (container == null || container.courses == null || container.courses.Count == 0) return 1;
            return container.courses.Max(c => c.id) + 1;
        }

        // Tasks
        public static List<TaskModel> LoadTasks()
        {
            SaveService.EnsureWorkingFiles();

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
                var tasks = wrapper?.tasks ?? new List<TaskModel>();
                var taskIdsMissingAnswerCount = GetTaskIdsMissingField(json, "answerCount");
                foreach (var task in tasks)
                {
                    if (task != null && taskIdsMissingAnswerCount.Contains(task.id))
                        task.answerCount = 0;
                }
                NormalizeTaskDefaults(tasks, !json.Contains("\"rewardEnabled\""));
                var count = tasks.Count;
                Debug.Log($"DataManager.LoadTasks: loaded {count} task(s)");
                return tasks;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("DataManager.LoadTasks: exception reading tasks.json -> " + ex);
                return new List<TaskModel>();
            }
        }

        public static void SaveTasks(List<TaskModel> tasks)
        {
            SaveService.EnsureWorkingFiles();

            try
            {
                // защита: убедимся, что директория существует
                var dir = Path.GetDirectoryName(TasksFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var safeTasks = tasks ?? new List<TaskModel>();
                NormalizeTaskDefaults(safeTasks);
                Debug.Log($"DataManager.SaveTasks called. tasks.Count={safeTasks.Count}, path={TasksFilePath}");

                var wrapper = new TaskListWrapper { tasks = safeTasks };
                var json = JsonUtility.ToJson(wrapper, true);

                SaveService.SaveFile(SaveService.TasksFileName, json);

                Debug.Log($"DataManager.SaveTasks finished. Written bytes={json.Length}. Time={System.DateTime.Now:O}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("DataManager.SaveTasks: exception saving tasks.json -> " + ex);
            }
        }

        public static void NormalizeTaskDefaults(List<TaskModel> tasks)
        {
            NormalizeTaskDefaults(tasks, false);
        }

        public static void NormalizeTaskDefaults(List<TaskModel> tasks, bool useLegacyRewardDefault)
        {
            if (tasks == null) return;

            foreach (var task in tasks)
            {
                NormalizeTaskDefaults(task, useLegacyRewardDefault);
            }
        }

        public static void NormalizeTaskDefaults(TaskModel task)
        {
            NormalizeTaskDefaults(task, false);
        }

        public static void NormalizeTaskDefaults(TaskModel task, bool useLegacyRewardDefault)
        {
            if (task == null) return;

            if (task.answers == null) task.answers = new List<string>();
            if (task.correctAnswerIndexes == null) task.correctAnswerIndexes = new List<int>();
            task.worldEvent = WorldEventKey.Normalize(task.worldEvent);

            if (task.answerCount <= 0)
                task.answerCount = task.answers.Count > 0 ? task.answers.Count : 4;

            task.answerCount = Mathf.Clamp(task.answerCount, 2, 5);

            if (task.maxStars <= 0) task.maxStars = 3;
            task.maxStars = Mathf.Clamp(task.maxStars, 1, 3);

            if (task.timeLimitSeconds <= 0f) task.timeLimitSeconds = 60f;
            if (task.hintText == null) task.hintText = "";
            task.hintCost = Mathf.Clamp(task.hintCost <= 0 ? 1 : task.hintCost, 1, 3);
            task.worldEvent = WorldEventKey.Normalize(task.worldEvent);

            // Old JSON files did not have rewardEnabled. For those files, default rewards to enabled.
            if (useLegacyRewardDefault) task.rewardEnabled = true;
            task.hasStars = task.rewardEnabled;

            task.correctAnswerIndexes = task.correctAnswerIndexes
                .Where(index => index >= 0 && index < task.answerCount)
                .Distinct()
                .ToList();
        }

        private static HashSet<int> GetTaskIdsMissingField(string json, string fieldName)
        {
            var result = new HashSet<int>();
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(fieldName))
                return result;

            var taskMatches = Regex.Matches(json, @"\{[^{}]*""id""\s*:\s*(-?\d+)[^{}]*\}");
            foreach (Match match in taskMatches)
            {
                if (!match.Success || match.Groups.Count < 2)
                    continue;

                var taskJson = match.Value;
                if (taskJson.Contains($"\"{fieldName}\""))
                    continue;

                if (int.TryParse(match.Groups[1].Value, out var taskId))
                    result.Add(taskId);
            }

            return result;
        }


        // Вспомогательная оболочка, поскольку Unity JsonUtility не поддерживает списки верхнего уровня.
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

        public static int GetNextTaskIdForCourse(List<TaskModel> tasks, int courseId, CoursesContainer coursesContainer)
        {
            if (tasks == null) tasks = new List<TaskModel>();

            var course = coursesContainer?.courses?.FirstOrDefault(c => c != null && c.id == courseId);
            var idsInCourse = new HashSet<int>();

            foreach (var task in tasks)
            {
                if (task != null && task.courseId == courseId && task.id >= 0)
                    idsInCourse.Add(task.id);
            }

            if (course?.taskIds != null)
            {
                foreach (var id in course.taskIds)
                {
                    if (id >= 0 && tasks.Any(task => task != null && task.id == id && (task.courseId == courseId || task.courseId <= 0)))
                        idsInCourse.Add(id);
                }
            }

            return idsInCourse.Count == 0 ? 0 : idsInCourse.Max() + 1;
        }
    }
}
