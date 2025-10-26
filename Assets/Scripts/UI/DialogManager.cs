using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [Header("UI")]
    public GameObject dialogPanel;
    public Text titleText;
    public Text bodyText;
    public Button closeButton;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        dialogPanel.SetActive(false);
        closeButton.onClick.AddListener(CloseDialog);
    }

    public void OpenDialog(string title, string body, Action onClose = null)
    {
        titleText.text = title;
        bodyText.text = body;
        dialogPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        onDialogClosed = onClose;
    }

    private Action onDialogClosed;

    public void CloseDialog()
    {
        dialogPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        onDialogClosed?.Invoke();
        onDialogClosed = null;
    }
}
