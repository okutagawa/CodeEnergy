using UnityEngine;

[DisallowMultipleComponent]
public class LampStateByTasks : MonoBehaviour
{
    [Header("Required progress")]
    [SerializeField] private int stableAfterCompletedTasks = 2;
    [SerializeField] private string stableWorldEvent = WorldEventKey.FixLanterns;

    [Header("References")]
    [SerializeField] private RandomLightFlicker flicker;
    [SerializeField] private Light targetLight;
    [SerializeField] private Renderer targetRenderer;

    [Header("Stable neon light")]
    [SerializeField] private float stableLightIntensity = 4f;
    [SerializeField] private float stableLightRange = 8f;
    [SerializeField] private Color stableLightColor = Color.cyan;

    [Header("Stable neon emission")]
    [SerializeField] private string emissionPropertyName = "_EmissionColor";
    [SerializeField] private Color stableEmissionColor = Color.cyan;
    [SerializeField] private float stableEmissionMultiplier = 3f;
    [SerializeField] private bool applyStableValuesEveryFrame = true;

    private MaterialPropertyBlock _block;
    private bool _isStable;

    private void Awake()
    {
        if (flicker == null)
            flicker = GetComponent<RandomLightFlicker>();

        if (targetLight == null)
            targetLight = GetComponentInChildren<Light>();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        _block = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        GameState.OnTaskCompleted += HandleTaskCompleted;
        GameState.OnWorldEventCompleted += HandleWorldEventCompleted;
        RefreshState();
    }

    private void OnDisable()
    {
        GameState.OnTaskCompleted -= HandleTaskCompleted;
        GameState.OnWorldEventCompleted -= HandleWorldEventCompleted;
    }

    private void Update()
    {
        if (_isStable && applyStableValuesEveryFrame)
            ApplyStableNeonState();
    }

    private void HandleTaskCompleted(int taskId)
    {
        RefreshState();
    }

    private void HandleWorldEventCompleted(string worldEvent)
    {
        if (string.IsNullOrEmpty(stableWorldEvent) || WorldEventKey.Normalize(worldEvent) == WorldEventKey.Normalize(stableWorldEvent))
            RefreshState();
    }

    private void RefreshState()
    {
        _isStable = IsStableProgressReached();

        if (flicker != null)
            flicker.enabled = !_isStable;

        if (_isStable)
            ApplyStableNeonState();
    }

    private void ApplyStableNeonState()
    {
        if (targetLight != null)
        {
            targetLight.enabled = true;
            targetLight.color = stableLightColor;
            targetLight.intensity = stableLightIntensity;
            targetLight.range = stableLightRange;
        }

        if (targetRenderer != null)
        {
            targetRenderer.GetPropertyBlock(_block);
            _block.SetColor(emissionPropertyName, stableEmissionColor * Mathf.Max(0f, stableEmissionMultiplier));
            targetRenderer.SetPropertyBlock(_block);
        }
    }

    private bool IsStableProgressReached()
    {
        if (GameState.Instance == null)
            return false;
        if (GameState.Instance.IsWorldEventCompleted(stableWorldEvent))
            return true;
        return GameState.Instance.GetData().completedTaskIds.Count >= Mathf.Max(0, stableAfterCompletedTasks);
    }
}