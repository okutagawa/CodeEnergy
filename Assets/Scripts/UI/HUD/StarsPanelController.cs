using UnityEngine;
using UnityEngine.UI;

public class StarsPanelController : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Text starsText;

    [Header("Display")]
    [SerializeField] private string format = "{0}";

    private void OnEnable()
    {
        GameState.OnTotalStarsChanged += HandleTotalStarsChanged;
        Refresh();
    }

    private void OnDisable()
    {
        GameState.OnTotalStarsChanged -= HandleTotalStarsChanged;
    }

    public void Refresh()
    {
        var stars = GameState.Instance != null ? GameState.Instance.GetTotalStars() : 0;
        HandleTotalStarsChanged(stars);
    }

    private void HandleTotalStarsChanged(int totalStars)
    {
        if (starsText == null) return;

        string safeFormat = string.IsNullOrEmpty(format) ? "{0}" : format;
        starsText.text = string.Format(safeFormat, Mathf.Max(0, totalStars));
    }
}