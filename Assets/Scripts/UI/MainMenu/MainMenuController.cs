using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button btnPlayer;
    public Button btnAdmin;
    public Button btnExit;
    public GameObject adminPasswordPanel;

    private void Start()
    {
        if (btnPlayer != null)
        {
            btnPlayer.onClick.RemoveAllListeners();
            btnPlayer.onClick.AddListener(OnPlayerClicked);
        }

        if (btnAdmin != null)
        {
            btnAdmin.onClick.RemoveAllListeners();
            btnAdmin.onClick.AddListener(OnAdminClicked);
        }

        if (btnExit != null)
        {
            btnExit.onClick.RemoveAllListeners();
            btnExit.onClick.AddListener(OnExitClicked);
        }
    }

    private void OnPlayerClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnAdminClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowAdminPassword();
        }
        else if (adminPasswordPanel != null)
        {
            adminPasswordPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    private void OnExitClicked()
    {
        Application.Quit();
    }
}