using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MyGame.Data;
using MyGame.Models;
using UnityEngine;
using UnityEngine.UI;

public class TaskEditorController : MonoBehaviour
{
    private const int MinAnswerCount = 2;
    private const int MaxAnswerCount = 5;
    private const string DefaultWorldEvent = "None";

    private static readonly string[] WorldEventKeys =
    {
        "None",
        "FixLanterns",
        "UnlockDoors",
        "StartGenerator",
        "ActivatePortal",
        "CompleteIsland"
    };

    private static readonly string[] WorldEventLabels =
    {
        "Нет события",
        "Починить фонари",
        "Открыть двери",
        "Запустить генератор",
        "Активировать портал",
        "Завершить остров"
    };

    [Header("Title")]
    public Text titleText;
    [Header("Section_MainInfo")]
    public InputField inputTaskId;
    public InputField inputTaskTitle;

    [Header("Section_NPC")]
    public Dropdown dropdownGiverNPC;
    public Dropdown dropdownReceiverNPC;

    [Header("Section_Dialogues")]
    public InputField inputFirstNPCText;
    public InputField inputSecondNPCText;

    [Header("Section_Question")]
    public InputField inputQuestionText;

    [Header("Section_Answers")]
    public Dropdown dropdownAnswersCount;
    public GameObject rowAnswer1;
    public GameObject rowAnswer2;
    public GameObject rowAnswer3;
    public GameObject rowAnswer4;
    public GameObject rowAnswer5;
    public InputField inputAnswer1;
    public InputField inputAnswer2;
    public InputField inputAnswer3;
    public InputField inputAnswer4;
    public InputField inputAnswer5;
    public Toggle toggleCorrect1;
    public Toggle toggleCorrect2;
    public Toggle toggleCorrect3;
    public Toggle toggleCorrect4;
    public Toggle toggleCorrect5;

    [Header("Section_Rewards")]
    public Toggle toggleRewardEnabled;
    public Dropdown dropdownMaxStars;
    public InputField inputTimeLimit;

    [Header("Section_WorldEvent")]
    public Dropdown dropdownWorldEvent;

    [Header("RightButtonsPanel")]
    public Button buttonSaveTask;
    public Button buttonClearForm;
    public Button buttonBack;

    [Header("Optional")]
    public ScrollRect scrollRect;
    public InputFieldExpander inputFieldExpander;

    private int contextCourseId = -1;
    private bool isEditing;
    private int editingTaskId = -1;
    private List<TaskModel> allTasks = new List<TaskModel>();
    private CoursesContainer coursesContainer;
    private List<string> giverOptionGuids = new List<string>();
    private List<string> receiverOptionGuids = new List<string>();

    private InputField[] AnswerInputs => new[] { inputAnswer1, inputAnswer2, inputAnswer3, inputAnswer4, inputAnswer5 };
    private Toggle[] CorrectToggles => new[] { toggleCorrect1, toggleCorrect2, toggleCorrect3, toggleCorrect4, toggleCorrect5 };
    private GameObject[] AnswerRows => new[] { rowAnswer1, rowAnswer2, rowAnswer3, rowAnswer4, rowAnswer5 };

    private void Awake()
    {
        TryAutoAssignUiRefs();
        SetupStaticDropdowns();
        BindButtons();
        BindAnswerCountDropdown();
        DisableLegacyInputExpander();
    }

    // загрузка существующих заданий и отображения строк
    private void OnEnable()
    {
        BindButtons();
        BindAnswerCountDropdown();
    }

    private void OnDisable()
    {
        if (buttonSaveTask != null) buttonSaveTask.onClick.RemoveListener(OnSaveTaskClicked);
        if (buttonClearForm != null) buttonClearForm.onClick.RemoveListener(OnClearFormClicked);
        if (buttonBack != null) buttonBack.onClick.RemoveListener(OnBackClicked);
        if (dropdownAnswersCount != null) dropdownAnswersCount.onValueChanged.RemoveListener(OnAnswersCountChanged);
    }

