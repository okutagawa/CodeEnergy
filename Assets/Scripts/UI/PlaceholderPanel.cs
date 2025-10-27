using UnityEngine;
using UnityEngine.UI;

// Простая панель для сообщений/заглушек с кнопкой закрытия
public class PlaceholderPanel : MonoBehaviour
{
    public Text messageText;
    public Button closeButton;

    void Start()
    {
        if (closeButton != null) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        if (messageText != null) messageText.text = message;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
