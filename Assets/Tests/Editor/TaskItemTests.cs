using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using MyGame.Models;

public class TaskItemTests
{
    private GameObject _taskItemObject;
    private TaskItem _taskItem;
    private Text _titleText;
    private Button _buttonRoot;
    private Image _image;

    [SetUp]
    public void SetUp()
    {
        _taskItemObject = new GameObject("TaskItem_TestObject");
        _taskItemObject.AddComponent<RectTransform>();
        _image = _taskItemObject.AddComponent<Image>();

        _taskItem = _taskItemObject.AddComponent<TaskItem>();

        var titleObject = new GameObject("TitleText");
        titleObject.transform.SetParent(_taskItemObject.transform);
        titleObject.AddComponent<RectTransform>();

        _titleText = titleObject.AddComponent<Text>();
        _titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var buttonObject = new GameObject("ButtonRoot");
        buttonObject.transform.SetParent(_taskItemObject.transform);
        buttonObject.AddComponent<RectTransform>();
        buttonObject.AddComponent<Image>();

        _buttonRoot = buttonObject.AddComponent<Button>();

        _taskItem.textTitle = _titleText;
        _taskItem.buttonRoot = _buttonRoot;
    }

    [TearDown]
    public void TearDown()
    {
        if (_taskItemObject != null)
        {
            Object.DestroyImmediate(_taskItemObject);
        }
    }

    [Test]
    public void Initialize_WhenTaskHasTitle_ShouldDisplayTaskTitle()
    {
        var model = CreateTaskModel(1, "Первые строки");

        _taskItem.Initialize(model);

        Assert.AreEqual("Первые строки", _titleText.text);
    }

    [Test]
    public void Initialize_WhenTaskTitleIsEmpty_ShouldDisplayEmptyPlaceholder()
    {
        var model = CreateTaskModel(2, string.Empty);

        _taskItem.Initialize(model);

        Assert.AreEqual("(empty)", _titleText.text);
    }

    [Test]
    public void Initialize_WhenTaskTitleIsNull_ShouldDisplayEmptyPlaceholder()
    {
        var model = CreateTaskModel(3, null);

        _taskItem.Initialize(model);

        Assert.AreEqual("(empty)", _titleText.text);
    }

    [Test]
    public void Initialize_WhenCalled_ShouldStoreTaskModel()
    {
        var model = CreateTaskModel(4, "Чистота кода");

        _taskItem.Initialize(model);

        var storedModel = GetPrivateField<TaskModel>(_taskItem, "data");

        Assert.IsNotNull(storedModel);
        Assert.AreEqual(4, storedModel.id);
        Assert.AreEqual("Чистота кода", storedModel.title);
    }

    [Test]
    public void Initialize_WhenCalled_ShouldBindButtonClick()
    {
        var model = CreateTaskModel(5, "Ключ от двери");
        var wasClicked = false;

        _taskItem.onSingleClick = task =>
        {
            wasClicked = true;
        };

        _taskItem.Initialize(model);
        PrepareSingleClick();

        _buttonRoot.onClick.Invoke();

        Assert.IsTrue(wasClicked);
    }

    [Test]
    public void ButtonClick_WhenClickedOnce_ShouldInvokeSingleClick()
    {
        var model = CreateTaskModel(6, "Одиночный клик");
        var singleClickCount = 0;

        _taskItem.onSingleClick = task =>
        {
            singleClickCount++;
        };

        _taskItem.Initialize(model);
        PrepareSingleClick();

        _buttonRoot.onClick.Invoke();

        Assert.AreEqual(1, singleClickCount);
    }

    [Test]
    public void ButtonClick_WhenClickedOnce_ShouldPassCorrectTaskModel()
    {
        var model = CreateTaskModel(7, "Передача модели");
        TaskModel receivedModel = null;

        _taskItem.onSingleClick = task =>
        {
            receivedModel = task;
        };

        _taskItem.Initialize(model);
        PrepareSingleClick();

        _buttonRoot.onClick.Invoke();

        Assert.IsNotNull(receivedModel);
        Assert.AreEqual(7, receivedModel.id);
        Assert.AreEqual("Передача модели", receivedModel.title);
    }

