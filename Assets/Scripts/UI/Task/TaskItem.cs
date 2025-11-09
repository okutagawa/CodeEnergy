using UnityEngine;
using UnityEngine.UI;
using System;
using MyGame.Models;

public class TaskItem : MonoBehaviour
{
    public Text textTitle;
    public Button buttonRoot;
    private TaskModel data;
    private float lastClick;
    private const float doubleClickThreshold = 0.3f;

    public Action<TaskModel> onSingleClick;
    public Action<TaskModel> onDoubleClick;

    public void Initialize(TaskModel model)
    {
        data = model;
        textTitle.text = string.IsNullOrEmpty(model.title) ? "(empty)" : model.title;
        buttonRoot.onClick.RemoveAllListeners();
        buttonRoot.onClick.AddListener(OnRootClicked);
    }

    private void OnRootClicked()
    {
        var t = Time.unscaledTime;
        if (t - lastClick < doubleClickThreshold)
        {
            onDoubleClick?.Invoke(data);
            lastClick = 0f;
        }
        else
        {
            onSingleClick?.Invoke(data);
            lastClick = t;
        }
    }

    public void SetSelected(bool sel)
    {
        var img = GetComponent<Image>();
        if (img != null) img.color = sel ? new Color(0.85f, 0.95f, 1f) : Color.white;
    }

    public void UpdateTitle(string newTitle)
    {
        textTitle.text = string.IsNullOrEmpty(newTitle) ? "(empty)" : newTitle;
    }
}
