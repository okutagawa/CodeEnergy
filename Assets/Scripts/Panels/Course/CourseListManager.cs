using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MyGame.Models;
using MyGame.Data;

public class CourseListManager : MonoBehaviour
{
    [Header("Courses CRUD")]
    public RectTransform contentCourses;
    public GameObject prefabCourseItem;
    public InputField inputCourseName;
    public Button buttonAddCourse;
    public Button buttonDeleteSelected;
    public Button buttonEditSelected;
    public Button buttonExit;

    [Header("Admin JSON")]
    public Button buttonImportCourses;
    public Button buttonExportCourses;
    public Button buttonImportTasks;
    public Button buttonExportTasks;
    public Button buttonImportGameState;
    public Button buttonExportGameState;
    public Button buttonCreateBackup;
    public Button buttonRestoreBackup;
    public Button buttonOpenDataFolder;
    public Text operationStatusText;

    private CoursesContainer container;
    private readonly Dictionary<int, GameObject> instantiated = new Dictionary<int, GameObject>();
    private int selectedCourseId = -1;

    void Start()
    {
        SaveService.EnsureWorkingFiles();

        if (buttonAddCourse != null)
        {
            buttonAddCourse.onClick.RemoveAllListeners();
            buttonAddCourse.onClick.AddListener(OnAddCourseClicked);
        }
        if (buttonDeleteSelected != null) { buttonDeleteSelected.onClick.RemoveAllListeners(); buttonDeleteSelected.onClick.AddListener(DeleteSelectedCourse); }
        if (buttonEditSelected != null) { buttonEditSelected.onClick.RemoveAllListeners(); buttonEditSelected.onClick.AddListener(OnEditSelectedCourseClicked); }
        if (buttonExit != null)
        {
            buttonExit.onClick.RemoveAllListeners();
            buttonExit.onClick.AddListener(OnExitClicked);
        }

        BindAdminButtons();

        container = DataManager.LoadCourses();
        RefreshUI();
    }

    private void BindAdminButtons()
    {
        Bind(buttonImportCourses, () => OnImportJson(SaveService.CoursesFileName, SaveService.ValidateCoursesJson));
        Bind(buttonExportCourses, () => OnExportJson(SaveService.CoursesFileName));

        Bind(buttonImportTasks, () => OnImportJson(SaveService.TasksFileName, SaveService.ValidateTasksJson));
        Bind(buttonExportTasks, () => OnExportJson(SaveService.TasksFileName));

        Bind(buttonImportGameState, () => OnImportJson(SaveService.GameStateFileName, SaveService.ValidateGameStateJson));
        Bind(buttonExportGameState, () => OnExportJson(SaveService.GameStateFileName));

        Bind(buttonCreateBackup, OnCreateBackupClicked);
        Bind(buttonRestoreBackup, OnRestoreBackupClicked);
        Bind(buttonOpenDataFolder, OnOpenDataFolderClicked);
    }

    private static void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void OnImportJson(string fileName, System.Func<string, (bool ok, string error)> validator)
    {
        if (!EnsureAdmin()) return;

        var importPath = SaveService.GetTransferPath(fileName);
        if (!SaveService.ImportFile(importPath, fileName, validator, out var error))
        {
            SetStatus($"Не удалось импортировать {fileName}: {error}", true);
            return;
        }

        if (fileName == SaveService.CoursesFileName)
        {
            container = DataManager.LoadCourses();
            RefreshUI();
        }

        if (fileName == SaveService.GameStateFileName)
        {
            GameState.Instance?.LoadState();
        }

        SetStatus($"Imported {fileName} from: {importPath}");
        SaveService.OpenTransferFolder();
    }

    private void OnExportJson(string fileName)
    {
        if (!EnsureAdmin()) return;

        var exportPath = SaveService.GetTransferPath(fileName);
        if (!SaveService.ExportFile(fileName, exportPath, out var error))
        {
            SetStatus($"Не удалось экспортировать {fileName}: {error}", true);
            return;
        }

        SetStatus($"Exported {fileName} to: {exportPath}");
        SaveService.OpenTransferFolder();
    }

    private void OnCreateBackupClicked()
    {
        if (!EnsureAdmin()) return;

        var backupFolder = SaveService.CreateBackupBundle();
        SetStatus($"Backup created: {backupFolder}");
    }

    private void OnRestoreBackupClicked()
    {
        if (!EnsureAdmin()) return;

        if (!SaveService.RestoreLatestBackupBundle(out var restoredFrom, out var error))
        {
            SetStatus($"Не удалось восстановить резервную копию: {error}", true);
            return;
        }

        container = DataManager.LoadCourses();
        RefreshUI();
        GameState.Instance?.LoadState();

        SetStatus($"Restored backup from: {restoredFrom}");
    }

    private void OnOpenDataFolderClicked()
    {
        if (!EnsureAdmin()) return;

        SaveService.OpenDataFolder();
        SetStatus($"Opened data folder: {SaveService.SaveFolder}");
    }

    private bool EnsureAdmin()
    {
        GameState.EnsureExists();
        if (GameState.Instance != null && GameState.Instance.IsAdminMode) return true;
        SetStatus("Эта операция доступна только администратору.", true);
        return false;
    }

