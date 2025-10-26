using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject roleSelectionPanel;
    public GameObject adminPasswordPanel; // password UI can be separate
    public GameObject profilePanel;
    public GameObject coursesPanel;
    public GameObject lessonsPanel;
    public GameObject adminPanel;
    public GameObject placeholderPanel; // simple text panel for quiz placeholder

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
    public void ShowAdminPanel() { GameState.Instance.IsAdminMode = true; ShowOnly(adminPanel); }
    public void ShowCoursesList() { GameState.Instance.IsAdminMode = false; ShowOnly(coursesPanel); }
    public void ShowLessonsForCourse(Course course)
    {
        ShowOnly(lessonsPanel);
        var controller = lessonsPanel.GetComponent<LessonsController>();
        controller?.SetCourse(course);
    }
    public void ShowPlaceholder(string text)
    {
        ShowOnly(placeholderPanel);
        var ph = placeholderPanel.GetComponent<PlaceholderPanel>();
        ph?.SetText(text);
    }
}
