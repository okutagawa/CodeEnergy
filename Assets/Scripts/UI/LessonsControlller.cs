using UnityEngine;
using UnityEngine.UI;

public class LessonsController : MonoBehaviour
{
    public Transform lessonListContent;
    public GameObject lessonItemPrefab;
    public Text headerTitle;

    private Course currentCourse;

    public void SetCourse(Course course)
    {
        currentCourse = course;
        headerTitle.text = course.name;
        Refresh();
    }

    void Refresh()
    {
        foreach (Transform t in lessonListContent) Destroy(t.gameObject);
        foreach (var lesson in currentCourse.lessons)
        {
            var go = Instantiate(lessonItemPrefab, lessonListContent);
            var title = go.transform.Find("Title").GetComponent<Text>();
            var stars = go.transform.Find("Stars").GetComponent<Text>();
            title.text = lesson.title;
            stars.text = $"Stars: {lesson.stars}";
            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => UIManager.Instance.ShowPlaceholder($"Здесь будет квиз: {lesson.title}"));
        }
    }
}