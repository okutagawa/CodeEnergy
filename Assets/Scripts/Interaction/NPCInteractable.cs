// Assets/Scripts/Interaction/NPCInteractable.cs
using UnityEngine;
using MyGame.Models;

[RequireComponent(typeof(NPCIdentity))]
public class NPCInteractable : MonoBehaviour
{
    private TaskModel _peekGiverTask;
    private TaskModel _peekReceiverTask;

    public string DisplayName => GetComponent<NPCIdentity>()?.DisplayName ?? gameObject.name;
    public string Guid => GetComponent<NPCIdentity>()?.Guid ?? "";

    [Header("Optional bindings")]
    [SerializeField] private QuizPanelController quizPanelReference;

    // Этот метод вызывается TaskAssignmentManager после загрузки/изменения очереди
    public void SetAssignedTasks(TaskModel peekForGiver, TaskModel peekForReceiver)
    {
        _peekGiverTask = peekForGiver;
        _peekReceiverTask = peekForReceiver;
    }

    public virtual void Interact()
    {
        Debug.Log($"Interact on {DisplayName} (guid={Guid}). giver={(_peekGiverTask != null ? _peekGiverTask.id.ToString() : "null")}, receiver={(_peekReceiverTask != null ? _peekReceiverTask.id.ToString() : "null")}");

        // Giver: показываем теорию (peek), не удаляем задачу
        if (_peekGiverTask != null && !string.IsNullOrEmpty(_peekGiverTask.textForGiver))
        {
            DialogueUI.Instance?.ShowForTask(_peekGiverTask, true, DisplayName);
            return;
        }

        // Receiver: показываем квиз для peek (потом при подтверждении — GetNext)
        if (_peekReceiverTask != null && !string.IsNullOrEmpty(_peekReceiverTask.textForReceiver))
        {
            var quizPanel = quizPanelReference != null ? quizPanelReference : FindObjectOfType<QuizPanelController>();
            if (quizPanel == null)
            {
                Debug.LogError($"NPCInteractable: QuizPanelController not found in scene for {DisplayName}.");
                return;
            }

            var quizTask = new QuizTask
            {
                title = _peekReceiverTask.title,
                textForReceiver = _peekReceiverTask.textForReceiver,
                answers = _peekReceiverTask.answers != null ? new System.Collections.Generic.List<string>(_peekReceiverTask.answers) : new System.Collections.Generic.List<string>(),
                correctAnswerIndexes = _peekReceiverTask.correctAnswerIndexes != null ? new System.Collections.Generic.List<int>(_peekReceiverTask.correctAnswerIndexes) : new System.Collections.Generic.List<int>(),
                hasStars = _peekReceiverTask.hasStars
            };

            // Передаём ссылку на этого NPC в QuizPanel, чтобы при успешном выполнении он мог подтвердить старт задачи
            quizPanel.Show(quizTask, this);
            return;
        }

        // Fallback
        DialogueUI.Instance?.Show(DisplayName, "(empty dialog)");
    }

    // Вызывается, когда игрок подтвердил/успешно выполнил задание у Receiver
    public void ConfirmStartTask()
    {
        var manager = FindObjectOfType<TaskAssignmentManager>();
        if (manager == null)
        {
            Debug.LogError("ConfirmStartTask: TaskAssignmentManager not found");
            return;
        }

        var started = manager.GetNextForReceiver(Guid);
        if (started != null)
        {
            Debug.Log($"NPC {DisplayName} confirmed start of task {started.id}");
            // пометка в GameState и т.д.
        }
    }
}
