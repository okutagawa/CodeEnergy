using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using MyGame.Models;
using MyGame.Data;

public class CourseListManager : MonoBehaviour
{
    public RectTransform contentCourses;
    public GameObject prefabCourseItem;
    public InputField inputCourseName;
    public Button buttonAddCourse;
    public Button buttonDeleteSelected;
    public Button buttonEditSelected;
    public Button buttonExit;

    private CoursesContainer container;
    private Dictionary<int, GameObject> instantiated = new Dictionary<int, GameObject>();
    private int selectedCourseId = -1;

    void Start()
    {
        if (buttonAddCourse != null) { buttonAddCourse.onClick.RemoveAllListeners(); buttonAddCourse.onClick.AddListener(OnAddCourseClicked); }
        if (buttonDeleteSelected != null) buttonDeleteSelected.onClick.AddListener(DeleteSelectedCourse);
        if (buttonExit != null) { buttonExit.onClick.RemoveAllListeners(); buttonExit.onClick.AddListener(OnExitClicked); }

        container = DataManager.LoadCourses();
        RefreshUI();
    }

    void OnAddCourseClicked()
    {
        var title = inputCourseName.text.Trim();
        Debug.Log("CourseListManager: Adding course with title: '" + title + "'");
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
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentCourses);
    }

    void OnCourseSingleClick(CourseModel c) => SelectCourse(c.id);

    void OnCourseDoubleClick(CourseModel c)
    {
        Debug.Log("CourseListManager: double click course id=" + c.id + " name='" + c.name + "'");
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
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentCourses);
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
            UIManager.Instance.ReturnToRoleSelection();
        }
        else
        {
            Debug.LogWarning("CourseListManager: UIManager.Instance is null when trying to return to role selection");
        }
    }
}
