using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LessonsController : MonoBehaviour
{
    public Text headerTitle;
    public Transform lessonsListContent; // LessonsScroll -> Viewport -> Content
    public GameObject lessonItemPrefab; // LessonItemPrefab

    private Course currentCourse;

    public void OpenCourse(Course course)
    {
        currentCourse = course;
        if (headerTitle != null) headerTitle.text = course.name;
        RefreshList();
        gameObject.SetActive(true);
    }

    public void RefreshList()
    {
        if (lessonsListContent == null || lessonItemPrefab == null || currentCourse == null) return;
        foreach (Transform t in lessonsListContent) Destroy(t.gameObject);

        foreach (var lesson in currentCourse.lessons)
        {
            var go = Instantiate(lessonItemPrefab, lessonsListContent);
            var txt = go.GetComponentInChildren<Text>();
            if (txt != null) txt.text = $"{lesson.id}. {lesson.title}";
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                var captured = lesson;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnLessonClicked(captured));
            }
        }
    }

    void OnLessonClicked(Lesson lesson)
    {
        // ƒл€ начала Ч показываем Placeholder с информацией
        var placeholder = FindObjectOfType<PlaceholderPanel>();
        if (placeholder != null) placeholder.Show($"ќткрыт урок: {lesson.title}");
        // «десь можно открыть сцену урока, тест или контент
    }
}
