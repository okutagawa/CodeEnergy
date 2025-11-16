using UnityEngine;
using UnityEngine.UI;
using MyGame.Models;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    public GameObject panel;
    public Text titleText;
    public Text bodyText;
    public Button closeButton;

    // Чтобы восстановить предыдущее состояние курсора
    private CursorLockMode prevLockState;
    private bool prevVisible;

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(this);
        if (panel != null) panel.SetActive(false);
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
    }

    public void Show(string title, string text)
    {
        if (panel == null) return;
        if (titleText != null) titleText.text = title ?? "";
        if (bodyText != null) bodyText.text = text ?? "";

        // сохранить старое состояние курсора
        prevLockState = Cursor.lockState;
        prevVisible = Cursor.visible;

        // показать панель и курсор
        panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowForTask(TaskModel task, bool forGiver, string npcDisplayName)
    {
        if (task == null) { Show(npcDisplayName, "(no task)"); return; }
        var t = forGiver ? task.textForGiver : task.textForReceiver;
        Show(npcDisplayName, string.IsNullOrEmpty(t) ? "(empty dialog)" : t);
    }

    public void Hide()
    {
        if (panel == null) return;
        panel.SetActive(false);
        // восстановить прежнее состояние курсора
        Cursor.lockState = prevLockState;
        Cursor.visible = prevVisible;
    }
}