    public void OpenForCourseEditor(int courseId)
    {
        OpenForCreate(courseId);
    }

    public void OpenForCreate(int courseId)
    {
        if (!EnsureUiReady(nameof(OpenForCreate))) return;

        coursesContainer = DataManager.LoadCourses();
        allTasks = DataManager.LoadTasks();
        contextCourseId = courseId;
        isEditing = false;
        editingTaskId = -1;

        PopulateNpcDropdowns();
        SetupStaticDropdowns();
        ClearFormFields(DataManager.GetNextTaskIdForCourse(allTasks, contextCourseId, coursesContainer));

        var course = coursesContainer?.courses?.Find(c => c.id == courseId);
        if (titleText != null) titleText.text = course != null ? $"Создание задания: {course.name}" : "Создание задания";

        Debug.Log($"[TaskEditor] Open create form for courseId={courseId}, nextTaskId={GetDisplayedTaskId()}");
    }

    // Открыт для редактирования одной существующей модели TaskModel (вызывается из TasksListManager.Edit)
    public void OpenForEdit(TaskModel model)
    {
        if (model == null)
        {
            Debug.LogError("[TaskEditor] Open edit form failed: model is null");
            return;
        }

        if (!EnsureUiReady(nameof(OpenForEdit))) return;

        // Попытаемся определить courseId, если он не передан явно
        coursesContainer = DataManager.LoadCourses();
        allTasks = DataManager.LoadTasks();
        DataManager.NormalizeTaskDefaults(model);

        contextCourseId = FindCourseIdForTask(model.id);
        isEditing = true;
        editingTaskId = model.id;
        PopulateNpcDropdowns();
        SetupStaticDropdowns();
        FillForm(model);

        if (titleText != null) titleText.text = $"Редактирование задания: {model.title} (id={model.id})";
        Debug.Log($"[TaskEditor] Open edit form for taskId={model.id}, courseId={contextCourseId}");
    }

    private void BindButtons()
    {
        if (buttonSaveTask != null)
        {
            buttonSaveTask.onClick.RemoveListener(OnSaveTaskClicked);
            buttonSaveTask.onClick.AddListener(OnSaveTaskClicked);
        }

        if (buttonClearForm != null)
        {
            buttonClearForm.onClick.RemoveListener(OnClearFormClicked);
            buttonClearForm.onClick.AddListener(OnClearFormClicked);
        }

        if (buttonBack != null)
        {
            buttonBack.onClick.RemoveListener(OnBackClicked);
            buttonBack.onClick.AddListener(OnBackClicked);
        }
    }

    private void BindAnswerCountDropdown()
    {
        if (dropdownAnswersCount == null) return;
        dropdownAnswersCount.onValueChanged.RemoveListener(OnAnswersCountChanged);
        dropdownAnswersCount.onValueChanged.AddListener(OnAnswersCountChanged);
        UpdateAnswerRows(GetSelectedAnswerCount());
    }

    private void SetupStaticDropdowns()
    {
        if (dropdownAnswersCount != null)
        {
            dropdownAnswersCount.ClearOptions();
            dropdownAnswersCount.AddOptions(new List<string> { "2", "3", "4", "5" });
            if (dropdownAnswersCount.value < 0 || dropdownAnswersCount.value > 3) dropdownAnswersCount.value = 0;
            dropdownAnswersCount.RefreshShownValue();
        }
        if (dropdownMaxStars != null)
        {
            dropdownMaxStars.ClearOptions();
            dropdownMaxStars.AddOptions(new List<string> { "1", "2", "3" });
            dropdownMaxStars.value = 2;
            dropdownMaxStars.RefreshShownValue();
        }

        if (dropdownWorldEvent != null)
        {
            dropdownWorldEvent.ClearOptions();
            dropdownWorldEvent.AddOptions(WorldEventLabels.ToList());
            dropdownWorldEvent.value = 0;
            dropdownWorldEvent.RefreshShownValue();
        }
    }

