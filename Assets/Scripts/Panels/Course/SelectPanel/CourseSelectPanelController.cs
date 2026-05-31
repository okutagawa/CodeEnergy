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
    private readonly List<CourseSelectItem> spawnedItems = new List<CourseSelectItem>();

    private void Awake()
    {
        if (buttonExit != null)
        {
            buttonExit.onClick.RemoveAllListeners();
            buttonExit.onClick.AddListener(ClosePanel);
        }

        if (descriptionText != null)
        {
            descriptionText.text = defaultDescription;
        }
    }

    private void OnEnable()
    {
        RefreshCourses();
    }

    public void OpenPanel()
    {
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(false);
        }

        if (courseSelectPanel != null)
        {
            courseSelectPanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        RefreshCourses();
    }

    public void ClosePanel()
    {
        if (courseSelectPanel != null)
        {
            courseSelectPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(true);
        }
    }

    public void RefreshCourses()
    {
        ClearCourseItems();

        SaveService.EnsureWorkingFiles();

        coursesContainer = DataManager.LoadCourses();
        selectedCourse = null;

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
        if (course == null) return;

        selectedCourse = course;

        foreach (CourseSelectItem item in spawnedItems)
        {
            if (item == null) continue;
            item.SetSelected(item.GetCourseId() == selectedCourse.id);
        }

        SetStatus("Выбран курс: " + selectedCourse.name + ". Нажмите дважды, чтобы начать.", false);
    }

    private void OnCourseDoubleClicked(CourseModel course)
    {
        if (course == null)
        {
            SetStatus("Не удалось выбрать курс.", true);
            return;
        }

        selectedCourse = course;

        foreach (CourseSelectItem item in spawnedItems)
        {
            if (item == null) continue;
            item.SetSelected(item.GetCourseId() == selectedCourse.id);
        }

        StartNewGameWithCourse(selectedCourse);
    }

    private void StartNewGameWithCourse(CourseModel course)
    {
        if (course == null)
        {
            SetStatus("Сначала выберите курс.", true);
            return;
        }

        GameState.EnsureExists();

        GameStateData newGameData = new GameStateData();
        newGameData.selectedCourseId = course.id;

        if (GameState.Instance != null)
        {
            GameState.Instance.ApplyData(newGameData);
            GameState.Instance.SaveState();
        }
        else
        {
            SaveManager.Save(newGameData);
        }

        Debug.Log("[CourseSelectPanelController] Started new game with course id: " + course.id + ", name: " + course.name);

        SceneManager.LoadScene(gameSceneName);
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

    private void SetStatus(string message, bool isError)
    {
        Debug.Log(isError
            ? "[CourseSelectPanelController] " + message
            : "[CourseSelectPanelController] " + message);

        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = isError ? Color.red : Color.white;
        }
    }
}