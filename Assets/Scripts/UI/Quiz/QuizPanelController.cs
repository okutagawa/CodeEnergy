using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuizPanelController : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text bodyText;
    [SerializeField] private RectTransform cardsContainer;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    [Header("Prefabs")]
    [SerializeField] private QuizCardController quizCardPrefab;
    [SerializeField] private RewardPanelController rewardPanelPrefab;

    [Header("Reward tuning")]
    [SerializeField] private float fastCompletionThresholdSeconds = 20f;


    private readonly List<QuizCardController> _cards = new List<QuizCardController>();
    private QuizTask _task;
    private bool _isMulti;
    private NPCInteractable _sourceNpc;
    private float _openedAtUnscaledTime;

    private void Awake()
    {
        if (submitButton != null) submitButton.onClick.AddListener(HandleSubmit);
        if (nextButton != null) nextButton.onClick.AddListener(HandleNext);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
    }

    public void Show(QuizTask task, NPCInteractable sourceNpc = null)
    {
        _sourceNpc = sourceNpc;
        _task = task;
        _openedAtUnscaledTime = Time.unscaledTime;
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ShowRoutine(task));
    }

    private IEnumerator ShowRoutine(QuizTask task)
    {
        _task = task;
        _isMulti = task != null && task.correctAnswerIndexes != null && task.correctAnswerIndexes.Count > 1;

        if (titleText != null) titleText.text = task?.title ?? "";
        if (bodyText != null) bodyText.text = task?.textForReceiver ?? "";

        ClearCards();
        CreateCards(task?.answers ?? new List<string>());

        yield return null;

        Canvas.ForceUpdateCanvases();
        if (cardsContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardsContainer);
            foreach (RectTransform child in cardsContainer)
            {
                if (child == null) continue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(child);

                var gfxs = child.GetComponentsInChildren<Graphic>(true);
                foreach (var g in gfxs) g.SetAllDirty();
            }
        }
        Canvas.ForceUpdateCanvases();

        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (submitButton != null) submitButton.interactable = true;
    }

    private void CreateCards(List<string> answers)
    {
        if (quizCardPrefab == null || cardsContainer == null) return;

        for (int i = 0; i < answers.Count; i++)
        {
            var card = Instantiate(quizCardPrefab, cardsContainer);
            card.Setup(i, answers[i]);
            card.OnClicked.AddListener(HandleCardClicked);
            _cards.Add(card);
        }
    }

    private void HandleCardClicked(int optionIndex)
    {
        if (_isMulti)
        {
            var target = _cards.FirstOrDefault(c => c.OptionIndex == optionIndex);
            if (target != null) target.SetSelected(!target.IsSelected);
        }
        else
        {
            foreach (var c in _cards)
                c.SetSelected(c.OptionIndex == optionIndex);
        }
    }

    private void HandleSubmit()
    {
        if (_task == null || _cards.Count == 0) return;

        var selected = _cards.Where(c => c.IsSelected).Select(c => c.OptionIndex).OrderBy(x => x).ToList();
        var correct = _task.correctAnswerIndexes != null ? _task.correctAnswerIndexes.OrderBy(x => x).ToList() : new List<int>();
        bool isCorrect = selected.SequenceEqual(correct);

        foreach (var c in _cards)
        {
            c.LockAfterSubmit();
            bool showIcon = c.IsSelected || correct.Contains(c.OptionIndex);
            c.ShowStateIcon(showIcon);
        }

        if (submitButton != null) submitButton.interactable = false;
        if (nextButton != null) nextButton.gameObject.SetActive(true);

        var gameState = GameState.Instance;
        if (gameState == null)
        {
            Debug.LogWarning("[Quiz] GameState.Instance is null. Reward/accounting logic is skipped.");
            if (isCorrect) ClosePanel();
            return;
        }
        if (!isCorrect)
        {
            gameState.RegisterFailedQuizAttempt(_task.taskId);
            return;
        }

        _sourceNpc?.ConfirmStartTask();

        if (_task != null)
        {
            GameState.Instance.MarkTaskCompleted(_task.taskId);
            Debug.Log($"[DEBUG] Marked completed task {_task.taskId}. Completed count={GameState.Instance.GetData().completedTaskIds.Count}");
            TaskAssignmentManager.Instance?.ExportQueuesToGameState();
        }

        if (_task != null && _task.hasStars)
        {
            int failedAttempts = gameState.GetFailedQuizAttempts(_task.taskId);
            float duration = Mathf.Max(0f, Time.unscaledTime - _openedAtUnscaledTime);
            int starsAwarded = CalculateStarsForCompletion(failedAttempts, duration);
            gameState.TryAwardTaskStars(_task.taskId, starsAwarded, failedAttempts);


            if (rewardPanelPrefab != null)
            {
                var reward = Instantiate(rewardPanelPrefab, transform.parent);
                reward.Show("Задание выполнено!", starsAwarded);
            }
            else
            {
                ClosePanel();
            }
        }
        else
        {
            gameState.ClearQuizProgress(_task.taskId);
            ClosePanel();
        }
    }

    private int CalculateStarsForCompletion(int failedAttemptsBeforeSuccess, float completionSeconds)
    {
        int safeFails = Mathf.Max(0, failedAttemptsBeforeSuccess);

        if (safeFails >= 2)
            return 1;

        if (safeFails == 1)
            return 2;

        return completionSeconds <= fastCompletionThresholdSeconds ? 3 : 2;
    }

    private void HandleNext()
    {
        ClosePanel();
    }

    private void ClosePanel()
    {
        _task = null;
        _sourceNpc = null;
        gameObject.SetActive(false);
    }

    public void ForceCloseFromReward()
    {
        gameObject.SetActive(false);
    }

    private void ClearCards()
    {
        if (cardsContainer == null) return;
        foreach (Transform child in cardsContainer)
            Destroy(child.gameObject);
        _cards.Clear();
    }
}
