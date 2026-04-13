using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Button btnResume;
    public Button btnSettings;
    public Button btnQuitToMenu;
    public GameObject settingsPanelPrefab;
    public Transform modalContainer;
    public GameObject firstSelectedOnPause;

    [Header("Behavior")]
    public string menuSceneName = "MenuScene";
    public bool closePauseWhenOpenSettings = true;

    // internal
    private bool isPaused = false;
    private bool pauseUiFocusTaken = false;
    private GameObject settingsInstance;

    void Awake()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        if (btnResume != null) btnResume.onClick.AddListener(Resume);
        if (btnSettings != null) btnSettings.onClick.AddListener(OpenSettings);
        if (btnQuitToMenu != null) btnQuitToMenu.onClick.AddListener(QuitToMenu);
    }

    void Update()
    {
        // Обработка Esc (работает в старой и новой системе ввода)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Если открыт Settings, закрываем его первым
            if (settingsInstance != null && settingsInstance.activeSelf)
            {
                CloseSettings();
                return;
            }

            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;

        // Показать UI
        if (pausePanel != null) pausePanel.SetActive(true);

        // Остановить время
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // Отключить ввод игрока — через событие или интерфейс
        PauseBroadcast(true);

        if (!pauseUiFocusTaken)
        {
            CursorUIManager.Instance?.EnterUiFocus();
            pauseUiFocusTaken = true;
        }

        // Установить фокус для контроллера/клавиатуры
        if (firstSelectedOnPause != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelectedOnPause);
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        CloseSettings();
        // Скрыть UI
        if (pausePanel != null) pausePanel.SetActive(false);

        // Восстановить время
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Включить ввод игрока
        PauseBroadcast(false);

        if (pauseUiFocusTaken)
        {
            CursorUIManager.Instance?.ExitUiFocus();
            pauseUiFocusTaken = false;
        }

        // Очистить фокус
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenSettings()
    {
        if (!isPaused)
            Pause();

        // Если префаб задан — инстанцируем, иначе предполагаем, что SettingsPanel уже в сцене и активируем её
        if (settingsInstance == null && settingsPanelPrefab != null)
        {
            Transform parent = modalContainer != null ? modalContainer : transform.parent;
            settingsInstance = Instantiate(settingsPanelPrefab, parent);
        }
        else if (settingsInstance == null)
        {
            // Попытка найти SettingsPanel в сцене
            var existing = GameObject.FindObjectOfType<SettingsController>(true);
            if (existing != null) settingsInstance = existing.gameObject;
        }

        if (settingsInstance == null)
            return;

        settingsInstance.SetActive(true);

        if (closePauseWhenOpenSettings && pausePanel != null)
            pausePanel.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseSettings()
    {
        if (settingsInstance == null) return;

        // Если это инстанс префаба — уничтожаем, иначе просто деактивируем
        if (settingsPanelPrefab != null && settingsInstance.scene == gameObject.scene)
            // Если это префаб-инстанс (созданный нами), уничтожаем
            Destroy(settingsInstance);
        else
            settingsInstance.SetActive(false);

        settingsInstance = null;

        if (isPaused && pausePanel != null)
        {
            pausePanel.SetActive(true);
            if (firstSelectedOnPause != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstSelectedOnPause);
        }
    }

    public void QuitToMenu()
    {
        // Важно: перед сменой сцены восстановить Time.timeScale
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // (Опционально) Сохранить прогресс перед выходом
        // SaveService.SaveGameState(...);

        SceneManager.LoadScene(menuSceneName);
    }

    private void PauseBroadcast(bool paused)
    {
        // Простая реализация: посылаем сообщение всем GameObject'ам
        // Лучше: использовать интерфейс IPausable (см. ниже) или событие
        var receivers = FindObjectsOfType<MonoBehaviour>();
        foreach (var r in receivers)  
            // Попытка вызвать метод OnGamePaused(bool)
            r.SendMessage("OnGamePaused", paused, SendMessageOptions.DontRequireReceiver);
        
    }
}
