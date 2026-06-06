using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Main menu buttons")]
    public Button btnStartGame;
    public Button btnContinue;
    public Button btnSettings;
    public Button btnExit;

    public Button btnAdmin;

    [Header("Panels")]
    public CourseSelectPanelController courseSelectPanel;

    private const string GameSceneName = "GameScene";

    private void Start()
    {
        GameState.EnsureExists();
        RecoverMenuInteractionState();

        BindButton(btnStartGame, OnStartGameClicked);
        BindButton(btnContinue, OnContinueClicked);
        BindButton(btnSettings, OnSettingsClicked);
        BindButton(btnExit, OnExitClicked);
        BindButton(btnAdmin, OnAdminClicked);

        RefreshContinueButtonState();
    }

    private static void RecoverMenuInteractionState()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    public void OnStartGameClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowCourseSelectPanel();
            return;
        }

        var panel = courseSelectPanel != null
            ? courseSelectPanel
            : FindObjectOfType<CourseSelectPanelController>(true);

        if (panel != null)
        {
            panel.OpenPanel();
            return;
        }

        Debug.LogWarning("[MainMenu] Course select panel was not found. Starting a new game without course selection.");
        StartNewGameWithoutCourse();
    }

    private static void StartNewGameWithoutCourse()
    {
        SaveManager.Delete();
        GameState.EnsureExists();

        if (GameState.Instance != null)
        {
            GameState.Instance.ApplyData(new GameStateData());
            GameState.Instance.IsAdminMode = false;
            GameState.Instance.SaveState();
        }

        SceneManager.LoadScene(GameSceneName);
    }

    public void OnContinueClicked()
    {
        if (!HasSaveFile())
        {
            ShowError("Сохранение не найдено. Начните новую игру.");
            Debug.LogWarning("[MainMenu] Continue pressed, but save file was not found.");
            RefreshContinueButtonState();
            return;
        }

        if (GameState.Instance != null)
        {
            GameState.Instance.LoadState();
            GameState.Instance.IsAdminMode = false;
        }

        SceneManager.LoadScene(GameSceneName);
    }

    public void OnSettingsClicked()
    {
        Debug.Log("[MainMenu] Settings button clicked.");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSettingsPanel();
            return;
        }

        var settingsController = FindObjectOfType<SettingsController>(true);
        if (settingsController != null)
        {
            settingsController.OpenSettings();
            return;
        }
        ShowError("Не удалось открыть настройки. Панель настроек не найдена.");
        Debug.LogWarning("[MainMenu] Settings panel/controller not found in scene.");
    }

    public void OnAdminClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowAdminPassword();
            return;
        }
        ShowError("Не удалось открыть вход администратора. Менеджер интерфейса не найден.");
        Debug.LogWarning("[MainMenu] UIManager.Instance is null. Cannot open admin password panel.");
    }

    private static void ShowError(string message)
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowError(message);
        else ErrorPopupController.Show(message);
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }

    private void RefreshContinueButtonState()
    {
        if (btnContinue != null)
            btnContinue.interactable = HasSaveFile();
    }

    private bool HasSaveFile()
    {
        var path = SaveService.GetPath(SaveService.GameStateFileName);
        if (!File.Exists(path)) return false;

        try
        {
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return false;

            var validation = SaveService.ValidateGameStateJson(json);
            if (!validation.ok) return false;

            var data = JsonUtility.FromJson<GameStateData>(json);
            data?.Normalize();
            return data != null && data.selectedCourseId > 0;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MainMenu] Save file is not readable: {ex.Message}");
            return false;
        }
    }
}