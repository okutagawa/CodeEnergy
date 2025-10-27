using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// ”правление списком профилей пользовател€ (создание/удаление/выбор)
public class ProfileController : MonoBehaviour
{
    public InputField newProfileName;
    public Button addProfileBtn;
    public Transform profilesListContent;
    public GameObject profileButtonPrefab; // prefab with Button + Text

    private ProfilesContainer profilesData;

    void Start()
    {
        profilesData = DataManager.LoadProfiles();
        addProfileBtn.onClick.AddListener(OnAddProfile);
        RefreshList();
    }

    void OnAddProfile()
    {
        string name = newProfileName.text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        var p = new Profile { name = name };
        profilesData.profiles.Add(p);
        DataManager.SaveProfiles(profilesData);
        newProfileName.text = "";
        RefreshList();
    }

    void RefreshList()
    {
        foreach (Transform t in profilesListContent) Destroy(t.gameObject);
        foreach (var profile in profilesData.profiles)
        {
            var go = Instantiate(profileButtonPrefab, profilesListContent);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<Text>();
            txt.text = profile.name;
            btn.onClick.AddListener(() => OnSelectProfile(profile));
        }
    }

    void OnSelectProfile(Profile profile)
    {
        GameState.Instance.CurrentProfile = profile;
        UIManager.Instance.ShowCoursesList();
    }
}