    private void TryAutoAssignUiRefs()
    {
        if (titleText == null) titleText = GetComponentInChildren<Text>(true);

        inputTaskId = inputTaskId ?? FindInput("Input_TaskId", "Input_TaskIdd");
        inputTaskTitle = inputTaskTitle ?? FindInput("Input_TaskTitle");
        inputFirstNPCText = inputFirstNPCText ?? FindInput("Input_FirstNPCText");
        inputSecondNPCText = inputSecondNPCText ?? FindInput("Input_SecondNPCText");
        inputQuestionText = inputQuestionText ?? FindInput("Input_QuestionText");
        inputAnswer1 = inputAnswer1 ?? FindInput("Input_Answer1");
        inputAnswer2 = inputAnswer2 ?? FindInput("Input_Answer2");
        inputAnswer3 = inputAnswer3 ?? FindInput("Input_Answer3");
        inputAnswer4 = inputAnswer4 ?? FindInput("Input_Answer4");
        inputAnswer5 = inputAnswer5 ?? FindInput("Input_Answer5");
        inputTimeLimit = inputTimeLimit ?? FindInput("Input_TimeLimit");

        dropdownGiverNPC = dropdownGiverNPC ?? FindDropdown("Dropdown_GiverNPC");
        dropdownReceiverNPC = dropdownReceiverNPC ?? FindDropdown("Dropdown_ReceiverNPC");
        dropdownAnswersCount = dropdownAnswersCount ?? FindDropdown("Dropdown_AnswersCount");
        dropdownMaxStars = dropdownMaxStars ?? FindDropdown("Dropdown_MaxStars");
        dropdownWorldEvent = dropdownWorldEvent ?? FindDropdown("Dropdown_WorldEvent");

        toggleCorrect1 = toggleCorrect1 ?? FindToggle("Toggle_Correct1");
        toggleCorrect2 = toggleCorrect2 ?? FindToggle("Toggle_Correct2");
        toggleCorrect3 = toggleCorrect3 ?? FindToggle("Toggle_Correct3");
        toggleCorrect4 = toggleCorrect4 ?? FindToggle("Toggle_Correct4");
        toggleCorrect5 = toggleCorrect5 ?? FindToggle("Toggle_Correct5");
        toggleRewardEnabled = toggleRewardEnabled ?? FindToggle("Toggle_RewardEnabled");

        rowAnswer1 = rowAnswer1 ?? FindChildGameObject("Row_Answer1");
        rowAnswer2 = rowAnswer2 ?? FindChildGameObject("Row_Answer2");
        rowAnswer3 = rowAnswer3 ?? FindChildGameObject("Row_Answer3");
        rowAnswer4 = rowAnswer4 ?? FindChildGameObject("Row_Answer4");
        rowAnswer5 = rowAnswer5 ?? FindChildGameObject("Row_Answer5");

        buttonSaveTask = buttonSaveTask ?? FindButton("ButtonSaveTask");
        buttonClearForm = buttonClearForm ?? FindButton("ButtonClearForm");
        buttonBack = buttonBack ?? FindButton("ButtonBack");
        inputFieldExpander = inputFieldExpander ?? GetComponentInChildren<InputFieldExpander>(true);
    }

    private bool EnsureUiReady(string caller)
    {
        TryAutoAssignUiRefs();

        if (inputTaskTitle == null || inputQuestionText == null || dropdownAnswersCount == null || buttonSaveTask == null)
        {
            Debug.LogError($"TaskEditorController.{caller}: required form references are not assigned. Assign TaskEditorPanel fields in Inspector.");
            return false;
        }

        return true;
    }

    private InputField FindInput(params string[] names) => FindComponentByNames<InputField>(names);
    private Dropdown FindDropdown(params string[] names) => FindComponentByNames<Dropdown>(names);
    private Toggle FindToggle(params string[] names) => FindComponentByNames<Toggle>(names);
    private Button FindButton(params string[] names) => FindComponentByNames<Button>(names);

