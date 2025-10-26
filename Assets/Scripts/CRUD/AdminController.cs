using UnityEngine;
using UnityEngine.UI;

public class AdminController : MonoBehaviour
{
    public InputField newCourseName;
    public Button addCourseBtn;
    public CoursesController coursesController;
    private CoursesContainer coursesData;

    void Start()
    {
        coursesData = DataManager.LoadCourses();
        addCourseBtn.onClick.AddListener(OnAddCourse);
    }

    void OnAddCourse()
    {
        string name = newCourseName.text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        var c = new Course
        {
            id = DataManager.NextCourseId(coursesData),
            name = name
        };
        coursesData.courses.Add(c);
        DataManager.SaveCourses(coursesData);
        newCourseName.text = "";
        coursesController.RefreshList();
    }

    public void DeleteCourse(Course course)
    {
        coursesData.courses.RemoveAll(x => x.id == course.id);
        DataManager.SaveCourses(coursesData);
        coursesController.RefreshList();
    }

    public void EditCourseName(Course course, string newName)
    {
        var c = coursesData.courses.Find(x => x.id == course.id);
        if (c == null) return;
        c.name = newName;
        DataManager.SaveCourses(coursesData);
        coursesController.RefreshList();
    }

    public void AddLesson(Course course, string lessonTitle)
    {
        var c = coursesData.courses.Find(x => x.id == course.id);
        if (c == null) return;
        var l = new Lesson { id = DataManager.NextLessonId(c), title = lessonTitle, stars = 0 };
        c.lessons.Add(l);
        DataManager.SaveCourses(coursesData);
        coursesController.RefreshList();
    }

    public void DeleteLesson(Course course, Lesson lesson)
    {
        var c = coursesData.courses.Find(x => x.id == course.id);
        if (c == null) return;
        c.lessons.RemoveAll(x => x.id == lesson.id);
        DataManager.SaveCourses(coursesData);
        coursesController.RefreshList();
    }

    public void EditLessonTitle(Course course, Lesson lesson, string newTitle)
    {
        var c = coursesData.courses.Find(x => x.id == course.id);
        var l = c?.lessons.Find(x => x.id == lesson.id);
        if (l == null) return;
        l.title = newTitle;
        DataManager.SaveCourses(coursesData);
        coursesController.RefreshList();
    }
}