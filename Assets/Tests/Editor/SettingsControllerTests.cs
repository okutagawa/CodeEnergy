using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SettingsControllerTests
{
    private GameObject _controllerObject;
    private SettingsController _controller;

    private Dropdown _resolutionDropdown;
    private Toggle _fullscreenToggle;
    private Slider _masterVolume;
    private Slider _musicVolume;
    private Slider _sfxVolume;
    private Button _saveButton;
    private Button _exitButton;
    private GameObject _settingsPanelRoot;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();

        _controllerObject = new GameObject("SettingsController_TestObject");
        _controller = _controllerObject.AddComponent<SettingsController>();

        _resolutionDropdown = CreateDropdown("ResolutionDropdown");
        _fullscreenToggle = CreateToggle("FullscreenToggle");
        _masterVolume = CreateSlider("MasterVolumeSlider", 0.75f);
        _musicVolume = CreateSlider("MusicVolumeSlider", 0.50f);
        _sfxVolume = CreateSlider("SfxVolumeSlider", 0.25f);
        _saveButton = CreateButton("SaveButton");
        _exitButton = CreateButton("ExitButton");
        _settingsPanelRoot = new GameObject("SettingsPanelRoot");
        _settingsPanelRoot.SetActive(true);

        SetPrivateField(_controller, "resolutionDropdown", _resolutionDropdown);
        SetPrivateField(_controller, "fullscreenToggle", _fullscreenToggle);
        SetPrivateField(_controller, "masterVolume", _masterVolume);
        SetPrivateField(_controller, "musicVolume", _musicVolume);
        SetPrivateField(_controller, "sfxVolume", _sfxVolume);
        SetPrivateField(_controller, "btnSave", _saveButton);
        SetPrivateField(_controller, "btnExit", _exitButton);
        SetPrivateField(_controller, "settingsPanelRoot", _settingsPanelRoot);

        PrepareResolutionDropdown();

        InvokePrivateMethod(_controller, "Start");
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteAll();

        if (_controllerObject != null)
        {
            Object.DestroyImmediate(_controllerObject);
        }

        if (_resolutionDropdown != null)
        {
            Object.DestroyImmediate(_resolutionDropdown.gameObject);
        }

        if (_fullscreenToggle != null)
        {
            Object.DestroyImmediate(_fullscreenToggle.gameObject);
        }

        if (_masterVolume != null)
        {
            Object.DestroyImmediate(_masterVolume.gameObject);
        }

        if (_musicVolume != null)
        {
            Object.DestroyImmediate(_musicVolume.gameObject);
        }

        if (_sfxVolume != null)
        {
            Object.DestroyImmediate(_sfxVolume.gameObject);
        }

        if (_saveButton != null)
        {
            Object.DestroyImmediate(_saveButton.gameObject);
        }

        if (_exitButton != null)
        {
            Object.DestroyImmediate(_exitButton.gameObject);
        }

        if (_settingsPanelRoot != null)
        {
            Object.DestroyImmediate(_settingsPanelRoot);
        }
    }

    [Test]
    public void SaveSettings_WhenValuesChanged_ShouldSaveMasterVolume()
    {
        _masterVolume.value = 0.80f;

        _controller.SaveSettings();

        Assert.AreEqual(0.80f, PlayerPrefs.GetFloat("MasterVolume"), 0.001f);
    }

    [Test]
    public void SaveSettings_WhenValuesChanged_ShouldSaveMusicVolume()
    {
        _musicVolume.value = 0.35f;

        _controller.SaveSettings();

        Assert.AreEqual(0.35f, PlayerPrefs.GetFloat("MusicVolume"), 0.001f);
    }

    [Test]
    public void SaveSettings_WhenValuesChanged_ShouldSaveSfxVolume()
    {
        _sfxVolume.value = 0.60f;

        _controller.SaveSettings();

        Assert.AreEqual(0.60f, PlayerPrefs.GetFloat("SFXVolume"), 0.001f);
    }

    [Test]
    public void SaveSettings_WhenResolutionSelected_ShouldSaveResolutionPreference()
    {
        _resolutionDropdown.value = 0;

        _controller.SaveSettings();

        Assert.AreEqual(0, PlayerPrefs.GetInt("ResolutionPreference"));
    }

    [Test]
    public void SaveSettings_WhenFullscreenChanged_ShouldSaveFullscreenPreference()
    {
        _controller.SetFullscreen(false);
        _controller.SaveSettings();

        Assert.AreEqual(0, PlayerPrefs.GetInt("FullscreenPreference"));
    }

    [Test]
    public void LoadSettings_WhenPlayerPrefsContainMasterVolume_ShouldRestoreMasterVolumeSlider()
    {
        PlayerPrefs.SetFloat("MasterVolume", 0.22f);

        _controller.LoadSettings();

        Assert.AreEqual(0.22f, _masterVolume.value, 0.001f);
    }

    [Test]
    public void LoadSettings_WhenPlayerPrefsContainMusicVolume_ShouldRestoreMusicVolumeSlider()
    {
        PlayerPrefs.SetFloat("MusicVolume", 0.44f);

        _controller.LoadSettings();

        Assert.AreEqual(0.44f, _musicVolume.value, 0.001f);
    }

    [Test]
    public void LoadSettings_WhenPlayerPrefsContainSfxVolume_ShouldRestoreSfxVolumeSlider()
    {
        PlayerPrefs.SetFloat("SFXVolume", 0.66f);

        _controller.LoadSettings();

        Assert.AreEqual(0.66f, _sfxVolume.value, 0.001f);
    }

    [Test]
    public void LoadSettings_WhenPlayerPrefsContainFullscreen_ShouldRestoreFullscreenToggle()
    {
        PlayerPrefs.SetInt("FullscreenPreference", 1);

        _controller.LoadSettings();

        Assert.IsTrue(_fullscreenToggle.isOn);
    }

    [Test]
    public void ChangeMasterVolume_WhenAudioMixerIsMissing_ShouldNotThrowException()
    {
        Assert.DoesNotThrow(() => _controller.ChangeMasterVolume(0.50f));
    }

    [Test]
    public void ChangeMusicVolume_WhenAudioMixerIsMissing_ShouldNotThrowException()
    {
        Assert.DoesNotThrow(() => _controller.ChangeMusicVolume(0.50f));
    }

    [Test]
    public void ChangeSfxVolume_WhenAudioMixerIsMissing_ShouldNotThrowException()
    {
        Assert.DoesNotThrow(() => _controller.ChangeSfxVolume(0.50f));
    }

    [Test]
    public void OpenSettings_WhenPanelRootExists_ShouldActivateSettingsPanel()
    {
        _settingsPanelRoot.SetActive(false);

        _controller.OpenSettings();

        Assert.IsTrue(_settingsPanelRoot.activeSelf);
    }

    [Test]
    public void CloseSettings_WhenPanelRootExists_ShouldDeactivateSettingsPanel()
    {
        _settingsPanelRoot.SetActive(true);

        _controller.CloseSettings();

        Assert.IsFalse(_settingsPanelRoot.activeSelf);
    }

    [Test]
    public void SaveButton_WhenClicked_ShouldSaveSettings()
    {
        _masterVolume.value = 0.91f;

        _saveButton.onClick.Invoke();

        Assert.AreEqual(0.91f, PlayerPrefs.GetFloat("MasterVolume"), 0.001f);
    }

    [Test]
    public void ExitButton_WhenClicked_ShouldCloseSettingsPanel()
    {
        _settingsPanelRoot.SetActive(true);

        _exitButton.onClick.Invoke();

        Assert.IsFalse(_settingsPanelRoot.activeSelf);
    }

    [Test]
    public void LinearToDb_WhenVolumeIsOne_ShouldReturnZeroDb()
    {
        var result = InvokePrivateStaticFloatMethod("LinearToDb", 1f);

        Assert.AreEqual(0f, result, 0.001f);
    }

    [Test]
    public void LinearToDb_WhenVolumeIsHalf_ShouldReturnApproximatelyMinusSixDb()
    {
        var result = InvokePrivateStaticFloatMethod("LinearToDb", 0.5f);

        Assert.AreEqual(-6.0206f, result, 0.01f);
    }

    private void PrepareResolutionDropdown()
    {
        _resolutionDropdown.ClearOptions();
        _resolutionDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "1920x1080 60Hz",
            "1280x720 60Hz"
        });
        _resolutionDropdown.value = 0;
        _resolutionDropdown.RefreshShownValue();
    }

    private static Dropdown CreateDropdown(string name)
    {
        var dropdownObject = new GameObject(name);
        dropdownObject.AddComponent<RectTransform>();

        var dropdown = dropdownObject.AddComponent<Dropdown>();

        var labelObject = new GameObject(name + "_Label");
        labelObject.transform.SetParent(dropdownObject.transform);
        var label = labelObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        dropdown.captionText = label;

        return dropdown;
    }

    private static Toggle CreateToggle(string name)
    {
        var toggleObject = new GameObject(name);
        toggleObject.AddComponent<RectTransform>();

        return toggleObject.AddComponent<Toggle>();
    }

    private static Slider CreateSlider(string name, float value)
    {
        var sliderObject = new GameObject(name);
        sliderObject.AddComponent<RectTransform>();

        var slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;

        return slider;
    }

    private static Button CreateButton(string name)
    {
        var buttonObject = new GameObject(name);
        buttonObject.AddComponent<RectTransform>();
        buttonObject.AddComponent<Image>();

        return buttonObject.AddComponent<Button>();
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target
            .GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(field, $"Поле {fieldName} не найдено.");

        field.SetValue(target, value);
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        var method = target
            .GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(method, $"Метод {methodName} не найден.");

        method.Invoke(target, null);
    }

    private static float InvokePrivateStaticFloatMethod(string methodName, float value)
    {
        var method = typeof(SettingsController)
            .GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);

        Assert.IsNotNull(method, $"Метод {methodName} не найден.");

        return (float)method.Invoke(null, new object[] { value });
    }
}