using UnityEngine;

[DisallowMultipleComponent]
public class PortalStateByTasks : MonoBehaviour
{
    [Header("Required progress")]
    [SerializeField] private TaskCompletionGate activationGate;
    [SerializeField] private string activationWorldEvent = WorldEventKey.ActivatePortal;

    [Header("Portal animation")]
    [SerializeField] private Animator portalAnimator;
    [SerializeField] private string activeParameterName = "IsActive";

    [Header("Optional visual objects")]
    [SerializeField] private GameObject portalVisualRoot;
    [SerializeField] private bool hidePortalVisualUntilActivated = false;

    private bool _isActive;

    private void Awake()
    {
        if (activationGate == null)
            activationGate = GetComponent<TaskCompletionGate>();

        if (portalAnimator == null)
            portalAnimator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        GameState.OnTaskCompleted += HandleTaskCompleted;
        GameState.OnWorldEventCompleted += HandleWorldEventCompleted;
        RefreshState();
    }

    private void Start()
    {
        RefreshState();
    }

    private void OnDisable()
    {
        GameState.OnTaskCompleted -= HandleTaskCompleted;
        GameState.OnWorldEventCompleted -= HandleWorldEventCompleted;
    }

    private void HandleTaskCompleted(int taskId)
    {
        RefreshState();
    }

    private void HandleWorldEventCompleted(string worldEvent)
    {
        if (WorldEventKey.Normalize(worldEvent) == WorldEventKey.Normalize(activationWorldEvent))
            RefreshState();
    }

    private void RefreshState()
    {
        bool shouldBeActive = IsWorldEventCompleted(activationWorldEvent);

        if (_isActive == shouldBeActive)
        {
            ApplyState();
            return;
        }

        _isActive = shouldBeActive;
        ApplyState();
    }

    private bool IsWorldEventCompleted(string worldEvent)
    {
        return GameState.Instance != null && GameState.Instance.IsWorldEventCompleted(worldEvent);
    }

    private void ApplyState()
    {
        if (portalAnimator != null)
        {
            portalAnimator.SetBool(activeParameterName, _isActive);
        }

        if (portalVisualRoot != null && hidePortalVisualUntilActivated)
        {
            portalVisualRoot.SetActive(_isActive);
        }
    }
}