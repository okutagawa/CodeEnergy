using UnityEngine;
using UnityEngine.UI;

public class CoursesController : MonoBehaviour
{
    public Transform coursesListContent;
    public GameObject courseButtonPrefab;
    private CoursesContainer coursesData;

    void Start()
    {
        coursesData = DataManager.LoadCourses();
        RefreshList();
    }

    public void RefreshList()
    {
        foreach (Transform t in coursesListContent) Destroy(t.gameObject);
        foreach (var course in coursesData.courses)
        {
            var go = Instantiate(courseButtonPrefab, coursesListContent);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<Text>();
            txt.text = course.name;
            btn.onClick.AddListener(() => OnOpenCourse(course));
            // If admin, additional UI elements (edit/delete) can be shown in prefab and wired separately.
        }
    }

    void OnOpenCourse(Course course)
    {
        UIManager.Instance.ShowLessonsForCourse(course);
    }
}