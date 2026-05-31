using System.Collections.Generic;
using MyGame.Data;
using MyGame.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CourseSelectPanelController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuRoot;
    public GameObject courseSelectPanel;

    [Header("Scroll View")]
    public RectTransform contentCourses;
    public GameObject courseSelectItemPrefab;

    [Header("Buttons")]
    public Button buttonExit;

    [Header("Texts")]
    public Text descriptionText;
    public Text statusText;

    [Header("Scene Settings")]
    public string gameSceneName = "GameScene";

    [Header("Messages")]
    [TextArea(2, 4)]
    public string defaultDescription = "Выберите курс из списка, который будет загружен для новой игры.";

    [TextArea(2, 4)]
    public string emptyCoursesMessage = "Курсы не найдены. Создайте курс в админ-панели.";

    private CoursesContainer coursesContainer;
    private CourseModel selectedCourse;
    private bool isStartingGame;
    private readonly List<CourseSelectItem> spawnedItems = new List<CourseSelectItem>();

    private void Awake()
    {
        BindButton(buttonExit, ClosePanel);
        SetDescription(defaultDescription);
    }

    private void OnEnable()
    {
        RefreshCourses();
    }

    public void OpenPanel()
    {
        isStartingGame = false;

        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(false);
        }

        var panel = courseSelectPanel != null ? courseSelectPanel : gameObject;
        bool wasActive = panel.activeSelf;
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        if (wasActive)
        {
            RefreshCourses();
        }
    }

    public void ClosePanel()
    {
        var panel = courseSelectPanel != null ? courseSelectPanel : gameObject;
        panel.SetActive(false);

        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(true);
        }
    }

    public void RefreshCourses()
    {
        ClearCourseItems();
        selectedCourse = null;
        isStartingGame = false;
        SetDescription(defaultDescription);

        SaveService.EnsureWorkingFiles();

        coursesContainer = DataManager.LoadCourses();

        if (coursesContainer == null || coursesContainer.courses == null || coursesContainer.courses.Count == 0)
        {
            SetStatus(emptyCoursesMessage, true);
            return;
        }

        foreach (CourseModel course in coursesContainer.courses)
        {
            if (course == null) continue;
            CreateCourseItem(course);
        }

        SetStatus("Выберите курс двойным нажатием.", false);
    }

    private void CreateCourseItem(CourseModel course)
    {
        if (contentCourses == null)
        {
            Debug.LogError("[CourseSelectPanelController] Content для списка курсов не назначен.");
            return;
        }

        if (courseSelectItemPrefab == null)
        {
            Debug.LogError("[CourseSelectPanelController] Префаб CourseSelectItem не назначен.");
            return;
        }

        GameObject itemObject = Instantiate(courseSelectItemPrefab, contentCourses);
        CourseSelectItem item = itemObject.GetComponent<CourseSelectItem>();

        if (item == null)
        {
            Debug.LogError("[CourseSelectPanelController] На префабе отсутствует компонент CourseSelectItem.");
            Destroy(itemObject);
            return;
        }

        item.Initialize(course, OnCourseSingleClicked, OnCourseDoubleClicked);
        spawnedItems.Add(item);

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentCourses);
    }

    private void OnCourseSingleClicked(CourseModel course)
    {
        if (course == null || isStartingGame) return;

        SelectCourse(course);
        SetStatus("Выбран курс: " + GetCourseName(selectedCourse) + ". Нажмите ещё раз, чтобы начать.", false);
    }

    private void OnCourseDoubleClicked(CourseModel course)
    {
        if (course == null)
        {
            SetStatus("Не удалось выбрать курс.", true);
            return;
        }

        if (isStartingGame) return;

        SelectCourse(course);
        StartNewGameWithCourse(selectedCourse);
    }

    private void SelectCourse(CourseModel course)
    {
        selectedCourse = course;

        foreach (CourseSelectItem item in spawnedItems)
        {
            if (item == null) continue;
            item.SetSelected(item.GetCourseId() == selectedCourse.id);
        }
    }

    private void StartNewGameWithCourse(CourseModel course)
    {
        if (course == null)
        {
            SetStatus("Сначала выберите курс.", true);
            return;
        }

        isStartingGame = true;
        SetItemsInteractable(false);
        SetStatus("Запускаем курс: " + GetCourseName(course) + "...", false);

        SaveManager.Delete();

        GameState.EnsureExists();

        GameStateData newGameData = new GameStateData
        {
            selectedCourseId = course.id
        };

        if (GameState.Instance != null)
        {
            GameState.Instance.ApplyData(newGameData);
            GameState.Instance.IsAdminMode = false;
            GameState.Instance.SaveState();
        }
        else
        {
            SaveManager.Save(newGameData);
        }

        Debug.Log("[CourseSelectPanelController] Started new game with course id: " + course.id + ", name: " + course.name);

        SceneManager.LoadScene(gameSceneName);
    }

    private void SetItemsInteractable(bool interactable)
    {
        foreach (CourseSelectItem item in spawnedItems)
        {
            if (item == null) continue;
            item.SetInteractable(interactable);
        }
    }

    private void ClearCourseItems()
    {
        if (contentCourses != null)
        {
            for (int i = contentCourses.childCount - 1; i >= 0; i--)
            {
                Destroy(contentCourses.GetChild(i).gameObject);
            }
        }

        spawnedItems.Clear();
    }

    private void SetDescription(string message)
    {
        if (descriptionText != null)
        {
            descriptionText.text = message;
        }
    }

    private void SetStatus(string message, bool isError)
    {
        if (isError)
        {
            Debug.LogWarning("[CourseSelectPanelController] " + message);
        }
        else
        {
            Debug.Log("[CourseSelectPanelController] " + message);
        }

        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = isError ? Color.red : Color.white;
        }
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static string GetCourseName(CourseModel course)
    {
        return course != null && !string.IsNullOrWhiteSpace(course.name)
            ? course.name
            : "Без названия";
    }
}