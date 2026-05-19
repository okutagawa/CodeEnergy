using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using MyGame.Data;
using MyGame.Models;

public class CourseManagementTests
{
    private string _testFolder;

    private GameObject _managerObject;
    private CourseListManager _manager;

    private RectTransform _contentCourses;
    private GameObject _courseItemPrefab;
    private InputField _inputCourseName;
    private Button _buttonAddCourse;
    private Button _buttonDeleteSelected;
    private Button _buttonEditSelected;
    private Button _buttonExit;
    private Text _operationStatusText;

    [SetUp]
    public void SetUp()
    {
        _testFolder = Path.Combine(Application.temporaryCachePath, "CodeEnergy_CourseManagementTests");

        if (Directory.Exists(_testFolder))
        {
            Directory.Delete(_testFolder, true);
        }

        Directory.CreateDirectory(_testFolder);

        SaveService.TestSaveFolderOverride = _testFolder;
        SaveService.EnsureWorkingFiles();

        _managerObject = new GameObject("CourseListManager_TestObject");
        _manager = _managerObject.AddComponent<CourseListManager>();

        var contentObject = new GameObject("ContentCourses");
        _contentCourses = contentObject.AddComponent<RectTransform>();

        _courseItemPrefab = CreateCourseItemPrefab();

        _inputCourseName = CreateInputField("InputCourseName");
        _buttonAddCourse = CreateButton("ButtonAddCourse");
        _buttonDeleteSelected = CreateButton("ButtonDeleteSelected");
        _buttonEditSelected = CreateButton("ButtonEditSelected");
        _buttonExit = CreateButton("ButtonExit");
        _operationStatusText = CreateText("OperationStatusText");

        _manager.contentCourses = _contentCourses;
        _manager.prefabCourseItem = _courseItemPrefab;
        _manager.inputCourseName = _inputCourseName;
        _manager.buttonAddCourse = _buttonAddCourse;
        _manager.buttonDeleteSelected = _buttonDeleteSelected;
        _manager.buttonEditSelected = _buttonEditSelected;
        _manager.buttonExit = _buttonExit;
        _manager.operationStatusText = _operationStatusText;

        InvokePrivateMethod(_manager, "Start");
    }

    [TearDown]
    public void TearDown()
    {
        SaveService.TestSaveFolderOverride = null;

        if (_managerObject != null)
        {
            Object.DestroyImmediate(_managerObject);
        }

        if (_contentCourses != null)
        {
            Object.DestroyImmediate(_contentCourses.gameObject);
        }

        if (_courseItemPrefab != null)
        {
            Object.DestroyImmediate(_courseItemPrefab);
        }

        if (_inputCourseName != null)
        {
            Object.DestroyImmediate(_inputCourseName.gameObject);
        }

        if (_buttonAddCourse != null)
        {
            Object.DestroyImmediate(_buttonAddCourse.gameObject);
        }

        if (_buttonDeleteSelected != null)
        {
            Object.DestroyImmediate(_buttonDeleteSelected.gameObject);
        }

        if (_buttonEditSelected != null)
        {
            Object.DestroyImmediate(_buttonEditSelected.gameObject);
        }

        if (_buttonExit != null)
        {
            Object.DestroyImmediate(_buttonExit.gameObject);
        }

        if (_operationStatusText != null)
        {
            Object.DestroyImmediate(_operationStatusText.gameObject);
        }

        if (Directory.Exists(_testFolder))
        {
            Directory.Delete(_testFolder, true);
        }
    }

    [Test]
    public void Start_WhenManagerInitialized_ShouldCreateCoursesFile()
    {
        var coursesPath = SaveService.GetPath(SaveService.CoursesFileName);

        Assert.IsTrue(File.Exists(coursesPath));
    }

    [Test]
    public void AddCourse_WhenInputHasName_ShouldAddCourseToDataFile()
    {
        _inputCourseName.text = "Основы C#";

        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        var courses = DataManager.LoadCourses();

        Assert.AreEqual(1, courses.courses.Count);
        Assert.AreEqual("Основы C#", courses.courses[0].name);
    }

    [Test]
    public void AddCourse_WhenInputHasName_ShouldClearInputField()
    {
        _inputCourseName.text = "Переменные и типы данных";

        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        Assert.AreEqual(string.Empty, _inputCourseName.text);
    }

    [Test]
    public void AddCourse_WhenInputHasOnlySpaces_ShouldNotAddCourse()
    {
        _inputCourseName.text = "   ";

        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        var courses = DataManager.LoadCourses();

        Assert.AreEqual(0, courses.courses.Count);
    }

    [Test]
    public void AddCourse_WhenInputIsEmpty_ShouldNotAddCourse()
    {
        _inputCourseName.text = string.Empty;

        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        var courses = DataManager.LoadCourses();

        Assert.AreEqual(0, courses.courses.Count);
    }

    [Test]
    public void AddCourse_WhenTwoCoursesAdded_ShouldAssignDifferentIds()
    {
        _inputCourseName.text = "Первый курс";
        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        _inputCourseName.text = "Второй курс";
        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        var courses = DataManager.LoadCourses();

        Assert.AreEqual(2, courses.courses.Count);
        Assert.AreNotEqual(courses.courses[0].id, courses.courses[1].id);
    }

