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
    public float doubleClickThreshold = 0.35f;

    public Action<CourseModel> onSingleClick;
    public Action<CourseModel> onDoubleClick;

    public void Initialize(CourseModel course)
    {
        data = course;
        if (textTitle != null) textTitle.text = course.name;
        else Debug.LogWarning("CourseItem: textTitle not assigned in inspector for prefab " + name);

        if (buttonRoot != null)
        {
            buttonRoot.onClick.RemoveAllListeners();
            buttonRoot.onClick.AddListener(OnRootClicked);
        }
        else Debug.LogWarning("CourseItem: buttonRoot not assigned in prefab " + name);
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

    public void SetSelected(bool selected)
    {
        var img = GetComponent<Image>();
        if (img != null) img.color = selected ? new Color(0.8f, 0.9f, 1f) : Color.white;
    }
}
