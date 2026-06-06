using System;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPopupController : MonoBehaviour
{
    private const string DefaultUserMessage = "Произошла ошибка. Попробуйте повторить действие.";

    [SerializeField] private Text statusText;
    [SerializeField] private Component statusTextComponent;
    [SerializeField] private Button btnOk;

    private static ErrorPopupController cachedInstance;

    private void Awake()
    {
        cachedInstance = this;
        ResolveReferences();
        BindOkButton();
    }

    private void OnEnable()
    {
        ResolveReferences();
        BindOkButton();
    }

    public static ErrorPopupController Instance
    {
        get
        {
            if (cachedInstance != null) return cachedInstance;

            cachedInstance = FindObjectOfType<ErrorPopupController>(true);
            if (cachedInstance != null) return cachedInstance;

            var panelObject = FindSceneGameObject("ErrorPopupPanel");
            if (panelObject == null) return null;

            cachedInstance = panelObject.GetComponent<ErrorPopupController>()
                             ?? panelObject.AddComponent<ErrorPopupController>();
            return cachedInstance;
        }
    }

    public static void Show(string message)
    {
        var popup = Instance;
        if (popup == null)
        {
            Debug.LogWarning("[ErrorPopup] ErrorPopupPanel was not found in the active scene.");
            return;
        }

        popup.ShowMessage(message);
    }

    public void ShowMessage(string message)
    {
        ResolveReferences();
        BindOkButton();

        SetStatusText(string.IsNullOrWhiteSpace(message) ? DefaultUserMessage : message.Trim());

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ResolveReferences()
    {
        if (statusText == null || statusTextComponent == null)
        {
            var statusTransform = FindChild("StatusText");
            if (statusTransform != null)
            {
                statusText = statusText != null ? statusText : statusTransform.GetComponent<Text>();
                statusTextComponent = statusText != null ? statusText : FindTextLikeComponent(statusTransform);
            }
        }

        if (btnOk == null)
        {
            btnOk = FindChildComponent<Button>("BtnOk");
        }
    }

    private void BindOkButton()
    {
        if (btnOk == null) return;
        btnOk.onClick.RemoveListener(Hide);
        btnOk.onClick.AddListener(Hide);
    }

    private void SetStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            return;
        }

        if (statusTextComponent == null) return;

        var textProperty = statusTextComponent.GetType().GetProperty("text");
        if (textProperty != null && textProperty.CanWrite)
            textProperty.SetValue(statusTextComponent, message, null);
    }

    private Transform FindChild(string childName)
    {
        foreach (var child in GetComponentsInChildren<Transform>(true))
        {
            if (child != null && child.name == childName) return child;
        }

        return null;
    }

    private T FindChildComponent<T>(string childName) where T : Component
    {
        var child = FindChild(childName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private static Component FindTextLikeComponent(Transform transform)
    {
        foreach (var component in transform.GetComponents<Component>())
        {
            if (component == null) continue;

            var type = component.GetType();
            if (type.Name == "TMP_Text" || IsSubclassOfName(type, "TMP_Text"))
                return component;
        }

        return null;
    }

    private static bool IsSubclassOfName(Type type, string typeName)
    {
        while (type != null)
        {
            if (type.Name == typeName) return true;
            type = type.BaseType;
        }

        return false;
    }

    private static GameObject FindSceneGameObject(string objectName)
    {
        var transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var transform in transforms)
        {
            if (transform == null || transform.name != objectName) continue;

            var scene = transform.gameObject.scene;
            if (scene.IsValid()) return transform.gameObject;
        }

        return null;
    }
}