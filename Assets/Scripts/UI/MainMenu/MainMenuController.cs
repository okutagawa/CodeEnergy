using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Main menu buttons")]
    public Button btnStartGame;
    public Button btnContinue;
    public Button btnSettings;
    public Button btnExit;

    public Button btnAdmin;

    private const string GameSceneName = "GameScene";
    private const string GameStateFileName = "gamestate.json";

    private void Start()
    {
        BindButton(btnStartGame, OnStartGameClicked);
        BindButton(btnContinue, OnContinueClicked);
        BindButton(btnSettings, OnSettingsClicked);
        BindButton(btnExit, OnExitClicked);
        BindButton(btnAdmin, OnAdminClicked);

        RefreshContinueButtonState();
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    public void OnStartGameClicked()
    {
        SaveManager.Delete();

        if (GameState.Instance != null)
        {
            GameState.Instance.ApplyData(new GameStateData());
            GameState.Instance.IsAdminMode = false;
        }

        SceneManager.LoadScene(GameSceneName);
    }

    public void OnContinueClicked()
    {
        if (!HasSaveFile())
        {
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

        Debug.LogWarning("[MainMenu] Settings panel/controller not found in scene.");
    }

    public void OnAdminClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowAdminPassword();
            return;
        }
        Debug.LogWarning("[MainMenu] UIManager.Instance is null. Cannot open admin password panel.");
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
        var filePath = Path.Combine(Application.persistentDataPath, GameStateFileName);
        return File.Exists(filePath);
    }
}