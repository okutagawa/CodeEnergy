using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using MyGame.Models;
using MyGame.Data;

public class TasksListManager : MonoBehaviour
{
    [Header("UI refs")]
    public RectTransform contentTasks;        // Content внутри ScrollView
    public GameObject prefabTaskItem;         // Prefab_TaskItem (Assets)
    public Text textCourseTitle;              // TitleText
    public Button buttonAddTask;
    public Button buttonEditTask;   // пока можно отключить или оставить
    public Button buttonDeleteTask;
    public Button buttonSave;
    public Button buttonExit;

    private CoursesContainer coursesContainer;
    private List<TaskModel> allTasks;
    private CourseModel currentCourse;
    private Dictionary<int, GameObject> instantiated = new Dictionary<int, GameObject>();
    private int selectedTaskId = -1;

    private void OnEnable()
    {
        if (buttonAddTask != null) buttonAddTask.onClick.AddListener(OnAddTaskClicked);
        if (buttonDeleteTask != null) buttonDeleteTask.onClick.AddListener(OnDeleteTaskClicked);
        if (buttonSave != null) buttonSave.onClick.AddListener(OnSaveClicked);
        if (buttonExit != null) buttonExit.onClick.AddListener(OnExitClicked);
        if (buttonEditTask != null) buttonEditTask.onClick.AddListener(OnEditTaskClicked);
    }

    private void OnDisable()
    {
        if (buttonAddTask != null) buttonAddTask.onClick.RemoveListener(OnAddTaskClicked);
        if (buttonDeleteTask != null) buttonDeleteTask.onClick.RemoveListener(OnDeleteTaskClicked);
        if (buttonSave != null) buttonSave.onClick.RemoveListener(OnSaveClicked);
        if (buttonExit != null) buttonExit.onClick.RemoveListener(OnExitClicked);
        if (buttonEditTask != null) buttonEditTask.onClick.RemoveListener(OnEditTaskClicked);
    }

    // Вызывается из UIManager.OpenTasksWindowForCourse(courseId)
    public void OpenForCourse(int courseId)
    {
        Debug.Log($"TasksListManager.OpenForCourse called for courseId={courseId}");
        // загрузка моделей
        coursesContainer = DataManager.LoadCourses();
        allTasks = DataManager.LoadTasks();
        currentCourse = coursesContainer.courses.Find(c => c.id == courseId);
        if (currentCourse == null)
        {
            Debug.LogError("TasksListManager: course not found " + courseId);
            textCourseTitle.text = $"(course {courseId} not found)";
            return;
        }

        textCourseTitle.text = currentCourse.name;
        selectedTaskId = -1;
        RefreshUI();
        UpdateButtons();
    }

    private void RefreshUI()
    {
        Debug.Log("TasksListManager.RefreshUI — clearing and recreating task items. currentCourse.taskIds count=" + (currentCourse?.taskIds?.Count ?? 0));
        // очистка старых элементов
        foreach (var kv in instantiated.Values) Destroy(kv);
        instantiated.Clear();

        if (currentCourse == null)
        {
            Debug.LogWarning("TasksListManager.RefreshUI: currentCourse is null");
            return;
        }

        // создаём TaskItem в том порядке, как в currentCourse.taskIds
        foreach (var id in currentCourse.taskIds)
        {
            var t = allTasks.Find(x => x.id == id);
            if (t == null)
            {
                Debug.LogWarning("TasksListManager.RefreshUI: task id not found in allTasks: " + id);
                continue;
            }
            AddTaskToUI(t);
        }
    }

    private void AddTaskToUI(TaskModel t)
    {
        var go = Instantiate(prefabTaskItem, contentTasks);
        var item = go.GetComponent<TaskItem>();
        if (item == null)
        {
            Debug.LogError("TasksListManager.AddTaskToUI: prefabTaskItem missing TaskItem component");
            Destroy(go);
            return;
        }

        item.Initialize(t);
        item.onSingleClick = OnTaskSingleClick;
        item.onDoubleClick = OnTaskDoubleClick;
        instantiated[t.id] = go;
        Debug.Log($"TasksListManager: instantiated TaskItem id={t.id} title='{t.title}'");
    }

    private void OnTaskSingleClick(TaskModel t) => SelectTask(t.id);

