using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MyGame.Data;
using MyGame.Models;

public class TaskEditorController : MonoBehaviour
{
    [Header("UI refs")]
    public Text titleText;
    public RectTransform contentRows;      // TasksEditorScroll/Viewport/Content
    public GameObject prefabTaskRow;       // TaskRow prefab (Asset)
    public Button createTaskBtn;
    public Button saveAndExitBtn;
    public ScrollRect scrollRect;          // optional, for auto-scrolling

    [Header("NPC list source")]
    public List<string> npcNames = new List<string>(); // fill in inspector or dynamically

    [Header("Integration")]
    public InputFieldExpander inputFieldExpander; // assign in inspector

    private int contextCourseId = -1;
    private List<TaskModel> allTasks;
    private CoursesContainer coursesContainer;
    private List<TaskRow> activeRows = new List<TaskRow>();

    // editing single task mode
    private bool isEditing = false;
    private int editingTaskId = -1;

    void Awake()
    {
        if (createTaskBtn != null) { createTaskBtn.onClick.RemoveAllListeners(); createTaskBtn.onClick.AddListener(OnCreateRow); }
        if (saveAndExitBtn != null) { saveAndExitBtn.onClick.RemoveAllListeners(); saveAndExitBtn.onClick.AddListener(OnSaveAndExit); }
    }

