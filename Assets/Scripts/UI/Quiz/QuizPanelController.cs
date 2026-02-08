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

    private readonly List<QuizCardController> _cards = new List<QuizCardController>();
    private QuizTask _task;
    private bool _isMulti;
    private NPCInteractable _sourceNpc;

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

        if (isCorrect)
        {
            _sourceNpc?.ConfirmStartTask();

            if (_task != null)
            {
                GameState.Instance?.MarkTaskCompleted(_task.taskId);
                Debug.Log($"[DEBUG] Marked completed task {_task.taskId}. Completed count={GameState.Instance.GetData().completedTaskIds.Count}");
                TaskAssignmentManager.Instance?.ExportQueuesToGameState();
            }

            if (_task.hasStars && rewardPanelPrefab != null)
            {
                var reward = Instantiate(rewardPanelPrefab, transform.parent);
                reward.Show("Задание выполнено!", CalculateStars(selected, correct));
            }
            else
            {
                ClosePanel();
            }
        }
    }

    private int CalculateStars(List<int> selected, List<int> correct)
    {
        if (selected.SequenceEqual(correct)) return 3;
        bool subset = selected.All(i => correct.Contains(i));
        if (subset && selected.Count > 0) return 2;
        return selected.Intersect(correct).Any() ? 1 : 0;
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