    private T FindComponentByNames<T>(params string[] names) where T : Component
    {
        foreach (var name in names)
        {
            var go = FindChildGameObject(name);
            if (go != null)
            {
                var component = go.GetComponent<T>();
                if (component != null) return component;
            }
        }

        return null;
    }

    private GameObject FindChildGameObject(string childName)
    {
        if (string.IsNullOrEmpty(childName)) return null;
        var transforms = GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms)
        {
            if (t != null && t.name == childName) return t.gameObject;
        }

        return null;
    }


    private void PopulateNpcDropdowns()
    {
        var combined = GetNpcOptions();
        var names = combined.Select(n => n.DisplayName).ToList();
        var guids = combined.Select(n => n.Guid).ToList();
        SetupNpcDropdown(dropdownGiverNPC, names, guids, giverOptionGuids);
        SetupNpcDropdown(dropdownReceiverNPC, names, guids, receiverOptionGuids);
        Debug.Log($"[TaskEditor] NPC dropdowns populated: {combined.Count} option(s)");
    }

    private List<NPCIdentity> GetNpcOptions()
    {
        var sceneNpcs = new List<NPCIdentity>();
        try
        {
            // Попытка явно пересобрать индекс
            SceneNpcRegistry.Instance.RebuildIndex();
            sceneNpcs = SceneNpcRegistry.Instance.GetAll() ?? new List<NPCIdentity>();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[TaskEditor] SceneNpcRegistry unavailable: " + ex.Message);
        }
        
        if (sceneNpcs.Count == 0)
        {
            var direct = FindObjectsOfType<NPCIdentity>(true);
            if (direct != null) sceneNpcs = direct.ToList();
        }

      
        var projectNpcs = ProjectNpcProvider.GetAllFromResources() ?? new List<NPCIdentity>();

        
        var combined = new List<NPCIdentity>();
        var seenGuids = new HashSet<string>();

        foreach (var npc in sceneNpcs.Concat(projectNpcs))
        {
            if (npc == null || string.IsNullOrEmpty(npc.Guid)) continue;
            if (seenGuids.Add(npc.Guid)) combined.Add(npc);
        }

        return combined;
    }

    private void SetupNpcDropdown(Dropdown dropdown, List<string> names, List<string> guids, List<string> targetGuidList)
    {
        targetGuidList.Clear();
        if (dropdown == null) return;

        dropdown.ClearOptions();
        if (names.Count > 0)
        {
            dropdown.AddOptions(names);
            targetGuidList.AddRange(guids);
            dropdown.interactable = true;
        }
        else
        {
            dropdown.AddOptions(new List<string> { "NPC не найдены" });
            dropdown.interactable = false;
        }

        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    private void FillForm(TaskModel model)
    {
        if (inputTaskId != null)
        {
            inputTaskId.text = model.id.ToString(CultureInfo.InvariantCulture);
            inputTaskId.interactable = false;
        }

        if (inputTaskTitle != null) inputTaskTitle.text = model.title ?? "";
        SetNpcSelection(dropdownGiverNPC, giverOptionGuids, model.giverNpcGuid);
        SetNpcSelection(dropdownReceiverNPC, receiverOptionGuids, model.receiverNpcGuid);
        if (inputFirstNPCText != null) inputFirstNPCText.text = model.textForGiver ?? "";
        if (inputSecondNPCText != null) inputSecondNPCText.text = model.textForReceiver ?? "";
        if (inputQuestionText != null) inputQuestionText.text = model.questionText ?? model.textForReceiver ?? "";

        var answerCount = Mathf.Clamp(model.answerCount > 0 ? model.answerCount : (model.answers?.Count ?? 4), MinAnswerCount, MaxAnswerCount);
        SetAnswersCount(answerCount);

        var inputs = AnswerInputs;
        var toggles = CorrectToggles;
        for (int i = 0; i < MaxAnswerCount; i++)
        {
            if (inputs[i] != null) inputs[i].text = model.answers != null && i < model.answers.Count ? model.answers[i] ?? "" : "";
            if (toggles[i] != null) toggles[i].isOn = model.correctAnswerIndexes != null && model.correctAnswerIndexes.Contains(i);
        }

        if (toggleRewardEnabled != null) toggleRewardEnabled.isOn = model.rewardEnabled;
        SetMaxStars(model.maxStars);
        if (inputTimeLimit != null) inputTimeLimit.text = model.timeLimitSeconds.ToString(CultureInfo.InvariantCulture);
        SetWorldEvent(model.worldEvent);
    }

    private void ClearFormFields(int nextTaskId)
    {
        if (inputTaskId != null)
        {
            inputTaskId.text = nextTaskId.ToString(CultureInfo.InvariantCulture);
            inputTaskId.interactable = false;
        }

        if (inputTaskTitle != null) inputTaskTitle.text = "";
        if (inputFirstNPCText != null) inputFirstNPCText.text = "";
        if (inputSecondNPCText != null) inputSecondNPCText.text = "";
        if (inputQuestionText != null) inputQuestionText.text = "";

        SetAnswersCount(MinAnswerCount);
        foreach (var input in AnswerInputs)
        {
            if (input != null) input.text = "";
        }

        foreach (var toggle in CorrectToggles)
        {
            if (toggle != null) toggle.isOn = false;
        }

        if (toggleRewardEnabled != null) toggleRewardEnabled.isOn = true;
        SetMaxStars(3);
        if (inputTimeLimit != null) inputTimeLimit.text = "60";
        SetWorldEvent(DefaultWorldEvent);
        UpdateAnswerRows(MinAnswerCount);
    }

    private void SetNpcSelection(Dropdown dropdown, List<string> guids, string guid)
    {
        if (dropdown == null || guids == null || guids.Count == 0) return;
        var idx = guids.IndexOf(guid);
        dropdown.value = idx >= 0 ? idx : 0;
        dropdown.RefreshShownValue();
    }

    private void SetAnswersCount(int count)
    {
        count = Mathf.Clamp(count, MinAnswerCount, MaxAnswerCount);
        if (dropdownAnswersCount != null)
        {
            dropdownAnswersCount.value = count - MinAnswerCount;
            dropdownAnswersCount.RefreshShownValue();
        }

        UpdateAnswerRows(count);
    }

    private int GetSelectedAnswerCount()
    {
        if (dropdownAnswersCount == null) return MinAnswerCount;
        return Mathf.Clamp(dropdownAnswersCount.value + MinAnswerCount, MinAnswerCount, MaxAnswerCount);
    }

    private void OnAnswersCountChanged(int _)
    {
        UpdateAnswerRows(GetSelectedAnswerCount());
    }

    private void UpdateAnswerRows(int answerCount)
    {
        var rows = AnswerRows;
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] != null) rows[i].SetActive(i < answerCount);
            if (i >= answerCount && CorrectToggles[i] != null) CorrectToggles[i].isOn = false;
        }
    }

    private void SetMaxStars(int maxStars)
    {
        maxStars = Mathf.Clamp(maxStars <= 0 ? 3 : maxStars, 1, 3);
        if (dropdownMaxStars != null)
        {
            dropdownMaxStars.value = maxStars - 1;
            dropdownMaxStars.RefreshShownValue();
        }
    }

    private int GetMaxStars()
    {
        return dropdownMaxStars == null ? 3 : Mathf.Clamp(dropdownMaxStars.value + 1, 1, 3);
    }

    private void SetWorldEvent(string worldEvent)
    {
        var idx = System.Array.IndexOf(WorldEventKeys, string.IsNullOrEmpty(worldEvent) ? DefaultWorldEvent : worldEvent);
        if (idx < 0) idx = 0;
        if (dropdownWorldEvent != null)
        {
            dropdownWorldEvent.value = idx;
            dropdownWorldEvent.RefreshShownValue();
        }
    }

    private string GetWorldEvent()
    {
        if (dropdownWorldEvent == null) return DefaultWorldEvent;
        var idx = Mathf.Clamp(dropdownWorldEvent.value, 0, WorldEventKeys.Length - 1);
        return WorldEventKeys[idx];
    }

    private string GetSelectedGuid(Dropdown dropdown, List<string> guids)
    {
        if (dropdown == null || guids == null || dropdown.value < 0 || dropdown.value >= guids.Count) return "";
        return guids[dropdown.value];
    }

    private void OnSaveTaskClicked()
    {
        coursesContainer = coursesContainer ?? DataManager.LoadCourses();
        allTasks = allTasks ?? DataManager.LoadTasks();

        if (!ValidateForm(out var validationError))
        {
            Debug.LogWarning("[TaskEditor] Validation error: " + validationError);
            return;
        }

        if (contextCourseId <= 0 && isEditing) contextCourseId = FindCourseIdForTask(editingTaskId);

        var course = coursesContainer?.courses?.Find(c => c.id == contextCourseId);
        if (course == null)
        {
            Debug.LogError($"[TaskEditor] Save failed: course not found for courseId={contextCourseId}");
            return;
        }

        var model = isEditing ? FindTaskForCourse(editingTaskId, contextCourseId) : null;
        if (model == null)
        {
            var nextId = isEditing && editingTaskId >= 0
                ? editingTaskId
                : DataManager.GetNextTaskIdForCourse(allTasks, contextCourseId, coursesContainer);

            model = new TaskModel { id = nextId, courseId = contextCourseId };
            allTasks.Add(model);
        }

        model.courseId = contextCourseId;
        ApplyFormToModel(model);

        if (course.taskIds == null) course.taskIds = new List<int>();
        if (!course.taskIds.Contains(model.id)) course.taskIds.Add(model.id);

        DataManager.SaveTasks(allTasks);
        DataManager.SaveCourses(coursesContainer);
        Debug.Log($"[TaskEditor] Task saved successfully: taskId={model.id}, courseId={course.id}, mode={(isEditing ? "edit" : "create")}");

        var savedCourseId = course.id;
        isEditing = false;
        editingTaskId = -1;
        contextCourseId = -1;
        gameObject.SetActive(false);
        OpenTasksPanel(savedCourseId);
    }

    private bool ValidateForm(out string error)
    {
        error = "";
        var answerCount = GetSelectedAnswerCount();

        if (inputTaskTitle == null || string.IsNullOrWhiteSpace(inputTaskTitle.text))
        {
            error = "Название задания не заполнено";
            return false;
        }

        if (inputQuestionText == null || string.IsNullOrWhiteSpace(inputQuestionText.text))
        {
            error = "Вопрос / условие задания не заполнено";
            return false;
        }

        var inputs = AnswerInputs;
        for (int i = 0; i < answerCount; i++)
        {
            if (inputs[i] == null || string.IsNullOrWhiteSpace(inputs[i].text))
            {
                error = $"Ответ {i + 1} не заполнен";
                return false;
            }
        }

        var toggles = CorrectToggles;
        var hasCorrect = false;
        for (int i = 0; i < answerCount; i++)
        {
            if (toggles[i] != null && toggles[i].isOn)
            {
                hasCorrect = true;
                break;
            }
        }

        if (!hasCorrect)
        {
            error = "Не выбран ни один правильный ответ";
            return false;
        }

        if (toggleRewardEnabled != null && toggleRewardEnabled.isOn)
        {
            if (inputTimeLimit == null || !float.TryParse(inputTimeLimit.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var timeLimit) || timeLimit <= 0f)
            {
                error = "Лимит времени должен быть числом больше 0";
                return false;
            }
        }

        return true;
    }

    private void ApplyFormToModel(TaskModel model)
    {
        var answerCount = GetSelectedAnswerCount();
        var inputs = AnswerInputs;
        var toggles = CorrectToggles;

        model.courseId = contextCourseId;
        model.title = inputTaskTitle != null ? inputTaskTitle.text.Trim() : "";
        model.giverNpcGuid = GetSelectedGuid(dropdownGiverNPC, giverOptionGuids);
        model.receiverNpcGuid = GetSelectedGuid(dropdownReceiverNPC, receiverOptionGuids);
        model.textForGiver = inputFirstNPCText != null ? inputFirstNPCText.text : "";
        model.textForReceiver = inputSecondNPCText != null ? inputSecondNPCText.text : "";
        model.questionText = inputQuestionText != null ? inputQuestionText.text : "";
        model.answerCount = answerCount;
        model.answers = new List<string>();
        model.correctAnswerIndexes = new List<int>();

        for (int i = 0; i < answerCount; i++)
        {
            model.answers.Add(inputs[i] != null ? inputs[i].text.Trim() : "");
            if (toggles[i] != null && toggles[i].isOn) model.correctAnswerIndexes.Add(i);
        }

        model.rewardEnabled = toggleRewardEnabled == null || toggleRewardEnabled.isOn;
        model.hasStars = model.rewardEnabled;
        model.maxStars = GetMaxStars();
        model.timeLimitSeconds = ParseTimeLimitOrDefault();
        model.worldEvent = GetWorldEvent();
    }

    private float ParseTimeLimitOrDefault()
    {
        if (inputTimeLimit != null && float.TryParse(inputTimeLimit.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value > 0f) return value;
        return 60f;
    }

    private int GetDisplayedTaskId()
    {
        if (inputTaskId != null && int.TryParse(inputTaskId.text, out var id)) return id;
        return -1;
    }

    private int FindCourseIdForTask(int taskId)
    {
        if (coursesContainer == null) coursesContainer = DataManager.LoadCourses();

        var taskWithCourse = allTasks?.FirstOrDefault(t => t != null && t.id == taskId && t.courseId > 0);
        if (taskWithCourse != null) return taskWithCourse.courseId;

        var course = coursesContainer?.courses?.FirstOrDefault(c => c.taskIds != null && c.taskIds.Contains(taskId));
        return course != null ? course.id : -1;
    }

    private TaskModel FindTaskForCourse(int taskId, int courseId)
    {
        if (allTasks == null) return null;

        var task = allTasks.FirstOrDefault(t => t != null && t.id == taskId && t.courseId == courseId);
        if (task != null) return task;

        return allTasks.FirstOrDefault(t => t != null && t.id == taskId && t.courseId <= 0);
    }


    private void OnClearFormClicked()
    {
        if (contextCourseId <= 0 && isEditing) contextCourseId = FindCourseIdForTask(editingTaskId);
        isEditing = false;
        editingTaskId = -1;
        allTasks = DataManager.LoadTasks();
        ClearFormFields(DataManager.GetNextTaskIdForCourse(allTasks, contextCourseId, coursesContainer));
        if (titleText != null) titleText.text = "Создание задания";
        Debug.Log($"[TaskEditor] Create form cleared for courseId={contextCourseId}");
    }

    private void OnBackClicked()
    {
        var courseId = contextCourseId;
        if (courseId <= 0 && isEditing) courseId = FindCourseIdForTask(editingTaskId);

        Debug.Log($"[TaskEditor] Back clicked without saving. courseId={courseId}");
        isEditing = false;
        editingTaskId = -1;
        contextCourseId = -1;
        gameObject.SetActive(false);
        OpenTasksPanel(courseId);
    }

    private void OpenTasksPanel(int courseId)
    {
        if (UIManager.Instance != null && courseId > 0)
        {
            UIManager.Instance.OpenTasksWindowForCourse(courseId);
        }
        else if (UIManager.Instance != null && UIManager.Instance.tasksPanel != null)
        {
            UIManager.Instance.ShowOnly(UIManager.Instance.tasksPanel);
        }
        else
        {
            Debug.LogWarning("[TaskEditor] Cannot open TasksPanel: UIManager or courseId is unavailable");
        }
    }

    private void DisableLegacyInputExpander()
    {
        if (inputFieldExpander == null) return;

        inputFieldExpander.gameObject.SetActive(false);
        inputFieldExpander = null;
    }
}