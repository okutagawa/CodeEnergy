using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Audio")]
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;
    [SerializeField] private AudioMixer mainAudioMixer;

    [Header("Buttons")]
    [SerializeField] private Button btnSave;
    [SerializeField] private Button btnExit;

    [Header("Optional")]
    [SerializeField] private GameObject settingsPanelRoot;

    private const string ResolutionPreferenceKey = "ResolutionPreference";
    private const string FullscreenPreferenceKey = "FullscreenPreference";
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SFXVolume";

    private const string MixerMasterParameter = "MasterVol";
    private const string MixerMusicParameter = "MusicVol";
    private const string MixerSfxParameter = "SFXVol";

    private Resolution[] resolutions;

    private void Start()
    {
        InitializeResolutions();
        LoadSettings();
        BindUiEvents();
    }

    private void InitializeResolutions()
    {
        if (resolutionDropdown == null)
            return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new List<string>();
        var currentResolutionIndex = 0;

        for (var i = 0; i < resolutions.Length; i++)
        {
            var option = $"{resolutions[i].width}x{resolutions[i].height} {resolutions[i].refreshRateRatio.value:0.##}Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                Mathf.Approximately((float)resolutions[i].refreshRateRatio.value, (float)Screen.currentResolution.refreshRateRatio.value))
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        var savedResolution = PlayerPrefs.GetInt(ResolutionPreferenceKey, currentResolutionIndex);
        resolutionDropdown.value = Mathf.Clamp(savedResolution, 0, Mathf.Max(0, resolutions.Length - 1));
        resolutionDropdown.RefreshShownValue();
    }

    private void BindUiEvents()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        if (masterVolume != null)
        {
            masterVolume.onValueChanged.RemoveListener(ChangeMasterVolume);
            masterVolume.onValueChanged.AddListener(ChangeMasterVolume);
        }

        if (musicVolume != null)
        {
            musicVolume.onValueChanged.RemoveListener(ChangeMusicVolume);
            musicVolume.onValueChanged.AddListener(ChangeMusicVolume);
        }

        if (sfxVolume != null)
        {
            sfxVolume.onValueChanged.RemoveListener(ChangeSfxVolume);
            sfxVolume.onValueChanged.AddListener(ChangeSfxVolume);
        }

        if (btnSave != null)
        {
            btnSave.onClick.RemoveListener(SaveSettings);
            btnSave.onClick.AddListener(SaveSettings);
        }

        if (btnExit != null)
        {
            btnExit.onClick.RemoveListener(CloseSettings);
            btnExit.onClick.AddListener(CloseSettings);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutions.Length == 0)
            return;

        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
        var resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SaveSettings()
    {
        if (resolutionDropdown != null)
            PlayerPrefs.SetInt(ResolutionPreferenceKey, resolutionDropdown.value);

        PlayerPrefs.SetInt(FullscreenPreferenceKey, Convert.ToInt32(Screen.fullScreen));

        if (masterVolume != null)
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume.value);

        if (musicVolume != null)
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume.value);

        if (sfxVolume != null)
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume.value);

        PlayerPrefs.Save();
        Debug.Log("[Settings] Settings saved.");
    }

    public void LoadSettings()
    {
        if (fullscreenToggle != null)
        {
            var isFullscreen = Convert.ToBoolean(
                PlayerPrefs.GetInt(FullscreenPreferenceKey, Convert.ToInt32(Screen.fullScreen))
            );
            fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);
            Screen.fullScreen = isFullscreen;
        }

        if (resolutionDropdown != null)
        {
            SetResolution(resolutionDropdown.value);
        }

        if (masterVolume != null)
        {
            var value = PlayerPrefs.GetFloat(MasterVolumeKey, masterVolume.value);
            masterVolume.SetValueWithoutNotify(value);
            ChangeMasterVolume(value);
        }

        if (musicVolume != null)
        {
            var value = PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume.value);
            musicVolume.SetValueWithoutNotify(value);
            ChangeMusicVolume(value);
        }

        if (sfxVolume != null)
        {
            var value = PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume.value);
            sfxVolume.SetValueWithoutNotify(value);
            ChangeSfxVolume(value);
        }
    }

    public void ChangeMasterVolume(float value)
    {
        ApplyMixerVolume(MixerMasterParameter, value);
    }

    public void ChangeMusicVolume(float value)
    {
        ApplyMixerVolume(MixerMusicParameter, value);
    }

    public void ChangeSfxVolume(float value)
    {
        ApplyMixerVolume(MixerSfxParameter, value);
    }

    public void CloseSettings()
    {
        var pauseMenu = FindObjectOfType<PauseMenuController>(true);
        if (pauseMenu != null && pauseMenu.IsPaused && pauseMenu.gameObject.scene == gameObject.scene)
        {
            pauseMenu.CloseSettings();
            return;
        }

        if (UIManager.Instance != null && UIManager.Instance.settingsPanel != null)
        {
            UIManager.Instance.HideSettingsPanel();
            return;
        }

        if (settingsPanelRoot != null)
        {
            settingsPanelRoot.SetActive(false);
            return;
        }

        gameObject.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsPanelRoot != null)
        {
            settingsPanelRoot.SetActive(true);
            return;
        }

        gameObject.SetActive(true);
    }

    private void ApplyMixerVolume(string mixerParameter, float normalizedVolume)
    {
        if (mainAudioMixer == null)
            return;

        mainAudioMixer.SetFloat(mixerParameter, LinearToDb(normalizedVolume));
    }

    private static float LinearToDb(float linear)
    {
        linear = Mathf.Clamp(linear, 0.0001f, 1f);
        return 20f * Mathf.Log10(linear);
    }
}