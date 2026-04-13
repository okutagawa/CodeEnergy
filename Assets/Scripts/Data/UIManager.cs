using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main panels (assign in inspector)")]
    public GameObject mainMenuRoot;
    public GameObject adminPasswordPanel;
    public GameObject settingsPanel;
    public GameObject coursesPanel;
    public GameObject tasksPanel;

    [Header("Optional / child panels")]
    public GameObject taskEditorPanel; 
    public GameObject editCoursePanel;

    // внутренние кэши для быстрого доступа к контроллерам
    private CourseListManager _courseListManager;
    private TasksListManager _tasksListManager;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log("persistentDataPath = " + Application.persistentDataPath);
        CacheManagers();
    }

    // Попытаться получить ссылки на менеджеры (если объекты уже в сцене)
    public void CacheManagers()
    {
        if (coursesPanel != null)
        {
            _courseListManager = coursesPanel.GetComponentInChildren<CourseListManager>(true);
            // Если CourseListManager не найден напрямую в children, попробуйте FindObjectOfType
            if (_courseListManager == null) _courseListManager = FindObjectOfType<CourseListManager>();
        }

        if (tasksPanel != null)
        {
            _tasksListManager = tasksPanel.GetComponentInChildren<TasksListManager>(true);
            if (_tasksListManager == null) _tasksListManager = FindObjectOfType<TasksListManager>();
        }
    }

    // Показываем только одну панель, скрывая остальные (без уничтожения)
    public void ShowOnly(GameObject panel)
    {
        // Список всех известных панелей — расширяйте по необходимости
        var all = new GameObject[]
        {
            mainMenuRoot,
            adminPasswordPanel,
            settingsPanel,
            coursesPanel,
            tasksPanel,
            taskEditorPanel,
            editCoursePanel
        };

        foreach (var p in all)
        {
            if (p == null) continue;
            p.SetActive(p == panel);
        }

        // После переключения иногда нужно обновить кеш менеджеров, если панели создаются динамически
        CacheManagers();
    }

    // Простые обёртки для удобства вызова из других контроллеров
    public void ShowMainMenu()
    {
        ShowOnly(mainMenuRoot);
    }

    public void ShowAdminPassword()
    {
        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(true);

        if (adminPasswordPanel != null)
        {
            adminPasswordPanel.SetActive(true);
            adminPasswordPanel.transform.SetAsLastSibling();
        }
    }

    public void ShowSettingsPanel()
    {
        if (settingsPanel != null)
        {
            if (mainMenuRoot != null)
                mainMenuRoot.SetActive(true);

            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
            return;
        }

        var settingsController = FindObjectOfType<SettingsController>(true);
        if (settingsController != null)
        {
            settingsController.OpenSettings();
            return;
        }

        Debug.LogWarning("UIManager: settingsPanel is not assigned and SettingsController was not found.");
    }

    public void HideAdminPassword()
    {
        if (adminPasswordPanel != null)
            adminPasswordPanel.SetActive(false);
    }

    public void HideSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ShowCoursesPanel()
    {
        ShowOnly(coursesPanel);
        // Обновить список курсов при показе
        if (_courseListManager == null) CacheManagers();
        _courseListManager?.RefreshUI();
    }

    // Основной метод для перехода к TasksPanel и передачи контекста courseId
    public void OpenTasksWindowForCourse(int courseId)
    {
        if (tasksPanel == null)
        {
            Debug.LogError("UIManager: tasksPanel is not assigned in inspector");
            return;
        }

        ShowOnly(tasksPanel);

        // Попытка получить TasksListManager и передать ему context
        if (_tasksListManager == null) CacheManagers();

        if (_tasksListManager != null)
        {
            _tasksListManager.OpenForCourse(courseId);
            return;
        }

        var found = FindObjectOfType<TasksListManager>();
        if (found != null)
        {
            _tasksListManager = found;
            _tasksListManager.OpenForCourse(courseId);
        }
        else
        {
            Debug.LogError("UIManager: TasksListManager not found in scene");
        }
    }

    // Удобный метод: вызов при успешной авторизации администратора
    public void OnAdminAuthenticated()
    {
        // Показываем панель курсов по умолчанию
        ShowCoursesPanel();
    }

    // Возврат к выбору ролей (для крестика/Exit)
    public void ReturnToMainMenu()
    {
        ShowOnly(mainMenuRoot);
    }

    // Простая утилита для безопасного обращения к CourseListManager (если метод RefreshUI доступен)
    public CourseListManager GetCourseListManager()
    {
        if (_courseListManager == null) CacheManagers();
        return _courseListManager;
    }

    public TasksListManager GetTasksListManager()
    {
        if (_tasksListManager == null) CacheManagers();
        return _tasksListManager;
    }

    public TaskEditorController GetTaskEditorController()
    {
        // Попробуем получить контроллер из кэшированных ссылок
        if (taskEditorPanel != null)
        {
            var ctrl = taskEditorPanel.GetComponentInChildren<TaskEditorController>(true);
            if (ctrl != null) return ctrl;
        }

        // Попробуем найти в сцене (активные объекты)
        var found = FindObjectOfType<TaskEditorController>();
        if (found != null) return found;

        // И последний вариант — поиск среди всех GameObject (включая неактивные)
        var all = Resources.FindObjectsOfTypeAll<TaskEditorController>();
        if (all != null && all.Length > 0) return all[0];

        return null;
    }

}
