using UnityEngine;
using UnityEngine.UI;
using System;

public class RewardPanelController : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Text titleText;         // "Задание выполнено!"
    [SerializeField] private Text statusText;
    [SerializeField] private Image[] starImages;     // 3 квадрата-«звезды»
    [SerializeField] private Button okButton;

    public Action OnClosed; // вызывается при закрытии окна

    private bool _closeQuizOnClose = true;

    private void Awake()
    {
        if (okButton != null)
            okButton.onClick.AddListener(HandleClose);
    }

    public void Show(string title, int starsCount)
    {
        ShowSuccess(title, starsCount, 0);
    }

    public void ShowSuccess(string title, int starsCount, int failedAttemptsBeforeSuccess)
    {
        _closeQuizOnClose = true;

        int safeStars = Mathf.Max(0, starsCount);
        int safeAttempts = Mathf.Max(0, failedAttemptsBeforeSuccess);
        string resolvedTitle = string.IsNullOrEmpty(title) ? " " : title;
        string resolvedStatus = BuildSuccessStatus(safeStars, safeAttempts);

        ApplyText(resolvedTitle, resolvedStatus);
        SetStarsVisible(safeStars);

        //    (  ,    )
        gameObject.SetActive(true);
    }

    public void ShowFailure(string title = null, string status = null)
    {
        _closeQuizOnClose = false;

        string resolvedTitle = string.IsNullOrEmpty(title) ? "  " : title;
        string resolvedStatus = string.IsNullOrEmpty(status)
            ? " .            ."
            : status;

        ApplyText(resolvedTitle, resolvedStatus);
        SetStarsVisible(0);

        //    (  ,    )
        gameObject.SetActive(true);
    }

    private void ApplyText(string title, string status)
    {
        if (titleText != null) titleText.text = title;
        if (statusText != null) statusText.text = status;
    }

    private string BuildSuccessStatus(int starsCount, int failedAttemptsBeforeSuccess)
    {
        string starsWord = GetStarsWord(starsCount);

        if (failedAttemptsBeforeSuccess > 0)
        {
            string attemptsWord = GetAttemptsWord(failedAttemptsBeforeSuccess);
            return $"  .  {failedAttemptsBeforeSuccess} {attemptsWord}  :   {starsCount} {starsWord}.";
        }

        return $"  .   {starsCount} {starsWord}.";
    }

    private string GetStarsWord(int starsCount)
    {
        int value = Mathf.Abs(starsCount) % 100;
        int lastDigit = value % 10;

        if (value >= 11 && value <= 14) return "";
        if (lastDigit == 1) return "";
        if (lastDigit >= 2 && lastDigit <= 4) return "";
        return "";
    }

    private string GetAttemptsWord(int attemptsCount)
    {
        int value = Mathf.Abs(attemptsCount) % 100;
        int lastDigit = value % 10;

        if (value >= 11 && value <= 14) return " ";
        if (lastDigit == 1) return " ";
        if (lastDigit >= 2 && lastDigit <= 4) return " ";
        return " ";
    }

    private void SetStarsVisible(int starsCount)
    {
        if (starImages == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            bool enabled = i < Mathf.Clamp(starsCount, 0, starImages.Length);
            if (starImages[i] != null)
                starImages[i].enabled = enabled;
        }
    }

    private void HandleClose()
    {
        // Скрываем панель награды
        gameObject.SetActive(false);

        // Закрываем QuizPanel (если он открыт поверх или под)
        // Попытаемся найти QuizPanelController в родительских объектах / сцене
        var quizPanel = FindObjectOfType<QuizPanelController>();
        if (quizPanel != null)
        {
            if (_closeQuizOnClose)
            {
                quizPanel.ForceCloseFromReward();
            }
            else
            {
                quizPanel.RetryCurrentTaskFromReward();
            }
        }

        OnClosed?.Invoke();

        // Удаляем объект (если был инстанцирован)
        Destroy(gameObject);
    }
}
