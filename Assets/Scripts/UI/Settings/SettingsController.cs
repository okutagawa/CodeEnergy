using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] public Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Audio")]
    [SerializeField] public Slider masterVolume;
    [SerializeField] public Slider musicVolume;
    [SerializeField] public Slider sfxVolume;
    [SerializeField] public AudioMixer mainAudioMixer;

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
            // Используем refreshRate (целое число), формируем читаемую строку
            var option = $"{resolutions[i].width}x{resolutions[i].height} {resolutions[i].refreshRate}Hz";
            options.Add(option);

            // Сравниваем с текущим экраном по ширине/высоте/частоте
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
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
        // Устанавливаем fullscreen через FullScreenMode: оставляем текущий режим, но меняем флаг fullScreen
        // В Unity есть несколько режимов FullScreenMode; здесь используем FullScreenMode.FullScreenWindow при true, иначе Windowed
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutions.Length == 0)
            return;

        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
        var resolution = resolutions[resolutionIndex];

        // Используем перегрузку с FullScreenMode и refreshRate (int)
        // Передаём текущий fullScreenMode, чтобы не менять тип полноэкранного режима
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
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

            // Устанавливаем fullscreen корректно через FullScreenMode
            Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.fullScreen = isFullscreen;
        }

        if (resolutionDropdown != null)
        {
            // При загрузке выставляем разрешение, используя текущее значение дропдауна (инициализировано в InitializeResolutions)
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
        mainAudioMixer.SetFloat("MasterVol", masterVolume.value);
    }

    public void ChangeMusicVolume(float value)
    {
        mainAudioMixer.SetFloat("MusicVol", musicVolume.value);
    }

    public void ChangeSfxVolume(float value)
    {
        mainAudioMixer.SetFloat("SFXVol", sfxVolume.value);
    }

    //private float LinearToDb(float linear)
    //{
    //    // Ожидаем, что слайдер даёт 0..1; конвертируем в dB для AudioMixer
    //    if (linear <= 0.0001f) return -80f;
    //    return 20f * Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f));
    //}

    public void CloseSettings()
    {
        if (settingsPanelRoot != null)
        {
            settingsPanelRoot.SetActive(false);
            return;
        }

        gameObject.SetActive(false);
    }
}
