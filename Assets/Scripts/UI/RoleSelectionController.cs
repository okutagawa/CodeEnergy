using UnityEngine;
using UnityEngine.UI;

// экран выбора роли (Student / Admin) и логика входа в админ‑режим (пароль)
public class RoleSelectionController : MonoBehaviour
{
    public Button btnStudent;
    public Button btnAdmin;
    public GameObject adminPasswordPanel;
    public InputField passwordInput;
    public Button adminConfirm;
    public Button adminCancel;
    private const string AdminPassword = "admin123";

    void Start()
    {
        btnStudent.onClick.AddListener(() => UIManager.Instance.ShowProfileSelection());
        btnAdmin.onClick.AddListener(ShowPassword);
        adminConfirm.onClick.AddListener(OnAdminConfirm);
        adminCancel.onClick.AddListener(() => adminPasswordPanel.SetActive(false));
        adminPasswordPanel.SetActive(false);
    }

    void ShowPassword()
    {
        adminPasswordPanel.SetActive(true);
        passwordInput.text = "";
    }

    void OnAdminConfirm()
    {
        if (passwordInput.text == AdminPassword)
        {
            adminPasswordPanel.SetActive(false);
            UIManager.Instance.ShowAdminPanel();
        }
        else
        {
            Debug.Log("Wrong admin password");
        }
    }
}
