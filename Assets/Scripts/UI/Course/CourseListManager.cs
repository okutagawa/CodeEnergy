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
        if (buttonDeleteSelected != null) buttonDeleteSelected.onClick.AddListener(DeleteSelectedCourse);
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
            SetStatus($"Import failed for {fileName}: {error}", true);
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
    }

    private void OnExportJson(string fileName)
    {
        if (!EnsureAdmin()) return;

        var exportPath = SaveService.GetTransferPath(fileName);
        if (!SaveService.ExportFile(fileName, exportPath, out var error))
        {
            SetStatus($"Export failed for {fileName}: {error}", true);
            return;
        }

        SetStatus($"Exported {fileName} to: {exportPath}");
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
            SetStatus($"Restore failed: {error}", true);
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
        if (GameState.Instance != null && GameState.Instance.IsAdminMode) return true;
        SetStatus("Operation is available only in admin mode.", true);
        return false;
    }

    private void SetStatus(string text, bool isError = false)
    {
        Debug.Log(isError ? "[Admin JSON] " + text : "[Admin JSON] " + text);
        if (operationStatusText != null)
        {
            operationStatusText.text = text;
            operationStatusText.color = isError ? Color.red : Color.white;
        }
    }

    void OnAddCourseClicked()
    {
        var title = inputCourseName.text.Trim();       
        if (string.IsNullOrEmpty(title)) return;

        var model = new CourseModel { id = DataManager.NextCourseId(container), name = title };
        container.courses.Add(model);
        AddCourseToUI(model);
        inputCourseName.text = "";
        DataManager.SaveCourses(container);
    }

    void AddCourseToUI(CourseModel c)
    {
        var go = Instantiate(prefabCourseItem, contentCourses);
        var item = go.GetComponent<CourseItem>();
        if (item == null) Debug.LogError("CourseListManager: prefabCourseItem missing CourseItem component");
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

    public void DeleteSelectedCourse()
    {
        if (selectedCourseId < 0) return;
        var model = container.courses.Find(x => x.id == selectedCourseId);
        if (model == null) return;

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
