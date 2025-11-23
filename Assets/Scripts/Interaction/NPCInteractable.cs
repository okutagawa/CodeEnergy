using UnityEngine;
using MyGame.Models;

[RequireComponent(typeof(NPCIdentity))]
public class NPCInteractable : MonoBehaviour
{
    [HideInInspector] public TaskModel assignedGiverTask;
    [HideInInspector] public TaskModel assignedReceiverTask;

    public string DisplayName => GetComponent<NPCIdentity>()?.DisplayName ?? gameObject.name;
    public string Guid => GetComponent<NPCIdentity>()?.Guid ?? "";

    [Header("Optional bindings")]
    [Tooltip("Назначь ссылку на QuizPanelController в инспекторе (GameScene)")]
    [SerializeField] private QuizPanelController quizPanelReference;

    public virtual void Interact()
    {
        Debug.Log($"Interact on {DisplayName} (guid={Guid}). giver={(assignedGiverTask != null ? assignedGiverTask.id.ToString() : "null")}, receiver={(assignedReceiverTask != null ? assignedReceiverTask.id.ToString() : "null")}");

        // Первый NPC: диалог-объяснение
        if (assignedGiverTask != null && !string.IsNullOrEmpty(assignedGiverTask.textForGiver))
        {
            DialogueUI.Instance?.ShowForTask(assignedGiverTask, true, DisplayName);
            return;
        }

        // Второй NPC: квиз с карточками
        if (assignedReceiverTask != null && !string.IsNullOrEmpty(assignedReceiverTask.textForReceiver))
        {
            var quizPanel = quizPanelReference != null ? quizPanelReference : FindObjectOfType<QuizPanelController>();
            if (quizPanel == null)
            {
                Debug.LogError($"NPCInteractable: QuizPanelController not found in scene for {DisplayName}.");
                return;
            }

            var quizTask = new QuizTask
            {
                title = assignedReceiverTask.title,
                textForReceiver = assignedReceiverTask.textForReceiver,
                answers = assignedReceiverTask.answers != null ? new System.Collections.Generic.List<string>(assignedReceiverTask.answers) : new System.Collections.Generic.List<string>(),
                correctAnswerIndexes = assignedReceiverTask.correctAnswerIndexes != null ? new System.Collections.Generic.List<int>(assignedReceiverTask.correctAnswerIndexes) : new System.Collections.Generic.List<int>(),
                hasStars = assignedReceiverTask.hasStars
            };

            quizPanel.Show(quizTask);
            return;
        }

        // Fallback
        DialogueUI.Instance?.Show(DisplayName, "(empty dialog)");
    }
}
