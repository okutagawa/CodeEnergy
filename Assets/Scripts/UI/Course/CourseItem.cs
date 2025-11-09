using UnityEngine;
using UnityEngine.UI;
using System;
using MyGame.Models;

public class CourseItem : MonoBehaviour
{
    public Text textTitle;
    public Button buttonRoot;

    private CourseModel data;
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    public Action<CourseModel> onSingleClick;
    public Action<CourseModel> onDoubleClick;

    public void Initialize(CourseModel course)
    {
        data = course;
        textTitle.text = course.name;
        buttonRoot.onClick.RemoveAllListeners();
        buttonRoot.onClick.AddListener(OnRootClicked);
    }

    private void OnRootClicked()
    {
        float t = Time.unscaledTime;
        if (t - lastClickTime < doubleClickThreshold)
        {
            onDoubleClick?.Invoke(data);
            lastClickTime = 0f;
        }
        else
        {
            onSingleClick?.Invoke(data);
            lastClickTime = t;
        }
    }

    // Вспомогательный метод для визуального выделения
    public void SetSelected(bool selected)
    {
        var img = GetComponent<Image>();
        if (img != null)
            img.color = selected ? new Color(0.8f, 0.9f, 1f) : Color.white;
    }
}
