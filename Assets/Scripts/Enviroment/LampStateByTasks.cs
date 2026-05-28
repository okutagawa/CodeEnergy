using UnityEngine;

[DisallowMultipleComponent]
public class LampStateByTasks : MonoBehaviour
{
    [Header("Required progress")]
    [SerializeField] private int stableAfterCompletedTasks = 2;

    [Header("References")]
    [SerializeField] private RandomLightFlicker flicker;
    [SerializeField] private Light targetLight;
    [SerializeField] private Renderer targetRenderer;

    [Header("Stable values")]
    [SerializeField] private float stableLightIntensity = 1.2f;
    [SerializeField] private string emissionPropertyName = "_EmissionColor";
    [SerializeField] private Color stableEmissionColor = Color.cyan;

    private MaterialPropertyBlock _block;

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
        RefreshState();
    }

    private void OnDisable()
    {
        GameState.OnTaskCompleted -= HandleTaskCompleted;
    }

    private void HandleTaskCompleted(int taskId)
    {
        RefreshState();
    }

    private void RefreshState()
    {
        bool stable = IsStableProgressReached();

        if (flicker != null)
            flicker.enabled = !stable;

        if (!stable)
            return;

        if (targetLight != null)
            targetLight.intensity = stableLightIntensity;

        if (targetRenderer != null)
        {
            targetRenderer.GetPropertyBlock(_block);
            _block.SetColor(emissionPropertyName, stableEmissionColor);
            targetRenderer.SetPropertyBlock(_block);
        }
    }

    private bool IsStableProgressReached()
    {
        if (GameState.Instance == null)
            return false;

        return GameState.Instance.GetData().completedTaskIds.Count >= Mathf.Max(0, stableAfterCompletedTasks);
    }
}