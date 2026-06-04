using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalTestResultPanelController : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text valueResult;
    [SerializeField] private Text valueRequired;
    [SerializeField] private Text valueGrade;
    [SerializeField] private Text portalStatusText;
    [SerializeField] private Text hintText;

    [Header("Buttons")]
    [SerializeField] private Button buttonRetry;
    [SerializeField] private Button buttonFinish;
    [SerializeField] private Button buttonClose;

    [Header("Navigation")]
    [SerializeField] private string menuSceneName = "MenuScene";

    private FinalTestController _controller;

    private void Awake()
    {
        TryAutoAssignRefs();
        if (buttonRetry != null) buttonRetry.onClick.AddListener(HandleRetry);
        if (buttonFinish != null) buttonFinish.onClick.AddListener(HandleFinish);
        if (buttonClose != null) buttonClose.onClick.AddListener(HandleClose);
    }

    private void OnDestroy()
    {
        if (buttonRetry != null) buttonRetry.onClick.RemoveListener(HandleRetry);
        if (buttonFinish != null) buttonFinish.onClick.RemoveListener(HandleFinish);
        if (buttonClose != null) buttonClose.onClick.RemoveListener(HandleClose);
    }

    public void Show(FinalTestController controller, int correctAnswers, int totalQuestions, int requiredCorrectAnswers, bool passed)
    {
        _controller = controller;
        TryAutoAssignRefs();

        if (titleText != null) titleText.text = passed ? "ФИНАЛЬНЫЙ ТЕСТ ПРОЙДЕН" : "ФИНАЛЬНЫЙ ТЕСТ НЕ ПРОЙДЕН";
        if (valueResult != null) valueResult.text = $"{correctAnswers}/{totalQuestions}";
        if (valueRequired != null) valueRequired.text = requiredCorrectAnswers.ToString();
        if (valueGrade != null) valueGrade.text = BuildGrade(correctAnswers, totalQuestions);
        if (portalStatusText != null) portalStatusText.text = passed ? "Портал активирован" : "Портал не активирован";
        if (hintText != null) hintText.text = passed ? "Вы можете перейти на следующий остров." : "Необходимо пройти тест повторно.";

        if (buttonFinish != null) buttonFinish.gameObject.SetActive(passed);
        if (buttonRetry != null) buttonRetry.gameObject.SetActive(!passed);
        gameObject.SetActive(true);
    }

    private static string BuildGrade(int correctAnswers, int totalQuestions)
    {
        if (totalQuestions <= 0) return "0%";
        return Mathf.RoundToInt((float)correctAnswers / totalQuestions * 100f) + "%";
    }

    private void HandleRetry()
    {
        gameObject.SetActive(false);
        _controller?.RetryFinalTest();
    }

    private void HandleFinish()
    {
        gameObject.SetActive(false);

        if (string.IsNullOrWhiteSpace(menuSceneName))
        {
            Debug.LogError("[FinalTestResult] Menu scene name is empty. Cannot finish final test.");
            return;
        }

        SceneManager.LoadScene(menuSceneName);
    }

    private void HandleClose()
    {
        gameObject.SetActive(false);
    }

    private void TryAutoAssignRefs()
    {
        titleText = titleText != null ? titleText : FindText("TitleText");
        valueResult = valueResult != null ? valueResult : FindText("ValueResult");
        valueRequired = valueRequired != null ? valueRequired : FindText("ValueRequired");
        valueGrade = valueGrade != null ? valueGrade : FindText("ValueGrade");
        portalStatusText = portalStatusText != null ? portalStatusText : FindText("PortalStatusText");
        hintText = hintText != null ? hintText : FindText("HintText");
        buttonRetry = buttonRetry != null ? buttonRetry : FindButton("ButtonRetry");
        buttonFinish = buttonFinish != null ? buttonFinish : FindButton("ButtonFinish");
        buttonClose = buttonClose != null ? buttonClose : FindButton("ButtonClose");
    }

    private Text FindText(string objectName) => FindComponentByName<Text>(objectName);
    private Button FindButton(string objectName) => FindComponentByName<Button>(objectName);

    private T FindComponentByName<T>(string objectName) where T : Component
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == objectName)
                return t.GetComponent<T>();
        }
        return null;
    }
}