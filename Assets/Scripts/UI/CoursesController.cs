using UnityEngine;
using UnityEngine.UI;

//  онтроллер списка курсов Ч подгружает все курсы, создаЄт CourseButtonPrefab дл€ каждого, обрабатывает добавление курса и переходы.
public class CoursesController : MonoBehaviour
{
    public Transform coursesListContent; // Content внутри Scroll View
    public GameObject courseButtonPrefab;
    public InputField newCourseName; 
    public Button addCourseBtn; 
    public Button backButton; 

    private CoursesContainer coursesData;

    void Start()
    {
        coursesData = DataManager.LoadCourses();
        if (addCourseBtn != null) addCourseBtn.onClick.AddListener(OnAddCourse);
        if (backButton != null) backButton.onClick.AddListener(() => UIManager.Instance.ShowProfileSelection());
        RefreshList();
    }

    public void RefreshList()
    {
        if (coursesListContent == null || courseButtonPrefab == null) return;

        // ќчистка
        foreach (Transform t in coursesListContent) Destroy(t.gameObject);

        // —оздание элементов
        foreach (var course in coursesData.courses)
        {
            var go = Instantiate(courseButtonPrefab, coursesListContent);
            var binder = go.GetComponent<CourseButtonBinder>();
            if (binder != null) binder.Init(course, this);
            else
            {
                var txt = go.GetComponentInChildren<Text>();
                if (txt != null) txt.text = course.name;
                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    var captured = course;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnOpenCourse(captured));
                }
            }
        }
    }

    // ѕубличный метод дл€ открыти€ курса Ч вызываетс€ из CourseButtonBinder
    public void OnOpenCourse(Course course)
    {
        if (course == null) return;
        UIManager.Instance.ShowLessonsForCourse(course);
    }

    void OnAddCourse()
    {
        if (newCourseName == null) return;
        var name = newCourseName.text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        var c = new Course { id = DataManager.NextCourseId(coursesData), name = name };
        coursesData.courses.Add(c);
        DataManager.SaveCourses(coursesData);
        newCourseName.text = "";
        RefreshList();
    }

    // ѕубличные методы редактировани€/удалени€ Ч вызываютс€ из CourseButtonBinder
    public void OnEditCourseClicked(Course course)
    {
        if (course == null) return;
        UIManager.Instance.ShowAdminPanel();
        var admin = UIManager.Instance.adminPanel != null ? UIManager.Instance.adminPanel.GetComponent<AdminController>() : null;
        if (admin != null) admin.OpenCourseEditor(course);
    }

    public void OnDeleteCourseClicked(Course course)
    {
        if (course == null) return;
        // ѕростой удал€ющий вариант: удал€ем и сохран€ем
        coursesData.courses.RemoveAll(x => x.id == course.id);
        DataManager.SaveCourses(coursesData);
        RefreshList();
    }
}
