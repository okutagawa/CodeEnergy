using System;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldExpander : MonoBehaviour
{
    public GameObject largeEditorPanel; // assign the panel GameObject
    public InputField largeInputField;  // the big input inside panel
    public Button btnOk;
    public Button btnCancel;

    private InputField currentSmall; // the InputField that requested expansion

    void Start()
    {
        if (largeEditorPanel != null) largeEditorPanel.SetActive(false);
        if (btnOk != null) btnOk.onClick.AddListener(OnOk);
        if (btnCancel != null) btnCancel.onClick.AddListener(OnCancel);
    }

    // call this when small field gets selected (subscribe InputField.onSelect)
    public void Expand(InputField small)
    {
        currentSmall = small;
        largeEditorPanel.SetActive(true);
        largeInputField.text = small.text;
        largeInputField.Select();
        largeInputField.ActivateInputField();
    }

    private void OnOk()
    {
        if (currentSmall != null) currentSmall.text = largeInputField.text;
        Close();
    }

    private void OnCancel()
    {
        Close();
    }

    private void Close()
    {
        largeEditorPanel.SetActive(false);
        currentSmall = null;
    }
}
