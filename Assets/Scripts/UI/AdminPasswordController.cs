using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("UI References")]
    public InputField passwordInput;
    public Button confirmBtn;
    public Button cancelBtn;
    public Text attemptsText;
    public GameObject roleSelectionPanel; // для возврата при Cancel или можно использовать UIManager

    [Header("Settings")]
    public string expectedPassword = "admin"; // заменить на безопасное хранение при необходимости
    public int maxAttempts = 3;

    private int _attemptsLeft;

    void Awake()
    {
        _attemptsLeft = maxAttempts;
    }

    void Start()
    {
        if (confirmBtn != null) confirmBtn.onClick.RemoveAllListeners();
        if (cancelBtn != null) cancelBtn.onClick.RemoveAllListeners();

        if (confirmBtn != null) confirmBtn.onClick.AddListener(OnConfirm);
        if (cancelBtn != null) cancelBtn.onClick.AddListener(OnCancel);

        UpdateAttemptsText();
    }

    void OnEnable()
    {
        // Сбрасывать поле при показе панели
        if (passwordInput != null) passwordInput.text = "";
        // при желании не сбрасывать attemptsLeft при каждом открытии — зависит от требований
        UpdateAttemptsText();
    }

    void OnConfirm()
    {
        if (passwordInput == null) return;

        var entered = passwordInput.text ?? "";
        if (entered == expectedPassword)
        {
            // Успешный вход
            GameState.Instance.IsAdminMode = true;
            // Переходим в панель курсов через UIManager если он есть
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowOnly(UIManager.Instance.coursesPanel);
            }
            else
            {
                // fallback — скрыть текущую и показать coursesPanel если привязана вручную
                if (roleSelectionPanel != null) roleSelectionPanel.SetActive(false);
            }
            // Сбрасываем счётчик для следующего входа
            _attemptsLeft = maxAttempts;
            UpdateAttemptsText();
            return;
        }

        // Неверный пароль
        _attemptsLeft--;
        UpdateAttemptsText();
        if (_attemptsLeft <= 0)
        {
            // Кончатся попытки — закрываем приложение
            // В Editor Application.Quit не выйдет — для билда это будет работать
            Application.Quit();
            return;
        }

        // Очистить поле и дать фокус для повторной попытки
        passwordInput.text = "";
        passwordInput.Select();
        passwordInput.ActivateInputField();
    }

    void OnCancel()
    {
        // Вернуться к выбору роли
        if (UIManager.Instance != null && roleSelectionPanel != null)
        {
            UIManager.Instance.ShowOnly(roleSelectionPanel);
        }
        else if (roleSelectionPanel != null)
        {
            roleSelectionPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    void UpdateAttemptsText()
    {
        if (attemptsText != null)
        {
            attemptsText.text = $"Осталось попыток: {_attemptsLeft}";
        }
    }
}