    [Test]
    public void AddCourse_WhenCourseAdded_ShouldCreateUiItem()
    {
        _inputCourseName.text = "Курс для UI";

        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        Assert.AreEqual(1, _contentCourses.childCount);
    }

    [Test]
    public void RefreshUI_WhenCoursesExist_ShouldCreateUiItemsForAllCourses()
    {
        var container = new CoursesContainer();
        container.courses.Add(new CourseModel { id = 1, name = "Первый курс" });
        container.courses.Add(new CourseModel { id = 2, name = "Второй курс" });

        DataManager.SaveCourses(container);

        SetPrivateField(_manager, "container", null);

        _manager.RefreshUI();

        Assert.AreEqual(2, _contentCourses.childCount);
    }

    [Test]
    public void SelectCourse_WhenCourseExists_ShouldStoreSelectedCourseId()
    {
        _inputCourseName.text = "Курс для выбора";
        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        var courses = DataManager.LoadCourses();
        var courseId = courses.courses[0].id;

        _manager.SelectCourse(courseId);

        var selectedId = GetPrivateField<int>(_manager, "selectedCourseId");

        Assert.AreEqual(courseId, selectedId);
    }

    [Test]
    public void DeleteSelectedCourse_WhenCourseSelected_ShouldRemoveCourseFromDataFile()
    {
        _inputCourseName.text = "Курс для удаления";
        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        var coursesBefore = DataManager.LoadCourses();
        var courseId = coursesBefore.courses[0].id;

        _manager.SelectCourse(courseId);
        ExpectDestroyEditModeError();
        _manager.DeleteSelectedCourse();

        var coursesAfter = DataManager.LoadCourses();

        Assert.AreEqual(0, coursesAfter.courses.Count);
    }

    [Test]
    public void DeleteSelectedCourse_WhenCourseSelected_ShouldRemoveUiItemFromInternalDictionary()
    {
        _inputCourseName.text = "Курс для удаления из UI";
        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        var coursesBefore = DataManager.LoadCourses();
        var courseId = coursesBefore.courses[0].id;

        _manager.SelectCourse(courseId);

        ExpectDestroyEditModeError();
        _manager.DeleteSelectedCourse();

        var instantiated = GetPrivateField<System.Collections.IDictionary>(_manager, "instantiated");

        Assert.AreEqual(0, instantiated.Count);
    }

    [Test]
    public void DeleteSelectedCourse_WhenNoCourseSelected_ShouldNotThrowException()
    {
        Assert.DoesNotThrow(() => _manager.DeleteSelectedCourse());
    }

    [Test]
    public void DeleteSelectedCourse_WhenCourseDeleted_ShouldResetSelectedCourseId()
    {
        _inputCourseName.text = "Курс для сброса выбора";
        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        var coursesBefore = DataManager.LoadCourses();
        var courseId = coursesBefore.courses[0].id;

        _manager.SelectCourse(courseId);
        ExpectDestroyEditModeError();
        _manager.DeleteSelectedCourse();

        var selectedId = GetPrivateField<int>(_manager, "selectedCourseId");

        Assert.AreEqual(-1, selectedId);
    }

    [Test]
    public void AddButton_WhenClicked_ShouldAddCourse()
    {
        _inputCourseName.text = "Курс через кнопку";

        _buttonAddCourse.onClick.Invoke();

        var courses = DataManager.LoadCourses();

        Assert.AreEqual(1, courses.courses.Count);
        Assert.AreEqual("Курс через кнопку", courses.courses[0].name);
    }

    [Test]
    public void DeleteButton_WhenClicked_ShouldDeleteSelectedCourse()
    {
        _inputCourseName.text = "Курс через кнопку удаления";
        InvokePrivateMethod(_manager, "OnAddCourseClicked");

        var coursesBefore = DataManager.LoadCourses();
        var courseId = coursesBefore.courses[0].id;

        _manager.SelectCourse(courseId);
        ExpectDestroyEditModeError();
        _buttonDeleteSelected.onClick.Invoke();

        var coursesAfter = DataManager.LoadCourses();

        Assert.AreEqual(0, coursesAfter.courses.Count);
    }

    private static GameObject CreateCourseItemPrefab()
    {
        var prefab = new GameObject("CourseItemPrefab");
        prefab.AddComponent<RectTransform>();
        prefab.AddComponent<Image>();

        var item = prefab.AddComponent<CourseItem>();

        var textObject = new GameObject("CourseTitleText");
        textObject.transform.SetParent(prefab.transform);
        var text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var buttonObject = new GameObject("CourseButtonRoot");
        buttonObject.transform.SetParent(prefab.transform);
        buttonObject.AddComponent<RectTransform>();
        buttonObject.AddComponent<Image>();
        var button = buttonObject.AddComponent<Button>();

        item.textTitle = text;
        item.buttonRoot = button;

        return prefab;
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

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        var field = target
            .GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(field, $"Поле {fieldName} не найдено.");

        return (T)field.GetValue(target);
    }

    private static void ExpectDestroyEditModeError()
    {
        LogAssert.Expect(
            LogType.Error,
            new Regex("Destroy may not be called from edit mode")
        );
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