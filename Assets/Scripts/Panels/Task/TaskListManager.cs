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
    public Button buttonAddFinalTest;
    public Button buttonEditFinalTest;
    public Button buttonDeleteFinalTest;

    private CoursesContainer coursesContainer;
    private List<TaskModel> allTasks;
    private CourseModel currentCourse;
    private const int FinalTestTaskItemId = int.MinValue;
    private const string DefaultFinalTestTaskTitle = "Итоговый тест";
    private Dictionary<int, GameObject> instantiated = new Dictionary<int, GameObject>();
    private int selectedTaskId = -1;

    private void OnEnable()
    {
        TryAutoAssignOptionalButtons();
        if (buttonAddTask != null) { buttonAddTask.onClick.RemoveListener(OnAddTaskClicked); buttonAddTask.onClick.AddListener(OnAddTaskClicked); }
        if (buttonDeleteTask != null) { buttonDeleteTask.onClick.RemoveListener(OnDeleteTaskClicked); buttonDeleteTask.onClick.AddListener(OnDeleteTaskClicked); }
        if (buttonSave != null) { buttonSave.onClick.RemoveListener(OnSaveClicked); buttonSave.onClick.AddListener(OnSaveClicked); }
        if (buttonExit != null) { buttonExit.onClick.RemoveListener(OnExitClicked); buttonExit.onClick.AddListener(OnExitClicked); }
        if (buttonEditTask != null) { buttonEditTask.onClick.RemoveListener(OnEditTaskClicked); buttonEditTask.onClick.AddListener(OnEditTaskClicked); }
        if (buttonAddFinalTest != null) { buttonAddFinalTest.onClick.RemoveListener(OnEditFinalTestClicked); buttonAddFinalTest.onClick.AddListener(OnEditFinalTestClicked); }
        if (buttonEditFinalTest != null) { buttonEditFinalTest.onClick.RemoveListener(OnEditFinalTestClicked); buttonEditFinalTest.onClick.AddListener(OnEditFinalTestClicked); }
        if (buttonDeleteFinalTest != null) { buttonDeleteFinalTest.onClick.RemoveListener(OnDeleteFinalTestClicked); buttonDeleteFinalTest.onClick.AddListener(OnDeleteFinalTestClicked); }
    }

    private void OnDisable()
    {
        if (buttonAddTask != null) buttonAddTask.onClick.RemoveListener(OnAddTaskClicked);
        if (buttonDeleteTask != null) buttonDeleteTask.onClick.RemoveListener(OnDeleteTaskClicked);
        if (buttonSave != null) buttonSave.onClick.RemoveListener(OnSaveClicked);
        if (buttonExit != null) buttonExit.onClick.RemoveListener(OnExitClicked);
        if (buttonEditTask != null) buttonEditTask.onClick.RemoveListener(OnEditTaskClicked);
        if (buttonAddFinalTest != null) buttonAddFinalTest.onClick.RemoveListener(OnEditFinalTestClicked);
        if (buttonEditFinalTest != null) buttonEditFinalTest.onClick.RemoveListener(OnEditFinalTestClicked);
        if (buttonDeleteFinalTest != null) buttonDeleteFinalTest.onClick.RemoveListener(OnDeleteFinalTestClicked);
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
        foreach (var kv in instantiated.Values)
        {
            if (kv != null) Destroy(kv);
        }
        instantiated.Clear();

        if (contentTasks != null)
        {
            for (int i = contentTasks.childCount - 1; i >= 0; i--)
            {
                var child = contentTasks.GetChild(i);
                if (child != null) Destroy(child.gameObject);
            }
        }

        if (currentCourse == null)
        {
            Debug.LogWarning("TasksListManager.RefreshUI: currentCourse is null");
            return;
        }

        // создаём TaskItem в том порядке, как в currentCourse.taskIds
        foreach (var id in currentCourse.taskIds)
        {
            var t = FindTaskInCurrentCourse(id);
            if (t == null)
            {
                Debug.LogWarning("TasksListManager.RefreshUI: task id not found in allTasks: " + id);
                continue;
            }
            AddTaskToUI(t);
        }
        AddFinalTestToUI();
    }

    private TaskModel FindTaskInCurrentCourse(int taskId)
    {
        if (allTasks == null) return null;

        var courseId = currentCourse != null ? currentCourse.id : -1;
        var task = allTasks.Find(t => t != null && t.id == taskId && t.courseId == courseId);
        if (task != null) return task;

        return allTasks.Find(t => t != null && t.id == taskId && t.courseId <= 0);
    }

    private void AddTaskToUI(TaskModel t)
    {
        if (prefabTaskItem == null || contentTasks == null)
        {
            Debug.LogError("TasksListManager.AddTaskToUI: prefabTaskItem/contentTasks is not assigned");
            return;
        }
        var go = Instantiate(prefabTaskItem, contentTasks);
        var item = go.GetComponent<TaskItem>();
        if (item == null)
        {
            Debug.LogError("TasksListManager.AddTaskToUI: prefabTaskItem missing TaskItem component");
            Destroy(go);
            return;
        }

        item.Initialize(t);

        if (item.textTitle == null || item.buttonRoot == null)
        {
            Debug.LogError($"TasksListManager.AddTaskToUI: invalid TaskItem prefab setup for task id={t.id}. Destroying instance.");
            Destroy(go);
            return;
        }

        item.onSingleClick = OnTaskSingleClick;
        item.onDoubleClick = OnTaskDoubleClick;
        instantiated[t.id] = go;
        Debug.Log($"TasksListManager: instantiated TaskItem id={t.id} title='{t.title}'");
    }

    private void AddFinalTestToUI()
    {
        var finalTestItem = new TaskModel
        {
            id = FinalTestTaskItemId,
            courseId = currentCourse != null ? currentCourse.id : -1,
            title = GetFinalTestDisplayTitle()
        };

        AddTaskToUI(finalTestItem);
    }

    private string GetFinalTestDisplayTitle()
    {
        var title = currentCourse?.finalTest?.title;
        return string.IsNullOrWhiteSpace(title) ? DefaultFinalTestTaskTitle : title.Trim();
    }

    private bool HasFinalTest()
    {
        return currentCourse?.finalTest != null
            && currentCourse.finalTest.questions != null
            && currentCourse.finalTest.questions.Count > 0;
    }

    private bool IsFinalTestTask(TaskModel task)
    {
        return task != null && task.id == FinalTestTaskItemId;
    }

    private void OnTaskSingleClick(TaskModel t)
    {
        if (IsFinalTestTask(t))
        {
            SelectTask(FinalTestTaskItemId);
            return;
        }

        SelectTask(t.id);
    }

    private void OnTaskDoubleClick(TaskModel t)
    {
        if (IsFinalTestTask(t))
        {
            SelectTask(FinalTestTaskItemId);
            OnEditFinalTestClicked();
            return;
        }

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
        bool hasRegularTaskSelection = selectedTaskId >= 0;
        bool hasFinalTestSelection = selectedTaskId == FinalTestTaskItemId;
        if (buttonDeleteTask != null) buttonDeleteTask.interactable = hasRegularTaskSelection;
        if (buttonEditTask != null) buttonEditTask.interactable = hasRegularTaskSelection;
        if (buttonAddFinalTest != null) buttonAddFinalTest.interactable = currentCourse != null && !HasFinalTest();
        if (buttonEditFinalTest != null) buttonEditFinalTest.interactable = hasFinalTestSelection || HasFinalTest();
        if (buttonDeleteFinalTest != null) buttonDeleteFinalTest.interactable = HasFinalTest();
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
        editor.OpenForCourseEditor(currentCourse.id);
    }

    private void OnEditFinalTestClicked()
    {
        if (currentCourse == null)
        {
            Debug.LogError("TasksListManager.OnEditFinalTestClicked: currentCourse is null");
            return;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenFinalTestEditorForCourse(currentCourse.id);
            return;
        }

        var editor = FindObjectOfType<FinalTestEditorPanelController>(true);
        if (editor == null)
        {
            Debug.LogError("TasksListManager: FinalTestEditorPanelController not found.");
            return;
        }

        editor.gameObject.SetActive(true);
        editor.OpenForCourse(currentCourse.id);
    }

    private void OnDeleteFinalTestClicked()
    {
        if (currentCourse == null)
        {
            Debug.LogError("TasksListManager.OnDeleteFinalTestClicked: currentCourse is null");
            return;
        }

        currentCourse.finalTest = new FinalTestModel();
        DataManager.NormalizeFinalTestDefaults(currentCourse.finalTest);
        DataManager.SaveCourses(coursesContainer);

        if (instantiated.TryGetValue(FinalTestTaskItemId, out var go))
            go.GetComponent<TaskItem>()?.UpdateTitle(GetFinalTestDisplayTitle());

        selectedTaskId = -1;
        RefreshUI();
        UpdateButtons();
        Debug.Log($"TasksListManager: final test deleted for course id={currentCourse.id}");
    }

    private void TryAutoAssignOptionalButtons()
    {
        if (buttonAddFinalTest == null)
            buttonAddFinalTest = FindButtonInChildren("AddFinalTestBtn", "ButtonAddFinalTest");
        if (buttonEditFinalTest == null)
            buttonEditFinalTest = FindButtonInChildren("EditFinalTestBtn", "ButtonEditFinalTest");
        if (buttonDeleteFinalTest == null)
            buttonDeleteFinalTest = FindButtonInChildren("DeleteFinalTestBtn", "ButtonDeleteFinalTest");
    }

    private Button FindButtonInChildren(params string[] names)
    {
        foreach (var name in names)
        {
            var transforms = GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t != null && t.name == name)
                {
                    var button = t.GetComponent<Button>();
                    if (button != null) return button;
                }
            }
        }

        return null;
    }


    private void OnDeleteTaskClicked()
    {
        if (selectedTaskId == FinalTestTaskItemId)
        {
            OnDeleteFinalTestClicked();
            return;
        }
        if (selectedTaskId < 0) return;
        // удаляем задачу из списков
        var courseId = currentCourse != null ? currentCourse.id : -1;
        allTasks.RemoveAll(x => x != null && x.id == selectedTaskId && (x.courseId == courseId || x.courseId <= 0));
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
        if (selectedTaskId == FinalTestTaskItemId)
        {
            OnEditFinalTestClicked();
            return;
        }

        if (selectedTaskId < 0) return;

        var task = FindTaskInCurrentCourse(selectedTaskId);
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

        if (UIManager.Instance != null && UIManager.Instance.taskEditorPanel != null)
            UIManager.Instance.ShowOnly(UIManager.Instance.taskEditorPanel);
        else if (UIManager.Instance != null)
            UIManager.Instance.ShowOnly(UIManager.Instance.tasksPanel); // fallback

        editor.OpenForEdit(task);
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
