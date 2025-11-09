using UnityEngine;
using UnityEngine.UI;

public class AdminPasswordController : MonoBehaviour
{
    [Header("UI References")]
    public InputField passwordInput;
    public Button confirmBtn;
    public Button cancelBtn;
    public Text attemptsText;
    public GameObject roleSelectionPanel;

    [Header("Settings")]
    public string expectedPassword = "admin";
    public int maxAttempts = 3;

    private int _attemptsLeft;

    void Awake() => _attemptsLeft = maxAttempts;

    void Start()
    {
        if (confirmBtn != null) { confirmBtn.onClick.RemoveAllListeners(); confirmBtn.onClick.AddListener(OnConfirm); }
        if (cancelBtn != null) { cancelBtn.onClick.RemoveAllListeners(); cancelBtn.onClick.AddListener(OnCancel); }
        UpdateAttemptsText();
    }

    void OnEnable()
    {
        if (passwordInput != null) passwordInput.text = "";
        UpdateAttemptsText();
    }

    void OnConfirm()
    {
        if (passwordInput == null) { Debug.LogError("AdminPasswordController: passwordInput not assigned"); return; }

        string entered = passwordInput.text ?? "";
        if (entered == expectedPassword)
        {
            if (GameState.Instance != null) GameState.Instance.IsAdminMode = true;
            _attemptsLeft = maxAttempts;
            UpdateAttemptsText();

            if (UIManager.Instance != null) UIManager.Instance.OnAdminAuthenticated();
            else
            {
                if (roleSelectionPanel != null) roleSelectionPanel.SetActive(false);
            }
            return;
        }

        _attemptsLeft--;
        UpdateAttemptsText();

        if (_attemptsLeft <= 0) { Debug.LogWarning("AdminPasswordController: attempts exhausted"); Application.Quit(); return; }

        passwordInput.text = "";
        passwordInput.Select();
        passwordInput.ActivateInputField();
    }

    void OnCancel()
    {
        if (UIManager.Instance != null && roleSelectionPanel != null) UIManager.Instance.ShowOnly(roleSelectionPanel);
        else if (roleSelectionPanel != null) { roleSelectionPanel.SetActive(true); gameObject.SetActive(false); }
    }

    void UpdateAttemptsText()
    {
        if (attemptsText != null) attemptsText.text = $"Осталось попыток: {_attemptsLeft}";
    }
}