    // Open editor for a course: load existing tasks and show rows
    public void OpenForCourseEditor(int courseId)
    {
        isEditing = false;
        editingTaskId = -1;

        contextCourseId = courseId;
        coursesContainer = DataManager.LoadCourses();
        allTasks = DataManager.LoadTasks();

        var course = coursesContainer.courses.Find(c => c.id == courseId);
        if (course == null)
        {
            Debug.LogError("TaskEditorController: course not found " + courseId);
            return;
        }

        titleText.text = $"Редактирование заданий курса: {course.name}";
        ClearAllRows();

        if (course.taskIds != null && course.taskIds.Count > 0)
        {
            foreach (var tid in course.taskIds)
            {
                var model = allTasks.FirstOrDefault(t => t.id == tid);
                var go = Instantiate(prefabTaskRow, contentRows);
                var row = go.GetComponent<TaskRow>();
                if (row == null)
                {
                    Debug.LogError("TaskEditorController: prefabTaskRow missing TaskRow component");
                    Destroy(go);
                    continue;
                }

                row.Initialize(activeRows.Count + 1);
                PopulateNpcDropdown(row);
                row.SetupDropdownLabels();
                if (inputFieldExpander != null) row.SetupExpander(inputFieldExpander);

                if (model != null)
                {
                    row.FillFromModel(model);
                    row.SetExistingTaskId(model.id);

                    if (row.dropdownGiver != null && npcNames != null && npcNames.Count > 0)
                    {
                        var idx = npcNames.IndexOf(model.giverNpc);
                        row.dropdownGiver.value = idx >= 0 ? idx : 0;
                    }
                    if (row.dropdownReceiver != null && npcNames != null && npcNames.Count > 0)
                    {
                        var idx = npcNames.IndexOf(model.receiverNpc);
                        row.dropdownReceiver.value = idx >= 0 ? idx : 0;
                    }
                }
                else
                {
                    row.SetExistingTaskId(-1);
                }

                activeRows.Add(row);
            }
        }
        else
        {
            OnCreateRow();
        }

        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentRows);
        if (scrollRect != null) StartCoroutine(ScrollToBottomNextFrame());
    }

    // Open for editing a single existing TaskModel (called from TasksListManager.Edit)
    public void OpenForEdit(TaskModel model)
    {
        if (model == null) { Debug.LogError("TaskEditorController.OpenForEdit: model is null"); return; }

        isEditing = true;
        editingTaskId = model.id;

        coursesContainer = DataManager.LoadCourses();
        allTasks = DataManager.LoadTasks();

        titleText.text = $"Редактирование задания: {model.title} (id={model.id})";
        ClearAllRows();

        var go = Instantiate(prefabTaskRow, contentRows);
        var row = go.GetComponent<TaskRow>();
        if (row == null) { Debug.LogError("TaskEditorController: prefabTaskRow missing TaskRow component"); Destroy(go); return; }

        row.Initialize(1);
        PopulateNpcDropdown(row);
        row.SetupDropdownLabels();
        if (inputFieldExpander != null) row.SetupExpander(inputFieldExpander);

        row.FillFromModel(model);

        if (row.dropdownGiver != null && npcNames != null && npcNames.Count > 0)
        {
            var idx = npcNames.IndexOf(model.giverNpc);
            row.dropdownGiver.value = idx >= 0 ? idx : 0;
        }
        if (row.dropdownReceiver != null && npcNames != null && npcNames.Count > 0)
        {
            var idx = npcNames.IndexOf(model.receiverNpc);
            row.dropdownReceiver.value = idx >= 0 ? idx : 0;
        }

        activeRows.Clear();
        activeRows.Add(row);

        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentRows);
        if (scrollRect != null) StartCoroutine(ScrollToBottomNextFrame());
    }

    public void OnCreateRow()
    {
        var go = Instantiate(prefabTaskRow, contentRows);
        var row = go.GetComponent<TaskRow>();
        if (row == null)
        {
            Debug.LogError("TaskEditorController: prefabTaskRow missing TaskRow component");
            Destroy(go);
            return;
        }

        row.Initialize(activeRows.Count + 1);
        PopulateNpcDropdown(row);
        row.SetupDropdownLabels();
        if (inputFieldExpander != null) row.SetupExpander(inputFieldExpander);
        row.SetExistingTaskId(-1);

        activeRows.Add(row);

        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentRows);
        if (scrollRect != null) StartCoroutine(ScrollToBottomNextFrame());
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    private void PopulateNpcDropdown(TaskRow row)
    {
        if (row == null) return;
        if (row.dropdownGiver != null)
        {
            row.dropdownGiver.ClearOptions();
            row.dropdownGiver.AddOptions(npcNames);
        }
        if (row.dropdownReceiver != null)
        {
            row.dropdownReceiver.ClearOptions();
            row.dropdownReceiver.AddOptions(npcNames);
        }
    }

    private void ClearAllRows()
    {
        foreach (Transform c in contentRows) Destroy(c.gameObject);
        activeRows.Clear();
    }

    // Save: update existing tasks or create new ones; add new ids to course.taskIds
    public void OnSaveAndExit()
    {
        coursesContainer = coursesContainer ?? DataManager.LoadCourses();
        allTasks = allTasks ?? DataManager.LoadTasks();

        var course = coursesContainer.courses.Find(c => c.id == contextCourseId);
        if (course == null)
        {
            Debug.LogError("TaskEditorController: course not found when saving");
            return;
        }

        // update existing or create new
        foreach (var row in activeRows)
        {
            if (!row.IsValid(out var msg))
            {
                Debug.LogWarning($"TaskEditorController: skipping invalid row: {msg}");
                continue;
            }

            var data = row.ToRowData();

            if (row.existingTaskId >= 0)
            {
                var model = allTasks.FirstOrDefault(t => t.id == row.existingTaskId);
                if (model != null)
                {
                    model.title = data.title;
                    model.giverNpc = data.giverIndex >= 0 && data.giverIndex < npcNames.Count ? npcNames[data.giverIndex] : "";
                    model.receiverNpc = data.receiverIndex >= 0 && data.receiverIndex < npcNames.Count ? npcNames[data.receiverIndex] : "";
                    model.textForGiver = data.textForGiver;
                    model.textForReceiver = data.textForReceiver;
                    model.answers = data.answers.ToList();
                    model.correctAnswerIndexes = data.correctAnswerIndexes.ToList();
                    model.hasStars = data.hasStars;
                }
                else
                {
                    int nid = allTasks.Any() ? allTasks.Max(t => t.id) + 1 : 1;
                    var tm = new TaskModel
                    {
                        id = nid,
                        title = data.title,
                        giverNpc = data.giverIndex >= 0 && data.giverIndex < npcNames.Count ? npcNames[data.giverIndex] : "",
                        receiverNpc = data.receiverIndex >= 0 && data.receiverIndex < npcNames.Count ? npcNames[data.receiverIndex] : "",
                        textForGiver = data.textForGiver,
                        textForReceiver = data.textForReceiver,
                        answers = data.answers.ToList(),
                        correctAnswerIndexes = data.correctAnswerIndexes.ToList(),
                        hasStars = data.hasStars
                    };
                    allTasks.Add(tm);
                    course.taskIds.Add(tm.id);
                }
            }
            else
            {
                int nextId = DataManager.GetNextTaskId(allTasks);
                var tm = new TaskModel
                {
                    id = nextId,
                    title = data.title,
                    giverNpc = data.giverIndex >= 0 && data.giverIndex < npcNames.Count ? npcNames[data.giverIndex] : "",
                    receiverNpc = data.receiverIndex >= 0 && data.receiverIndex < npcNames.Count ? npcNames[data.receiverIndex] : "",
                    textForGiver = data.textForGiver,
                    textForReceiver = data.textForReceiver,
                    answers = data.answers.ToList(),
                    correctAnswerIndexes = data.correctAnswerIndexes.ToList(),
                    hasStars = data.hasStars
                };

                allTasks.Add(tm);
                course.taskIds.Add(tm.id);
            }
        }

        DataManager.SaveTasks(allTasks);
        DataManager.SaveCourses(coursesContainer);

        UIManager.Instance?.OpenTasksWindowForCourse(contextCourseId);
    }
}
