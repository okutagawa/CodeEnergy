using UnityEngine;

// координатор видимости панелей UI и шлюз для показа основных экранов (меню, курсы, уроки, админ и плейсхолдер)
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject roleSelectionPanel;
    public GameObject adminPasswordPanel; 
    public GameObject profilePanel;
    public GameObject coursesPanel;
    public GameObject lessonsPanel;
    public GameObject adminPanel;
    public GameObject placeholderPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        ShowOnly(roleSelectionPanel);
    }

    public void ShowOnly(GameObject panel)
    {
        roleSelectionPanel?.SetActive(false);
        adminPasswordPanel?.SetActive(false);
        profilePanel?.SetActive(false);
        coursesPanel?.SetActive(false);
        lessonsPanel?.SetActive(false);
        adminPanel?.SetActive(false);
        placeholderPanel?.SetActive(false);
        panel?.SetActive(true);
    }

    public void ShowProfileSelection() { ShowOnly(profilePanel); }

    public void ShowAdminPanel()
    {
        if (GameState.Instance != null) GameState.Instance.IsAdminMode = true;
        ShowOnly(adminPanel);
    }

    public void ShowCoursesList()
    {
        if (GameState.Instance != null) GameState.Instance.IsAdminMode = false;
        ShowOnly(coursesPanel);
    }

    public void ShowLessonsForCourse(Course course)
    {
        ShowOnly(lessonsPanel);
        var controller = lessonsPanel.GetComponent<LessonsController>();
        controller?.OpenCourse(course);
    }

    public void ShowPlaceholder(string text)
    {
        ShowOnly(placeholderPanel);
        var ph = placeholderPanel.GetComponent<PlaceholderPanel>();
        ph?.Show(text);
    }
}
