using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("UI References")]
    public InputField passwordInput;
    public Button confirmBtn;
    public Button cancelBtn;
    public Text attemptsText;
    public GameObject roleSelectionPanel; // дл€ возврата при Cancel или можно использовать UIManager

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
        // —брасывать поле при показе панели
        if (passwordInput != null) passwordInput.text = "";
        // при желании не сбрасывать attemptsLeft при каждом открытии Ч зависит от требований
        UpdateAttemptsText();
    }

    void OnConfirm()
    {
        Debug.Log("AdminPasswordController: OnConfirm invoked");

        // ѕроверки прив€занных полей
        if (passwordInput == null)
        {
            Debug.LogError("AdminPasswordController: passwordInput not assigned in Inspector");
            return;
        }

        string entered = passwordInput.text ?? "";
        Debug.Log("AdminPasswordController: entered password length = " + entered.Length);

        if (entered == expectedPassword)
        {
            Debug.Log("AdminPasswordController: password correct");

            if (GameState.Instance != null)
            {
                GameState.Instance.IsAdminMode = true;
            }
            else
            {
                Debug.LogWarning("AdminPasswordController: GameState.Instance is null");
            }

            // ѕопытка использовать UIManager дл€ перехода
            if (UIManager.Instance != null)
            {
                if (UIManager.Instance.coursesPanel != null)
                {
                    UIManager.Instance.ShowOnly(UIManager.Instance.coursesPanel);
                }
                else
                {
                    Debug.LogError("AdminPasswordController: UIManager.coursesPanel is not assigned in Inspector");
                }
            }
            else
            {
                Debug.LogError("AdminPasswordController: UIManager.Instance is null (UIManager missing or not initialized)");
                // fallback: если роль-панель прив€зана, просто скрываем текущую и показываем еЄ
                if (roleSelectionPanel != null)
                {
                    roleSelectionPanel.SetActive(false);
                }
            }

            _attemptsLeft = maxAttempts;
            UpdateAttemptsText();
            return;
        }

        // Ќеверный пароль
        _attemptsLeft--;
        UpdateAttemptsText();

        if (_attemptsLeft <= 0)
        {
            Debug.LogWarning("AdminPasswordController: attempts exhausted, quitting application");
            Application.Quit();
            return;
        }

        passwordInput.text = "";
        passwordInput.Select();
        passwordInput.ActivateInputField();
    }

    void OnCancel()
    {
        // ¬ернутьс€ к выбору роли
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
            attemptsText.text = $"ќсталось попыток: {_attemptsLeft}";
        }
    }
}
