using UnityEngine;

[DisallowMultipleComponent]
public class GeneratorStateByTasks : MonoBehaviour
{
    [Header("Required progress")]
    [SerializeField] private TaskCompletionGate workGate;

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
        RefreshState();
    }

    private void Start()
    {
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
        bool shouldWork = workGate != null && workGate.IsUnlocked();

        if (_isWorking == shouldWork)
        {
            ApplyState();
            return;
        }

        _isWorking = shouldWork;
        ApplyState();
    }

    private void ApplyState()
    {
        if (generatorAnimator != null)
        {
            generatorAnimator.SetBool(isWorkingParameterName, _isWorking);
        }
    }
}