    private void SetStatus(string text, bool isError = false)
    {
        Debug.Log(isError ? "[Admin JSON] " + text : "[Admin JSON] " + text);
        if (isError)
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowError(UserErrorMessages.FromValidation(text));
            else ErrorPopupController.Show(UserErrorMessages.FromValidation(text));
        }
        if (operationStatusText != null)
        {
            operationStatusText.text = text;
            operationStatusText.color = isError ? Color.red : Color.white;
        }
    }

    void OnAddCourseClicked()
    {
        if (inputCourseName == null)
        {
            SetStatus("Поле названия курса не настроено.", true);
            return;
        }

        var title = inputCourseName.text.Trim();
        if (string.IsNullOrEmpty(title))
        {
            SetStatus("Введите название курса.", true);
            return;
        }

        if (container == null) container = DataManager.LoadCourses();
        if (container.courses == null) container.courses = new List<CourseModel>();

        var model = new CourseModel { id = DataManager.NextCourseId(container), name = title };
        container.courses.Add(model);
        AddCourseToUI(model);
        inputCourseName.text = "";
        DataManager.SaveCourses(container);
    }

    void AddCourseToUI(CourseModel c)
    {
        if (c == null) return;
        if (prefabCourseItem == null || contentCourses == null)
        {
            SetStatus("Course list Content or Prefab is not assigned.", true);
            Debug.LogError("CourseListManager.AddCourseToUI: prefabCourseItem/contentCourses is not assigned");
            return;
        }
        var go = Instantiate(prefabCourseItem, contentCourses);
        var item = go.GetComponent<CourseItem>();
        if (item == null)
        {
            SetStatus("Course prefab is invalid: CourseItem component is missing.", true);
            Debug.LogError("CourseListManager: prefabCourseItem missing CourseItem component");
            Destroy(go);
            return;
        }
        item.Initialize(c);
        item.onSingleClick = OnCourseSingleClick;
        item.onDoubleClick = OnCourseDoubleClick;
        instantiated[c.id] = go;
        item.SetSelected(c.id == selectedCourseId);

        // force layout rebuild so ScrollRect updates immediately
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentCourses);
    }

    void OnCourseSingleClick(CourseModel c) => SelectCourse(c.id);

    void OnCourseDoubleClick(CourseModel c)
    {
        
        UIManager.Instance?.OpenTasksWindowForCourse(c.id);
    }

    public void SelectCourse(int courseId)
    {
        if (selectedCourseId == courseId) return;
        if (instantiated.TryGetValue(selectedCourseId, out var prevGo))
        {
            var prevItem = prevGo.GetComponent<CourseItem>();
            prevItem?.SetSelected(false);
        }
        selectedCourseId = courseId;
        if (instantiated.TryGetValue(selectedCourseId, out var curGo))
        {
            var curItem = curGo.GetComponent<CourseItem>();
            curItem?.SetSelected(true);
        }
    }

    private void OnEditSelectedCourseClicked()
    {
        if (selectedCourseId < 0)
        {
            SetStatus("Select a course to edit.", true);
            return;
        }

        if (inputCourseName == null)
        {
            SetStatus("Course name input is not assigned.", true);
            return;
        }

        if (container == null) container = DataManager.LoadCourses();
        if (container.courses == null) container.courses = new List<CourseModel>();

        var model = container.courses.Find(x => x != null && x.id == selectedCourseId);
        if (model == null)
        {
            SetStatus("Selected course was not found.", true);
            return;
        }

        var newTitle = inputCourseName.text.Trim();
        if (string.IsNullOrEmpty(newTitle))
        {
            inputCourseName.text = model.name ?? string.Empty;
            SetStatus("Enter a new course name and press edit again.", true);
            return;
        }

        model.name = newTitle;
        DataManager.SaveCourses(container);
        RefreshUI();
        SelectCourse(model.id);
        SetStatus($"Course renamed: {model.name}");
    }

    public void DeleteSelectedCourse()
    {
        if (selectedCourseId < 0)
        {
            SetStatus("Выберите курс для удаления.", true);
            return;
        }
        if (selectedCourseId < 0) return;
        if (container == null) container = DataManager.LoadCourses();
        if (container.courses == null) container.courses = new List<CourseModel>();

        var model = container.courses.Find(x => x != null && x.id == selectedCourseId);
        if (model == null)
        {
            SetStatus("Выбранный курс не найден.", true);
            return;
        }

        if (instantiated.TryGetValue(selectedCourseId, out var go))
        {
            Destroy(go);
            instantiated.Remove(selectedCourseId);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentCourses);
        }

        container.courses.RemoveAll(x => x.id == selectedCourseId);
        DataManager.SaveCourses(container);
        selectedCourseId = -1;
    }

    // made public so UIManager can call RefreshUI safely
    public void RefreshUI()
    {
        foreach (var kv in instantiated.Values) Destroy(kv);
        instantiated.Clear();
        container = container ?? DataManager.LoadCourses();
        if (container.courses == null) container.courses = new List<CourseModel>();
        foreach (var c in container.courses) AddCourseToUI(c);
    }

    private void OnExitClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ReturnToMainMenu();
        }
        else
        {
            Debug.LogWarning("CourseListManager: UIManager.Instance is null when trying to return to main menu");
        }
    }
}
