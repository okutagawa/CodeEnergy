using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoleSelectionController : MonoBehaviour
{
    public Button btnPlayer;
    public Button btnAdmin;
    public Button btnExit;
    public GameObject adminPasswordPanel;

    void Start()
    {
        if (btnPlayer != null) { btnPlayer.onClick.RemoveAllListeners(); btnPlayer.onClick.AddListener(OnPlayerClicked); }
        if (btnAdmin != null) { btnAdmin.onClick.RemoveAllListeners(); btnAdmin.onClick.AddListener(OnAdminClicked); }
        if (btnExit != null) { btnExit.onClick.RemoveAllListeners(); btnExit.onClick.AddListener(OnExitClicked); }
    }

    void OnPlayerClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    void OnAdminClicked()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowAdminPassword();
        else if (adminPasswordPanel != null) adminPasswordPanel.SetActive(true);
    }

    void OnExitClicked()
    {
        Application.Quit();
    }
}
