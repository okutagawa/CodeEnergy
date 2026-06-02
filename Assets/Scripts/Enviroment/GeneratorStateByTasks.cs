using UnityEngine;

[DisallowMultipleComponent]
public class GeneratorStateByTasks : MonoBehaviour
{
    [Header("Required progress")]
    [SerializeField] private TaskCompletionGate workGate;
    [SerializeField] private string workWorldEvent = WorldEventKey.StartGenerator;

    [Header("Generator animation")]
    [SerializeField] private Animator generatorAnimator;
    [SerializeField] private string isWorkingParameterName = "IsWorking";

    private bool _isWorking;

    private void Awake()
    {
        if (workGate == null)
            workGate = GetComponent<TaskCompletionGate>();

        if (generatorAnimator == null)
            generatorAnimator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
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
        if (WorldEventKey.Normalize(worldEvent) == WorldEventKey.Normalize(workWorldEvent))
            RefreshState();
    }

    private void RefreshState()
    {
        bool shouldWork = (workGate != null && workGate.IsUnlocked()) || IsWorldEventCompleted(workWorldEvent);

        if (_isWorking == shouldWork)
        {
            ApplyState();
            return;
        }

        _isWorking = shouldWork;
        ApplyState();
    }

    private bool IsWorldEventCompleted(string worldEvent)
    {
        return GameState.Instance != null && GameState.Instance.IsWorldEventCompleted(worldEvent);
    }

    private void ApplyState()
    {
        if (generatorAnimator != null)
        {
            generatorAnimator.SetBool(isWorkingParameterName, _isWorking);
        }
    }
}