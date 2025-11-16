using UnityEngine;
using MyGame.Models;

[RequireComponent(typeof(NPCIdentity))]
public class NPCInteractable : MonoBehaviour
{
    // <-- добавленные поля для хранения назначенных задач
    [HideInInspector] public TaskModel assignedGiverTask;
    [HideInInspector] public TaskModel assignedReceiverTask;

    // опционально: для логирования / отображения имени
    public string DisplayName => GetComponent<NPCIdentity>()?.DisplayName ?? gameObject.name;
    public string Guid => GetComponent<NPCIdentity>()?.Guid ?? "";

    // В Start/OnEnable ничего не требуется — назначение делается извне (TaskAssignmentManager)
    public virtual void Interact()
    {
        Debug.Log($"Interact called on {DisplayName} (guid={Guid}). assignedGiverTask={(assignedGiverTask != null ? assignedGiverTask.id.ToString() : "null")}, assignedReceiverTask={(assignedReceiverTask != null ? assignedReceiverTask.id.ToString() : "null")}");

        // Показать текст дающего только если он назначен и текст не пуст
        if (assignedGiverTask != null && !string.IsNullOrEmpty(assignedGiverTask.textForGiver))
        {
            DialogueUI.Instance?.ShowForTask(assignedGiverTask, true, DisplayName);
            return;
        }

        // Показать текст получателя если есть и текст не пуст
        if (assignedReceiverTask != null && !string.IsNullOrEmpty(assignedReceiverTask.textForReceiver))
        {
            DialogueUI.Instance?.ShowForTask(assignedReceiverTask, false, DisplayName);
            return;
        }

        // Если один из тасков есть, но текст пуст — логнуть и показать fallback
        if (assignedGiverTask != null || assignedReceiverTask != null)
        {
            Debug.LogWarning($"Interact: task exists for {DisplayName} but text is empty. giverText='{assignedGiverTask?.textForGiver}', receiverText='{assignedReceiverTask?.textForReceiver}'");
        }

        DialogueUI.Instance?.Show(DisplayName, "(empty dialog)");
    }

}
