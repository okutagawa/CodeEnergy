using MyGame.Models;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanelController : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Text questTitleText;
    [SerializeField] private Text questDescriptionText;

    [Header("Fallback text")]
    [SerializeField] private string noQuestTitle = "Нет активного задания";
    [SerializeField] private string noQuestDescription = "";
    [SerializeField] private bool hideWhenNoQuest;

    [Header("Completion text")]
    [SerializeField] private string allQuestsCompletedTitle = "Все задания завершены.";
    [SerializeField] private string allQuestsCompletedDescription = "Пройдите к порталу для активации итогового теста.";

    private void OnEnable()
    {
        TaskAssignmentManager.OnActiveQuestChanged += HandleActiveQuestChanged;
        RefreshFromTaskManager();
    }

    private void OnDisable()
    {
        TaskAssignmentManager.OnActiveQuestChanged -= HandleActiveQuestChanged;
    }

    private void HandleActiveQuestChanged(TaskModel task)
    {
        ApplyTask(task);
    }

    public void RefreshFromTaskManager()
    {
        var manager = TaskAssignmentManager.Instance ?? FindObjectOfType<TaskAssignmentManager>();
        ApplyTask(manager != null ? manager.GetGlobalActiveTask() : null);
    }

    public void ClearQuestPanel()
    {
        ApplyTask(null);
    }

    private void ApplyTask(TaskModel task)
    {
        if (task == null)
        {
            bool allQuestsCompleted = AreAllQuestsCompleted();
            SetPanelState(hasTask: allQuestsCompleted);
            SetTextSafe(questTitleText, allQuestsCompleted ? allQuestsCompletedTitle : noQuestTitle);
            SetTextSafe(questDescriptionText, allQuestsCompleted ? allQuestsCompletedDescription : noQuestDescription);
            return;
        }

        SetPanelState(hasTask: true);
        SetTextSafe(questTitleText, task.title);
        SetTextSafe(questDescriptionText, BuildTaskDescription(task));
    }

    private string BuildTaskDescription(TaskModel task)
    {
        if (task == null) return string.Empty;

        if (!string.IsNullOrWhiteSpace(task.textForReceiver))
            return task.textForReceiver;

        if (!string.IsNullOrWhiteSpace(task.textForGiver))
            return task.textForGiver;

        return string.Empty;
    }

    private void SetPanelState(bool hasTask)
    {
        if (panelRoot != null)
            panelRoot.SetActive(!hideWhenNoQuest || hasTask);
    }

    private bool AreAllQuestsCompleted()
    {
        var manager = TaskAssignmentManager.Instance ?? FindObjectOfType<TaskAssignmentManager>();
        return manager != null && manager.AreAllSelectedCourseTasksCompleted();
    }

    private static void SetTextSafe(Text target, string value)
    {
        if (target != null)
            target.text = value ?? string.Empty;
    }
}