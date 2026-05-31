// Assets/Scripts/UI/Quiz/QuizCardController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class QuizCardController : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Button rootButton;
    [SerializeField] private Text choiceText;
    [SerializeField] private Image selectionMark;      // optional
    [SerializeField] private Image stateIcon;          // optional

    [Header("Config")]
    [SerializeField] private Color textColorDefault = Color.white;
    [SerializeField] private Color textColorSelected = Color.white;

    [SerializeField] private Color bgColorDefault = new Color(0.1f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color bgColorSelected = new Color(0.25f, 0.5f, 0.25f, 1f);

    [Header("Outline (optional)")]
    [SerializeField] private bool useOutlineToggle = false;

    public int OptionIndex { get; private set; }
    public bool IsSelected { get; private set; }

    public UnityEvent<int> OnClicked = new UnityEvent<int>();

    private Button _button;
    private Text _text;
    private Outline _outline;

    private void Reset()
    {
        _button = GetComponent<Button>();
        if (rootButton == null) rootButton = _button;
        if (rootButton != null && choiceText == null)
        {
            var t = GetComponentInChildren<Text>();
            if (t != null) choiceText = t;
        }
    }

    private void Awake()
    {
        _button = rootButton != null ? rootButton : GetComponent<Button>();
        _text = choiceText;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => OnClicked.Invoke(OptionIndex));
            // оставляем Transition = ColorTint (настрой в префабе цвета normal/highlighted)
        }

        _outline = _text != null ? _text.GetComponent<Outline>() : null;
        if (_outline == null && _text != null && useOutlineToggle)
        {
            _outline = _text.gameObject.AddComponent<Outline>();
            _outline.enabled = false;
        }

        // Явно гарантируем видимость текста при первом показе
        if (_text != null)
        {
            _text.enabled = true;
            _text.color = textColorDefault;
        }

        ApplyVisual(false);
        if (stateIcon != null) stateIcon.gameObject.SetActive(false);
        if (selectionMark != null) selectionMark.enabled = false;
    }

    public void Setup(int optionIndex, string text)
    {
        OptionIndex = optionIndex;
        if (_text != null)
        {
            _text.text = text ?? "";
            _text.color = textColorDefault; // гарантируем видимость
        }
        SetSelected(false);
        if (stateIcon != null) stateIcon.gameObject.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        ApplyVisual(selected);
        if (selectionMark != null) selectionMark.enabled = selected;
    }

    private void ApplyVisual(bool selected)
    {
        if (_text != null)
            _text.color = selected ? textColorSelected : textColorDefault;

        if (_outline != null)
            _outline.enabled = selected && useOutlineToggle;

        if (_button != null)
        {
            var cb = _button.colors;
            cb.normalColor = selected ? bgColorSelected : bgColorDefault;
            cb.highlightedColor = selected ? bgColorSelected : cb.highlightedColor;
            cb.pressedColor = selected ? bgColorSelected * 0.9f : cb.pressedColor;
            _button.colors = cb;
        }
    }

    public void LockAfterSubmit()
    {
        if (_button != null) _button.interactable = false;
    }

    public void ShowStateIcon(bool visible)
    {
        if (stateIcon != null) stateIcon.gameObject.SetActive(visible);
    }
}
