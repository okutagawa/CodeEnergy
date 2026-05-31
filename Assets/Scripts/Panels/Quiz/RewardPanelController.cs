// UI/RewardPanelController.cs  (обновлённая версия)
using UnityEngine;
using UnityEngine.UI;
using System;

public class RewardPanelController : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Text titleText;         // "Задание выполнено!"
    [SerializeField] private Image[] starImages;     // 3 квадрата-«звезды»
    [SerializeField] private Button okButton;

    public Action OnClosed; // вызывается при закрытии окна

    private void Awake()
    {
        if (okButton != null)
            okButton.onClick.AddListener(HandleClose);
    }

    public void Show(string title, int starsCount)
    {
        if (titleText != null) titleText.text = string.IsNullOrEmpty(title) ? "Задание выполнено!" : title;

        for (int i = 0; i < starImages.Length; i++)
        {
            bool enabled = i < Mathf.Clamp(starsCount, 0, starImages.Length);
            if (starImages[i] != null)
                starImages[i].enabled = enabled;
        }

        // Показываем сам объект (если префаб инстанцируется, он обычно уже активен)
        gameObject.SetActive(true);
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
            quizPanel.ForceCloseFromReward(); // вызываем безопасный метод, реализуемый ниже
        }

        OnClosed?.Invoke();

        // Удаляем объект (если был инстанцирован)
        Destroy(gameObject);
    }
}
