using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MyGame.Models;
using MyGame.Data;

public class CourseListManager : MonoBehaviour
{
    public RectTransform contentCourses;
    public GameObject prefabCourseItem;
    public InputField inputCourseName;
    public Button buttonAddCourse;
    public Button buttonDeleteSelected; // привязать внешнюю кнопку удаления
    public Button buttonEditSelected;   // опционально: кнопка редактирования

    private CoursesContainer container;
    private Dictionary<int, GameObject> instantiated = new Dictionary<int, GameObject>();
    private int selectedCourseId = -1;

    void Start()
    {
        buttonAddCourse.onClick.AddListener(OnAddCourseClicked);
        if (buttonDeleteSelected != null) buttonDeleteSelected.onClick.AddListener(DeleteSelectedCourse);
        container = DataManager.LoadCourses();
        RefreshUI();
    }

    void OnAddCourseClicked()
    {
        var title = inputCourseName.text.Trim();
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
        item.Initialize(c);
        item.onSingleClick = OnCourseSingleClick;
        // item.onDoubleClick = OnCourseDoubleClick;
        instantiated[c.id] = go;
        // Если добавляем элемент — обновим визуал выделения
        item.SetSelected(c.id == selectedCourseId);
    }

    void OnCourseSingleClick(CourseModel c)
    {
        SelectCourse(c.id);
    }

    //void OnCourseDoubleClick(CourseModel c)
    //{
    //    // Переход в окно задач
    //    UIManager.Instance?.OpenTasksWindowForCourse(c.id);
    //}

    // Выполняем выбор: обновляем selectedCourseId и подсветку элементов
    public void SelectCourse(int courseId)
    {
        if (selectedCourseId == courseId) return; // уже выбран
        // снять выделение у предыдущего
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
        // находим модель
        var model = container.courses.Find(x => x.id == selectedCourseId);
        if (model == null) return;

        // удаляем UI
        if (instantiated.TryGetValue(selectedCourseId, out var go))
        {
            Destroy(go);
            instantiated.Remove(selectedCourseId);
        }

        // удаляем из контейнера и сохраняем
        container.courses.RemoveAll(x => x.id == selectedCourseId);
        DataManager.SaveCourses(container);

        // сброс выбора
        selectedCourseId = -1;
    }

    void RefreshUI()
    {
        foreach (var kv in instantiated.Values) Destroy(kv);
        instantiated.Clear();
        foreach (var c in container.courses) AddCourseToUI(c);
    }
}
