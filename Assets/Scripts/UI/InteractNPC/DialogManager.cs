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

    private Action onDialogClosed;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(CloseDialog);
    }

    public void OpenDialog(string title, string body, Action onClose = null)
    {
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;

        if (dialogPanel != null) dialogPanel.SetActive(true);

        // ¬место пр€мого управлени€ курсором Ч через CursorUIManager
        CursorUIManager.Instance?.ShowCursor();

        onDialogClosed = onClose;
    }

    public void CloseDialog()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);

        // —нимаем запрос на курсор
        CursorUIManager.Instance?.HideCursor();

        onDialogClosed?.Invoke();
        onDialogClosed = null;
    }
}
