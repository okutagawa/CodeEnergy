using System;
using MyGame.Models;
using UnityEngine;
using UnityEngine.UI;

public class CourseSelectItem : MonoBehaviour
{
    [Header("UI")]
    public Text courseTitleText;
    public Text taskCountText;
    public Button buttonRoot;
    public Image backgroundImage;

    [Header("Colors")]
    public Color normalColor = new Color(1f, 1f, 1f, 1f);
    public Color selectedColor = new Color(0.65f, 0.9f, 1f, 1f);

    [Header("Double Click")]
    public float doubleClickThreshold = 0.35f;

    private CourseModel course;
    private Action<CourseModel> onSingleClick;
    private Action<CourseModel> onDoubleClick;
    private float lastClickTime = -1f;

    public void Initialize(
        CourseModel courseModel,
        Action<CourseModel> singleClickCallback,
        Action<CourseModel> doubleClickCallback)
    {
        course = courseModel;
        onSingleClick = singleClickCallback;
        onDoubleClick = doubleClickCallback;

        if (courseTitleText != null)
        {
            courseTitleText.text = course != null && !string.IsNullOrWhiteSpace(course.name)
                ? course.name
                : "Ѕез названи€";
        }

        if (taskCountText != null)
        {
            int taskCount = course != null && course.taskIds != null ? course.taskIds.Count : 0;
            taskCountText.text = FormatTaskCount(taskCount);
        }

        if (buttonRoot == null)
        {
            buttonRoot = GetComponent<Button>();
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        if (buttonRoot != null)
        {
            buttonRoot.onClick.RemoveAllListeners();
            buttonRoot.onClick.AddListener(OnItemClicked);
        }

        SetSelected(false);
    }

    private void OnItemClicked()
    {
        if (course == null) return;

        float currentTime = Time.unscaledTime;

        if (lastClickTime > 0f && currentTime - lastClickTime <= doubleClickThreshold)
        {
            lastClickTime = -1f;
            onDoubleClick?.Invoke(course);
        }
        else
        {
            lastClickTime = currentTime;
            onSingleClick?.Invoke(course);
        }
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (buttonRoot != null)
        {
            buttonRoot.interactable = interactable;
        }
    }

    public int GetCourseId()
    {
        return course != null ? course.id : -1;
    }

    public CourseModel GetCourse()
    {
        return course;
    }

    private static string FormatTaskCount(int taskCount)
    {
        int lastTwoDigits = taskCount % 100;
        int lastDigit = taskCount % 10;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            return taskCount + " заданий";

        if (lastDigit == 1)
            return taskCount + " задание";

        if (lastDigit >= 2 && lastDigit <= 4)
            return taskCount + " задани€";

        return taskCount + " заданий";
    }
}