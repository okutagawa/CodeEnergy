using UnityEngine;

/// <summary>
/// ÷ентрализованное управление видимостью панелей UI в MenuScene.
/// ѕрив€жи в инспекторе все панели (GameObject): roleSelectionPanel, adminPasswordPanel,
/// coursesPanel, tasksPanel, taskEditorPanel, placeholderPanel, tasksSelectionPanel (если есть).
/// UIManager.Instance.ShowOnly(panel) скрывает все и показывает указанную панель.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels (assign in Inspector)")]
    public GameObject roleSelectionPanel;
    public GameObject adminPasswordPanel;
    public GameObject coursesPanel;
    public GameObject tasksPanel;             // дл€ переcмотра/CRUD задач
    public GameObject tasksSelectionPanel;    // дл€ выбора задач (чекбоксы)
    public GameObject taskEditorPanel;        // панель редактировани€ одного задани€
    public GameObject placeholderPanel;       // простое модальное окно дл€ сообщений

    void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // ѕо умолчанию показать выбор роли
        ShowOnly(roleSelectionPanel);
    }

    /// <summary>
    /// —крывает все панели и показывает указанную (если panel == null - скрывает все).
    /// </summary>
    public void ShowOnly(GameObject panel)
    {
        // —писок всех известных панелей
        var panels = new GameObject[]
        {
            roleSelectionPanel,
            adminPasswordPanel,
            coursesPanel,
            tasksPanel,
            tasksSelectionPanel,
            taskEditorPanel,
            placeholderPanel
        };

        foreach (var p in panels)
        {
            if (p == null) continue;
            p.SetActive(p == panel);
        }
    }

    /// <summary>
    /// »спользуетс€ после успешного входа администратора.
    /// </summary>
    public void ShowCoursesForAdmin()
    {
        if (coursesPanel == null)
        {
            Debug.LogWarning("UIManager: coursesPanel not assigned");
            return;
        }

        // ѕоказать панель курсов
        ShowOnly(coursesPanel);

        // ѕопросить CoursesController обновить список (если есть)
        //var ctrl = coursesPanel.GetComponentInChildren<CourseListManager>();
        //if (ctrl != null) ctrl.LoadCourses();
    }

    /// <summary>
    /// ќткрывает TasksSelectionPanel (режим выбора задач дл€ переданного курса).
    /// ќжидаетс€, что TasksController или соответствующий контроллер находитс€ на tasksSelectionPanel.
    /// </summary>
    //public void ShowTasksForCourse(int courseId)
    //{
    //    if (tasksSelectionPanel == null && tasksPanel == null)
    //    {
    //        Debug.LogWarning("UIManager: tasksSelectionPanel and tasksPanel not assigned");
    //        return;
    //    }

    //    // ≈сли есть €вна€ панель выбора задач Ч открываем еЄ,
    //    // иначе используем обычную tasksPanel в режиме просмотра/редактировани€.
    //    var targetPanel = tasksSelectionPanel != null ? tasksSelectionPanel : tasksPanel;
    //    ShowOnly(targetPanel);

    //    // ѕередать курс контроллеру на панели
    //    var tasksCtrl = targetPanel.GetComponentInChildren<TasksController>();
    //    if (tasksCtrl != null)
    //    {
    //        // ќткрытие в режиме выбора (реализовать в TasksController.OpenSelectionMode / OpenCourse)
    //        tasksCtrl.OpenSelectionMode(courseId);
    //    }
    //}

    /// <summary>
    /// ќткрыть TasksPanel (обычный просмотр/редактирование задач конкретного курса).
    /// </summary>
    //public void ShowTasksPanelForCourse(int courseId)
    //{
    //    if (tasksPanel == null)
    //    {
    //        Debug.LogWarning("UIManager: tasksPanel not assigned");
    //        return;
    //    }

    //    ShowOnly(tasksPanel);
    //    var tasksCtrl = tasksPanel.GetComponentInChildren<TasksController>();
    //    if (tasksCtrl != null) tasksCtrl.OpenCourse(courseId);
    //}

    /// <summary>
    /// ќткрыть панель редактировани€ конкретного задани€.
    /// </summary>
    //public void ShowTaskEditor(int courseId, int? taskId = null)
    //{
    //    if (taskEditorPanel == null)
    //    {
    //        Debug.LogWarning("UIManager: taskEditorPanel not assigned");
    //        return;
    //    }

    //    ShowOnly(taskEditorPanel);
    //    var editor = taskEditorPanel.GetComponentInChildren<TaskEditorController>();
    //    if (editor != null) editor.OpenTask(courseId, taskId);
    //}

    /// <summary>
    /// ѕоказать простой placeholder/modal с сообщением. ќжидаетс€, что PlaceholderPanel имеет компонент PlaceholderPanel.
    /// </summary>
    //public void ShowPlaceholder(string message)
    //{
    //    if (placeholderPanel == null)
    //    {
    //        Debug.LogWarning("UIManager: placeholderPanel not assigned");
    //        return;
    //    }
    //    ShowOnly(placeholderPanel);
    //    var ph = placeholderPanel.GetComponentInChildren<PlaceholderPanel>();
    //    if (ph != null) ph.Show(message);
    //}

    /// <summary>
    /// ”тилита: скрыть любое открытое UI (показать пустой экран).
    /// </summary>
    public void HideAll()
    {
        ShowOnly(null);
    }
}
