using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class AdminPasswordControllerTests
{
    private GameObject _controllerObject;
    private AdminPasswordController _controller;

    private GameObject _gameStateObject;
    private GameState _gameState;

    private InputField _passwordInput;
    private Button _confirmButton;
    private Button _cancelButton;
    private Text _attemptsText;
    private GameObject _mainMenuRoot;

    [SetUp]
    public void SetUp()
    {
        CleanupGameStateObjects();
        ResetGameStateInstance();

        _gameStateObject = new GameObject("GameState_TestObject");
        _gameState = _gameStateObject.AddComponent<GameState>();
        SetGameStateInstance(_gameState);

        _controllerObject = new GameObject("AdminPasswordController_TestObject");
        _controller = _controllerObject.AddComponent<AdminPasswordController>();

        _passwordInput = CreateInputField("PasswordInput");
        _confirmButton = CreateButton("ConfirmButton");
        _cancelButton = CreateButton("CancelButton");
        _attemptsText = CreateText("AttemptsText");

        _mainMenuRoot = new GameObject("MainMenuRoot");
        _mainMenuRoot.SetActive(false);

        _controller.passwordInput = _passwordInput;
        _controller.confirmBtn = _confirmButton;
        _controller.cancelBtn = _cancelButton;
        _controller.attemptsText = _attemptsText;
        _controller.mainMenuRoot = _mainMenuRoot;

        _controller.expectedPassword = "admin";
        _controller.maxAttempts = 3;

        InvokePrivateMethod(_controller, "Awake");
        InvokePrivateMethod(_controller, "Start");
        InvokePrivateMethod(_controller, "OnEnable");
    }

    [TearDown]
    public void TearDown()
    {
        if (_controllerObject != null)
        {
            Object.DestroyImmediate(_controllerObject);
        }

        if (_passwordInput != null)
        {
            Object.DestroyImmediate(_passwordInput.gameObject);
        }

        if (_confirmButton != null)
        {
            Object.DestroyImmediate(_confirmButton.gameObject);
        }

        if (_cancelButton != null)
        {
            Object.DestroyImmediate(_cancelButton.gameObject);
        }

        if (_attemptsText != null)
        {
            Object.DestroyImmediate(_attemptsText.gameObject);
        }

        if (_mainMenuRoot != null)
        {
            Object.DestroyImmediate(_mainMenuRoot);
        }

        if (_gameStateObject != null)
        {
            Object.DestroyImmediate(_gameStateObject);
        }

        CleanupGameStateObjects();
        ResetGameStateInstance();
    }

    [Test]
    public void OnEnable_WhenPanelOpened_ShouldClearPasswordInput()
    {
        _passwordInput.text = "some_password";

        InvokePrivateMethod(_controller, "OnEnable");

        Assert.AreEqual(string.Empty, _passwordInput.text);
    }

    [Test]
    public void OnEnable_WhenPanelOpened_ShouldShowMaxAttempts()
    {
        InvokePrivateMethod(_controller, "OnEnable");

        Assert.AreEqual("3", _attemptsText.text);
    }

    [Test]
    public void OnConfirm_WhenPasswordIsCorrect_ShouldEnableAdminMode()
    {
        _passwordInput.text = "admin";

        InvokePrivateMethod(_controller, "OnConfirm");

        Assert.IsNotNull(GameState.Instance);
        Assert.IsTrue(GameState.Instance.IsAdminMode);
    }

    [Test]
    public void OnConfirm_WhenPasswordIsCorrect_ShouldDisablePasswordPanel()
    {
        _passwordInput.text = "admin";

        InvokePrivateMethod(_controller, "OnConfirm");

        Assert.IsFalse(_controllerObject.activeSelf);
    }

    [Test]
    public void OnConfirm_WhenPasswordIsIncorrect_ShouldDecreaseAttempts()
    {
        _passwordInput.text = "wrong_password";

        InvokePrivateMethod(_controller, "OnConfirm");

        Assert.AreEqual("2", _attemptsText.text);
    }

    [Test]
    public void OnConfirm_WhenPasswordIsIncorrect_ShouldClearPasswordInput()
    {
        _passwordInput.text = "wrong_password";

        InvokePrivateMethod(_controller, "OnConfirm");

        Assert.AreEqual(string.Empty, _passwordInput.text);
    }

    [Test]
    public void OnConfirm_WhenPasswordIsEmpty_ShouldDecreaseAttempts()
    {
        _passwordInput.text = string.Empty;

        InvokePrivateMethod(_controller, "OnConfirm");

        Assert.AreEqual("2", _attemptsText.text);
    }

    [Test]
    public void OnConfirm_WhenPasswordIsIncorrectTwice_ShouldShowOneAttemptLeft()
    {
        _passwordInput.text = "wrong_password";
        InvokePrivateMethod(_controller, "OnConfirm");

        _passwordInput.text = "wrong_password";
        InvokePrivateMethod(_controller, "OnConfirm");

        Assert.AreEqual("1", _attemptsText.text);
    }

    [Test]
    public void OnConfirm_WhenPasswordIsCorrectAfterWrongAttempt_ShouldResetAttempts()
    {
        _passwordInput.text = "wrong_password";
        InvokePrivateMethod(_controller, "OnConfirm");

        _passwordInput.text = "admin";
        InvokePrivateMethod(_controller, "OnConfirm");

        Assert.AreEqual("3", _attemptsText.text);
    }

    [Test]
    public void OnCancel_WhenMainMenuExists_ShouldShowMainMenuAndHidePasswordPanel()
    {
        Assert.IsTrue(_controllerObject.activeSelf);
        Assert.IsFalse(_mainMenuRoot.activeSelf);

        InvokePrivateMethod(_controller, "OnCancel");

        Assert.IsTrue(_mainMenuRoot.activeSelf);
        Assert.IsFalse(_controllerObject.activeSelf);
    }

    [Test]
    public void ConfirmButton_WhenClickedWithCorrectPassword_ShouldEnableAdminMode()
    {
        _passwordInput.text = "admin";

        _confirmButton.onClick.Invoke();

        Assert.IsNotNull(GameState.Instance);
        Assert.IsTrue(GameState.Instance.IsAdminMode);
    }

    [Test]
    public void CancelButton_WhenClicked_ShouldHidePasswordPanel()
    {
        _cancelButton.onClick.Invoke();

        Assert.IsFalse(_controllerObject.activeSelf);
    }

    private static InputField CreateInputField(string name)
    {
        var inputObject = new GameObject(name);
        inputObject.AddComponent<RectTransform>();

        var inputField = inputObject.AddComponent<InputField>();

        var textObject = new GameObject(name + "_Text");
        textObject.transform.SetParent(inputObject.transform);

        var text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        inputField.textComponent = text;

        return inputField;
    }

    private static Button CreateButton(string name)
    {
        var buttonObject = new GameObject(name);
        buttonObject.AddComponent<RectTransform>();
        buttonObject.AddComponent<Image>();

        return buttonObject.AddComponent<Button>();
    }

    private static Text CreateText(string name)
    {
        var textObject = new GameObject(name);
        textObject.AddComponent<RectTransform>();

        var text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return text;
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        var method = target
            .GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(method, $"Метод {methodName} не найден.");

        method.Invoke(target, null);
    }

    private static void CleanupGameStateObjects()
    {
        var gameStates = Object.FindObjectsOfType<GameState>();

        foreach (var gameState in gameStates)
        {
            Object.DestroyImmediate(gameState.gameObject);
        }
    }

    private static void ResetGameStateInstance()
    {
        var backingField = typeof(GameState).GetField(
            "<Instance>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        if (backingField != null)
        {
            backingField.SetValue(null, null);
        }
    }

    private static void SetGameStateInstance(GameState gameState)
    {
        var backingField = typeof(GameState).GetField(
            "<Instance>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        Assert.IsNotNull(backingField, "Поле GameState.Instance не найдено.");

        backingField.SetValue(null, gameState);
    }
}