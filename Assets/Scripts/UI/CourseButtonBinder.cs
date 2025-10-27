using UnityEngine;
using UnityEngine.UI;

// биндер: хранит данные курса и настраивает действи€ кнопок префаба
public class CourseButtonBinder : MonoBehaviour
{
    public Text courseNameText;
    public Button mainButton; // открытие курса
    public Button editButton; // optional, admin only
    public Button deleteButton; // optional, admin only

    private Course course;
    private CoursesController parentController;

    public void Init(Course courseData, CoursesController controller)
    {
        course = courseData;
        parentController = controller;
        if (courseNameText != null) courseNameText.text = course.name;

        if (mainButton != null)
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() => parentController.OnOpenCourse(course));
        }

        if (editButton != null)
        {
            editButton.onClick.RemoveAllListeners();
            editButton.onClick.AddListener(() => parentController.OnEditCourseClicked(course));
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => parentController.OnDeleteCourseClicked(course));
        }

        // ѕоказать/скрыть элементы управлени€ в зависимости от режима
        if (editButton != null) editButton.gameObject.SetActive(GameState.Instance != null && GameState.Instance.IsAdminMode);
        if (deleteButton != null) deleteButton.gameObject.SetActive(GameState.Instance != null && GameState.Instance.IsAdminMode);
    }
}
