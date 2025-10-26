using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AdminController : MonoBehaviour
{
    public InputField courseNameInput;
    public Button saveCourseBtn;
    public Transform lessonsListContent;
    public GameObject lessonItemPrefab;
    public InputField newLessonTitleInput;
    public Button addLessonBtn;
    public Button closeAdminBtn;

    private CoursesContainer coursesData;
    private Course editingCourse;

    void Start()
    {
        coursesData = DataManager.LoadCourses();
        if (saveCourseBtn != null)
        {
            saveCourseBtn.onClick.RemoveAllListeners();
            saveCourseBtn.onClick.AddListener(OnSaveCourseName);
        }
        if (addLessonBtn != null)
        {
            addLessonBtn.onClick.RemoveAllListeners();
            addLessonBtn.onClick.AddListener(OnAddLesson);
        }
        if (closeAdminBtn != null)
        {
            closeAdminBtn.onClick.RemoveAllListeners();
            closeAdminBtn.onClick.AddListener(Close);
        }
        gameObject.SetActive(false);
    }

    public void OpenCourseEditor(Course course)
    {
        if (course == null) return;
        // Загружаем актуальные данные и находим курс по id
        coursesData = DataManager.LoadCourses();
        editingCourse = coursesData.courses.Find(c => c.id == course.id);
        if (editingCourse == null)
        {
            // Если курс не найден в контейнере, используем переданный объект и добавляем в контейнер
            editingCourse = course;
            if (!coursesData.courses.Any(c => c.id == editingCourse.id))
                coursesData.courses.Add(editingCourse);
        }

        if (courseNameInput != null) courseNameInput.text = editingCourse.name;
        RefreshLessonsEditor();
        gameObject.SetActive(true);

        // при необходимости обновляем UIManager
        if (UIManager.Instance != null) UIManager.Instance.ShowAdminPanel();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void RefreshLessonsEditor()
    {
        if (lessonsListContent == null || editingCourse == null) return;
        // Очистка контейнера
        foreach (Transform t in lessonsListContent) Destroy(t.gameObject);

        foreach (var lesson in editingCourse.lessons)
        {
            var go = Instantiate(lessonItemPrefab, lessonsListContent);
            var txt = go.GetComponentInChildren<Text>();
            if (txt != null) txt.text = $"{lesson.id}. {lesson.title}";

            var deleteBtn = go.transform.Find("DeleteBtn")?.GetComponent<Button>();
            if (deleteBtn != null)
            {
                var captured = lesson;
                deleteBtn.onClick.RemoveAllListeners();
                deleteBtn.onClick.AddListener(() => {
                    DeleteLesson(captured);
                });
            }

            var renameBtn = go.transform.Find("RenameBtn")?.GetComponent<Button>();
            if (renameBtn != null)
            {
                var captured = lesson;
                renameBtn.onClick.RemoveAllListeners();
                renameBtn.onClick.AddListener(() => {
                    if (newLessonTitleInput != null) newLessonTitleInput.text = captured.title;
                    // можно хранить editingLessonForRename = captured для подтверждения переименования
                });
            }
        }
    }

    void OnSaveCourseName()
    {
        if (editingCourse == null || courseNameInput == null) return;
        var newName = courseNameInput.text.Trim();
        if (string.IsNullOrEmpty(newName)) return;
        editingCourse.name = newName;
        DataManager.SaveCourses(coursesData);
        // Обновляем список курсов, если он есть в сцене
        var cc = FindObjectOfType<CoursesController>();
        if (cc != null) cc.RefreshList();
    }

    void OnAddLesson()
    {
        if (editingCourse == null || newLessonTitleInput == null) return;
        var title = newLessonTitleInput.text.Trim();
        if (string.IsNullOrEmpty(title)) return;
        var newId = DataManager.NextLessonId(editingCourse);
        var lesson = new Lesson { id = newId, title = title, stars = 0 };
        editingCourse.lessons.Add(lesson);
        DataManager.SaveCourses(coursesData);
        newLessonTitleInput.text = "";
        RefreshLessonsEditor();
        var cc = FindObjectOfType<CoursesController>();
        if (cc != null) cc.RefreshList();
    }

    void DeleteLesson(Lesson lesson)
    {
        if (editingCourse == null || lesson == null) return;
        editingCourse.lessons.RemoveAll(x => x.id == lesson.id);
        DataManager.SaveCourses(coursesData);
        RefreshLessonsEditor();
        var cc = FindObjectOfType<CoursesController>();
        if (cc != null) cc.RefreshList();
    }
}