    [Test]
    public void ButtonClick_WhenClickedTwiceQuickly_ShouldInvokeDoubleClick()
    {
        var model = CreateTaskModel(8, "Двойной клик");
        var doubleClickCount = 0;

        _taskItem.onDoubleClick = task =>
        {
            doubleClickCount++;
        };

        _taskItem.Initialize(model);
        PrepareSingleClick();

        _buttonRoot.onClick.Invoke();
        _buttonRoot.onClick.Invoke();

        Assert.AreEqual(1, doubleClickCount);
    }

    [Test]
    public void ButtonClick_WhenClickedTwiceQuickly_ShouldPassCorrectTaskModelToDoubleClick()
    {
        var model = CreateTaskModel(9, "Открытие задания");
        TaskModel receivedModel = null;

        _taskItem.onDoubleClick = task =>
        {
            receivedModel = task;
        };

        _taskItem.Initialize(model);
        PrepareSingleClick();

        _buttonRoot.onClick.Invoke();
        _buttonRoot.onClick.Invoke();

        Assert.IsNotNull(receivedModel);
        Assert.AreEqual(9, receivedModel.id);
        Assert.AreEqual("Открытие задания", receivedModel.title);
    }

    [Test]
    public void DoubleClick_WhenTriggered_ShouldResetLastClick()
    {
        var model = CreateTaskModel(10, "Сброс клика");

        _taskItem.Initialize(model);
        PrepareSingleClick();

        _buttonRoot.onClick.Invoke();
        _buttonRoot.onClick.Invoke();

        var lastClick = GetPrivateField<float>(_taskItem, "lastClick");

        Assert.AreEqual(0f, lastClick);
    }

    [Test]
    public void SetSelected_WhenTrue_ShouldChangeImageColor()
    {
        _taskItem.SetSelected(true);

        Assert.AreEqual(new Color(0.85f, 0.95f, 1f), _image.color);
    }

    [Test]
    public void SetSelected_WhenFalse_ShouldSetImageColorToWhite()
    {
        _taskItem.SetSelected(false);

        Assert.AreEqual(Color.white, _image.color);
    }

    [Test]
    public void SetSelected_WhenImageIsMissing_ShouldNotThrowException()
    {
        Object.DestroyImmediate(_image);

        Assert.DoesNotThrow(() => _taskItem.SetSelected(true));
    }

    [Test]
    public void UpdateTitle_WhenTitleIsValid_ShouldUpdateDisplayedTitle()
    {
        _taskItem.UpdateTitle("Новое название задания");

        Assert.AreEqual("Новое название задания", _titleText.text);
    }

    [Test]
    public void UpdateTitle_WhenTitleIsEmpty_ShouldDisplayEmptyPlaceholder()
    {
        _taskItem.UpdateTitle(string.Empty);

        Assert.AreEqual("(empty)", _titleText.text);
    }

    [Test]
    public void UpdateTitle_WhenTitleIsNull_ShouldDisplayEmptyPlaceholder()
    {
        _taskItem.UpdateTitle(null);

        Assert.AreEqual("(empty)", _titleText.text);
    }

    [Test]
    public void Initialize_WhenCalledTwice_ShouldReplacePreviousButtonListeners()
    {
        var firstModel = CreateTaskModel(11, "Первое задание");
        var secondModel = CreateTaskModel(12, "Второе задание");

        TaskModel receivedModel = null;

        _taskItem.onSingleClick = task =>
        {
            receivedModel = task;
        };

        _taskItem.Initialize(firstModel);
        _taskItem.Initialize(secondModel);
        PrepareSingleClick();

        _buttonRoot.onClick.Invoke();

        Assert.IsNotNull(receivedModel);
        Assert.AreEqual(12, receivedModel.id);
        Assert.AreEqual("Второе задание", receivedModel.title);
    }

    private void PrepareSingleClick()
    {
        SetPrivateField(_taskItem, "lastClick", -999f);
    }

    private static TaskModel CreateTaskModel(int id, string title)
    {
        return new TaskModel
        {
            id = id,
            title = title
        };
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        var field = target
            .GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(field, $"Поле {fieldName} не найдено.");

        return (T)field.GetValue(target);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target
            .GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(field, $"Поле {fieldName} не найдено.");

        field.SetValue(target, value);
    }
}