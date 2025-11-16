using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MyGame.Models;
using MyGame.Data;

public class TaskEditorController : MonoBehaviour
{
    [Header("UI refs")]
    public Text titleText;
    public RectTransform contentRows;      // TasksEditorScroll/Viewport/Content
    public GameObject prefabTaskRow;       // TaskRow 
    public Button createTaskBtn;
    public Button saveAndExitBtn;
    public ScrollRect scrollRect;          // optional

    [Header("Integration")]
    public InputFieldExpander inputFieldExpander; // назначить в инспекторе

    private int contextCourseId = -1;
    private List<TaskModel> allTasks;
    private CoursesContainer coursesContainer;
    private List<TaskRow> activeRows = new List<TaskRow>();

    // редактирование режима выполнения одной задачи
    private bool isEditing = false;
    private int editingTaskId = -1;

    void Awake()
    {
        if (createTaskBtn != null) { createTaskBtn.onClick.RemoveAllListeners(); createTaskBtn.onClick.AddListener(OnCreateRow); }
        if (saveAndExitBtn != null) { saveAndExitBtn.onClick.RemoveAllListeners(); saveAndExitBtn.onClick.AddListener(OnSaveAndExit); }
    }

    // загрузка существующих заданий и отображения строк
    public void OpenForCourseEditor(int courseId)
    {
        isEditing = false;
        editingTaskId = -1;
        contextCourseId = courseId;
        coursesContainer = DataManager.LoadCourses();
        allTasks = DataManager.LoadTasks();

        var course = coursesContainer?.courses?.Find(c => c.id == courseId);
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

                // 1) Заполнить UI из модели (поля текста, ответы и т.д.)
                if (model != null)
                {
                    row.FillFromModel(model);
                    row.SetExistingTaskId(model.id);
                }
                else
                {
                    row.SetExistingTaskId(-1);
                }

                // 2) Заполнить дропдауны NPC из проектного провайдера (или сценовых префабов)
                PopulateNpcDropdown(row);

                // 3) Выставить выбранные значения по GUID из модели (делает поиск по giverNpcGuid / receiverNpcGuid)
                if (model != null) row.ApplyModelSelection(model);

                // Expander
                if (inputFieldExpander != null) row.SetupExpander(inputFieldExpander);

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

    // Открыт для редактирования одной существующей модели TaskModel (вызывается из TasksListManager.Edit)
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

        // NPC options
        PopulateNpcDropdown(row);
        row.SetupDropdownLabels();

        if (inputFieldExpander != null) row.SetupExpander(inputFieldExpander);

        // fill fields and selection by GUID
        row.FillFromModel(model);
        row.ApplyModelSelection(model);

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

        // NPC из проекта/ресурсов
        PopulateNpcDropdown(row);
        row.SetupDropdownLabels();

        // Expander
        if (inputFieldExpander != null) row.SetupExpander(inputFieldExpander);

        // Новая строка
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

        // Если ты используешь префабы в Resources, ProjectNpcProvider вернёт их.
        // Иначе можно заменить на SceneNpcRegistry.Instance.GetAll() если сцена с NPC загружена.
        var npcs = ProjectNpcProvider.GetAllFromResources() ?? new List<NPCIdentity>();
        // защита: если в проекте нет префабов — попробуем сценовый реестр
        if (npcs.Count == 0)
        {
            npcs = SceneNpcRegistry.Instance.GetAll() ?? new List<NPCIdentity>();
            Debug.Log($"PopulateNpcDropdown: fallback to SceneNpcRegistry found {npcs.Count} NPC(s).");
        }
        else
        {
            Debug.Log($"PopulateNpcDropdown: found {npcs.Count} project NPC prefab(s).");
        }

        var names = npcs.Select(n => n.DisplayName).ToList();
        var guids = npcs.Select(n => n.Guid).ToList();

        if (row.dropdownGiver != null)
        {
            row.dropdownGiver.ClearOptions();
            if (names.Count > 0)
            {
                row.dropdownGiver.AddOptions(names);
                row.giverOptionGuids = new List<string>(guids);
                row.dropdownGiver.interactable = true;
            }
            else
            {
                row.dropdownGiver.AddOptions(new List<string> { "(no NPCs found)" });
                row.giverOptionGuids = new List<string>();
                row.dropdownGiver.interactable = false;
            }
            row.dropdownGiver.RefreshShownValue();
        }

        if (row.dropdownReceiver != null)
        {
            row.dropdownReceiver.ClearOptions();
            if (names.Count > 0)
            {
                row.dropdownReceiver.AddOptions(names);
                row.receiverOptionGuids = new List<string>(guids);
                row.dropdownReceiver.interactable = true;
            }
            else
            {
                row.dropdownReceiver.AddOptions(new List<string> { "(no NPCs found)" });
                row.receiverOptionGuids = new List<string>();
                row.dropdownReceiver.interactable = false;
            }
            row.dropdownReceiver.RefreshShownValue();
        }

        row.SetupDropdownLabels();
    }

    private void ClearAllRows()
    {
        if (contentRows == null) return;
        foreach (Transform c in contentRows) Destroy(c.gameObject);
        activeRows.Clear();
    }

    // Сохранение: обновляет существующие задачи или создает новые; добавляет новые идентификаторы в course.taskIds
    public void OnSaveAndExit()
    {
        coursesContainer = coursesContainer ?? DataManager.LoadCourses();
        allTasks = allTasks ?? DataManager.LoadTasks();

        var course = coursesContainer?.courses?.Find(c => c.id == contextCourseId);
        if (course == null)
        {
            Debug.LogError("TaskEditorController: course not found when saving");
            return;
        }

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
                    model.giverNpcGuid = data.giverNpcGuid;
                    model.receiverNpcGuid = data.receiverNpcGuid;
                    model.textForGiver = data.textForGiver;
                    model.textForReceiver = data.textForReceiver;
                    model.answers = data.answers.ToList();
                    model.correctAnswerIndexes = data.correctAnswerIndexes.ToList();
                    model.hasStars = data.hasStars;
                }
                else
                {
                    int nid = DataManager.GetNextTaskId(allTasks);
                    var tm = new TaskModel
                    {
                        id = nid,
                        title = data.title,
                        giverNpcGuid = data.giverNpcGuid,
                        receiverNpcGuid = data.receiverNpcGuid,
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
                    giverNpcGuid = data.giverNpcGuid,
                    receiverNpcGuid = data.receiverNpcGuid,
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
