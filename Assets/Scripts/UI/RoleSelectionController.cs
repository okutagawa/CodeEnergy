using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// экран выбора роли (Student / Admin) и логика входа в админ‑режим (пароль)
public class RoleSelectionController : MonoBehaviour
{
    public Button btnPlayer;
    public Button btnAdmin;
    public Button btnExit; // optional
    public GameObject adminPasswordPanel; // ссылка на панель ввода пароля

    void Start()
    {
        if (btnPlayer != null) btnPlayer.onClick.RemoveAllListeners();
        if (btnAdmin != null) btnAdmin.onClick.RemoveAllListeners();
        if (btnExit != null) btnExit.onClick.RemoveAllListeners();

        if (btnPlayer != null) btnPlayer.onClick.AddListener(OnPlayerClicked);
        if (btnAdmin != null) btnAdmin.onClick.AddListener(OnAdminClicked);
        if (btnExit != null) btnExit.onClick.AddListener(OnExitClicked);
    }

    void OnPlayerClicked()
    {
        // Загружает сцену игры по имени; убедись, что GameScene добавлена в Build Settings
        SceneManager.LoadScene("GameScene");
    }

    void OnAdminClicked()
    {
        // показываем панель ввода пароля через UIManager если он есть
        if (UIManager.Instance != null && adminPasswordPanel != null)
            UIManager.Instance.ShowOnly(adminPasswordPanel);
        else if (adminPasswordPanel != null)
            adminPasswordPanel.SetActive(true);
    }

    void OnExitClicked()
    {
        Application.Quit();
    }
}
