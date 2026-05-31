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

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(this); return; }
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

        panel.SetActive(true);
    }

    public void ShowForTask(TaskModel task, bool forGiver, string npcDisplayName)
    {
        if (!forGiver)
        {
            Debug.Log($"DialogueUI: receiver flow handled by QuizPanel. Skipping DialogueUI for {npcDisplayName}.");
            return;
        }

        if (task == null)
        {
            Show(npcDisplayName, "(no task)");
            return;
        }

        var t = task.textForGiver;
        Show(npcDisplayName, string.IsNullOrEmpty(t) ? "(empty dialog)" : t);
    }

    public void Hide()
    {
        if (panel == null) return;
        panel.SetActive(false);
    }

    //void OnDisable()
    //{
    //    // Страховка на случай скрытия панели без вызова Hide()
    //    if (CursorUIManager.Instance != null && CursorUIManager.Instance.IsUiFocused)
    //        CursorUIManager.Instance.ExitUiFocus();
    //}

    //void OnDestroy()
    //{
    //    if (CursorUIManager.Instance != null && CursorUIManager.Instance.IsUiFocused)
    //        CursorUIManager.Instance.ExitUiFocus();
    //}
}
