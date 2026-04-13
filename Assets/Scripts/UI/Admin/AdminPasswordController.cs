using UnityEngine;
using UnityEngine.UI;

public class AdminPasswordController : MonoBehaviour
{
    [Header("UI References")]
    public InputField passwordInput;
    public Button confirmBtn;
    public Button cancelBtn;
    public Text attemptsText;
    public GameObject mainMenuRoot;

    [Header("Settings")]
    public string expectedPassword = "admin";
    public int maxAttempts = 3;

    private int _attemptsLeft;

    private void Awake()
    {
        _attemptsLeft = maxAttempts;
    }

    private void Start()
    {
        if (confirmBtn != null)
        {
            confirmBtn.onClick.RemoveAllListeners();
            confirmBtn.onClick.AddListener(OnConfirm);
        }

        if (cancelBtn != null)
        {
            cancelBtn.onClick.RemoveAllListeners();
            cancelBtn.onClick.AddListener(OnCancel);
        }
        UpdateAttemptsText();
    }

    private void OnEnable()
    {
        if (passwordInput != null) passwordInput.text = "";
        UpdateAttemptsText();
    }

    private void OnConfirm()
    {
        if (passwordInput == null)
        {
            Debug.LogError("AdminPasswordController: passwordInput not assigned");
            return;
        }

        var entered = passwordInput.text ?? string.Empty;
        if (entered == expectedPassword)
        {
            GameState.EnsureExists();
            if (GameState.Instance != null) GameState.Instance.IsAdminMode = true;
            _attemptsLeft = maxAttempts;
            UpdateAttemptsText();

            if (UIManager.Instance != null) UIManager.Instance.OnAdminAuthenticated();
            else gameObject.SetActive(false);
            return;
        }

        _attemptsLeft--;
        UpdateAttemptsText();

        if (_attemptsLeft <= 0)
        {
            Debug.LogWarning("AdminPasswordController: attempts exhausted");
            Application.Quit();
            return;
        }

        passwordInput.text = "";
        passwordInput.Select();
        passwordInput.ActivateInputField();
    }

    private void OnCancel()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideAdminPassword();
        }
        else if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    private void UpdateAttemptsText()
    {
        if (attemptsText != null) attemptsText.text = $"Attempts left: {_attemptsLeft}";
    }
}