    private void OnTaskDoubleClick(TaskModel t)
    {
        SelectTask(t.id);
        OnEditTaskClicked();
    }

    public void SelectTask(int taskId)
    {
        if (selectedTaskId == taskId) return;
        if (instantiated.TryGetValue(selectedTaskId, out var prev)) prev.GetComponent<TaskItem>()?.SetSelected(false);
        selectedTaskId = taskId;
        if (instantiated.TryGetValue(selectedTaskId, out var cur)) cur.GetComponent<TaskItem>()?.SetSelected(true);
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool hasSelection = selectedTaskId >= 0;
        if (buttonDeleteTask != null) buttonDeleteTask.interactable = hasSelection;
        if (buttonEditTask != null) buttonEditTask.interactable = hasSelection;
    }

    // ADD: добавляем пустую задачу (для теста с пустыми именами)
    private void OnAddTaskClicked()
    {
        if (currentCourse == null)
        {
            Debug.LogError("TasksListManager.OnAddTaskClicked: currentCourse is null");
            return;
        }

        Debug.Log("OnAddTaskClicked: opening TaskEditor for course id = " + currentCourse.id);

        // получаем контроллер редактора через UIManager (надёжный путь)
        TaskEditorController editor = null;
        if (UIManager.Instance != null)
        {
            editor = UIManager.Instance.GetTaskEditorController();
        }

        // fallback: обычный Find
        if (editor == null) editor = FindObjectOfType<TaskEditorController>();

        if (editor == null)
        {
            Debug.LogError("TasksListManager: TaskEditorController not found. Ensure TaskEditorPanel exists in scene and has TaskEditorController attached.");
            return;
        }

        // Подготовить редактор к работе
        editor.OpenForCourseEditor(currentCourse.id);

        // Показать панель редактора через UIManager, если назначено
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.taskEditorPanel != null)
            {
                UIManager.Instance.ShowOnly(UIManager.Instance.taskEditorPanel);
            }
            else
            {
                UIManager.Instance.ShowOnly(UIManager.Instance.tasksPanel);
            }
        }
    }

    private void OnDeleteTaskClicked()
    {
        if (selectedTaskId < 0) return;
        // удаляем задачу из списков
        allTasks.RemoveAll(x => x.id == selectedTaskId);
        currentCourse.taskIds.RemoveAll(x => x == selectedTaskId);

        if (instantiated.TryGetValue(selectedTaskId, out var go)) { Destroy(go); instantiated.Remove(selectedTaskId); }
        selectedTaskId = -1;
        UpdateButtons();

        // Сохраним изменения сразу
        DataManager.SaveTasks(allTasks);
        DataManager.SaveCourses(coursesContainer);
        Debug.Log("TasksListManager: task deleted and data saved");
    }

    private void OnEditTaskClicked()
    {
        if (selectedTaskId < 0) return;

        var task = allTasks.Find(t => t.id == selectedTaskId);
        if (task == null)
        {
            Debug.LogError("TasksListManager: selected task not found id=" + selectedTaskId);
            return;
        }

        TaskEditorController editor = null;
        if (UIManager.Instance != null) editor = UIManager.Instance.GetTaskEditorController();
        if (editor == null) editor = FindObjectOfType<TaskEditorController>();
        if (editor == null)
        {
            Debug.LogError("TasksListManager: TaskEditorController not found for edit");
            return;
        }

        editor.OpenForEdit(task);

        if (UIManager.Instance != null && UIManager.Instance.taskEditorPanel != null)
            UIManager.Instance.ShowOnly(UIManager.Instance.taskEditorPanel);
        else if (UIManager.Instance != null)
            UIManager.Instance.ShowOnly(UIManager.Instance.tasksPanel); // fallback
    }

    private void OnSaveClicked()
    {
        if (allTasks == null) allTasks = DataManager.LoadTasks();
        if (coursesContainer == null) coursesContainer = DataManager.LoadCourses();

        DataManager.SaveTasks(allTasks);
        DataManager.SaveCourses(coursesContainer);
        Debug.Log("TasksListManager: saved tasks and courses");
    }

    private void OnExitClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowCoursesPanel();
        }
        else
        {
            Debug.LogWarning("TasksListManager: UIManager.Instance is null when trying to exit to courses");
        }
    }
}
