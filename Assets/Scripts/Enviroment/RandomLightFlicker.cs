using UnityEngine;

[DisallowMultipleComponent]
public class RandomLightFlicker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light targetLight;
    [SerializeField] private Renderer targetRenderer;

    [Header("Emission")]
    [SerializeField] private string emissionPropertyName = "_EmissionColor";
    [SerializeField] private Color baseEmissionColor = Color.cyan;
    [SerializeField] private float minEmissionMultiplier = 0.2f;
    [SerializeField] private float maxEmissionMultiplier = 1.8f;

    [Header("Light Intensity")]
    [SerializeField] private float minLightIntensity = 0.2f;
    [SerializeField] private float maxLightIntensity = 2f;

    [Header("Timing")]
    [SerializeField] private float minInterval = 0.04f;
    [SerializeField] private float maxInterval = 0.2f;
    [SerializeField] private float smoothSpeed = 10f;

    private MaterialPropertyBlock _propertyBlock;
    private float _targetLightIntensity;
    private float _targetEmissionMultiplier;
    private float _currentEmissionMultiplier;
    private float _instanceNoiseOffset;

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponentInChildren<Light>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        _propertyBlock = new MaterialPropertyBlock();

        // Different random state for each lamp, avoids synchronized flicker.
        _instanceNoiseOffset = Random.Range(0f, 1000f);

        _targetLightIntensity = Random.Range(minLightIntensity, maxLightIntensity);
        _targetEmissionMultiplier = Random.Range(minEmissionMultiplier, maxEmissionMultiplier);
        _currentEmissionMultiplier = _targetEmissionMultiplier;
    }

    private void OnEnable()
    {
        ScheduleNextTick(0f);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(PickNewTargets));
    }

    private void Update()
    {
        if (targetLight != null)
        {
            targetLight.intensity = Mathf.Lerp(targetLight.intensity, _targetLightIntensity, Time.deltaTime * smoothSpeed);
        }

        if (targetRenderer != null)
        {
            _currentEmissionMultiplier = Mathf.Lerp(_currentEmissionMultiplier, _targetEmissionMultiplier, Time.deltaTime * smoothSpeed);

            targetRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(emissionPropertyName, baseEmissionColor * _currentEmissionMultiplier);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }
    }

    private void PickNewTargets()
    {
        float lightNoise = Mathf.PerlinNoise(Time.time * 6f + _instanceNoiseOffset, _instanceNoiseOffset);
        float emissionNoise = Mathf.PerlinNoise(_instanceNoiseOffset, Time.time * 8f + _instanceNoiseOffset * 1.37f);

        _targetLightIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, lightNoise);
        _targetEmissionMultiplier = Mathf.Lerp(minEmissionMultiplier, maxEmissionMultiplier, emissionNoise);

        ScheduleNextTick(Random.Range(minInterval, maxInterval));
    }

    private void ScheduleNextTick(float delay)
    {
        CancelInvoke(nameof(PickNewTargets));
        Invoke(nameof(PickNewTargets), delay);
    }
}