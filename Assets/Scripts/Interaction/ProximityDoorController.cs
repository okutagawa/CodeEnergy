using UnityEngine;

[DisallowMultipleComponent]
public class ProximityDoorController : MonoBehaviour
{
    [Header("Gate")]
    [SerializeField] private TaskCompletionGate unlockGate;

    [Header("Door animation")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openBoolParameter = "IsOpen";
    [Tooltip("Optional animator state that represents the fully closed door pose. Leave empty if the controller already starts closed when IsOpen is false.")]
    [SerializeField] private string closedStateName = "";
    [SerializeField] private int animatorLayer = 0;
    [SerializeField] private bool disableAnimatorWhileLocked = true;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float openDistance = 3f;
    [SerializeField] private float closeDistance = 4f;

    private bool _isOpen;
    private bool _wasUnlocked;

    private void Awake()
    {
        if (unlockGate == null)
            unlockGate = GetComponent<TaskCompletionGate>();

        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        GameState.OnTaskCompleted += HandleTaskCompleted;
        RefreshLockState(forceClosed: true);
    }

    private void Start()
    {
        if (player == null)
        {
            var taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
                player = taggedPlayer.transform;
        }

        RefreshLockState(forceClosed: true);
    }

    private void OnDisable()
    {
        GameState.OnTaskCompleted -= HandleTaskCompleted;
    }

    private void Update()
    {
        bool unlocked = IsUnlocked();
        if (!unlocked)
        {
            if (_wasUnlocked || _isOpen)
                ForceClosed();

            _wasUnlocked = false;
            return;
        }

        if (!_wasUnlocked)
        {
            EnableAnimator();
            ForceClosed();
            _wasUnlocked = true;
        }

        if (player == null || doorAnimator == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (!_isOpen && distance <= openDistance)
        {
            SetOpen(true);
        }
        else if (_isOpen && distance >= closeDistance)
        {
            SetOpen(false);
        }
    }

    private void HandleTaskCompleted(int taskId)
    {
        RefreshLockState(forceClosed: true);
    }

    private void RefreshLockState(bool forceClosed)
    {
        bool unlocked = IsUnlocked();
        _wasUnlocked = unlocked;

        if (unlocked)
        {
            EnableAnimator();
        }
        else if (forceClosed)
        {
            ForceClosed();
        }
    }

    private bool IsUnlocked()
    {
        return unlockGate != null && unlockGate.IsUnlocked();
    }

    private void SetOpen(bool open)
    {
        EnableAnimator();
        _isOpen = open;
        doorAnimator.SetBool(openBoolParameter, open);
    }

    private void ForceClosed()
    {
        _isOpen = false;
        if (doorAnimator == null)
            return;

        bool wasEnabled = doorAnimator.enabled;
        doorAnimator.enabled = true;
        doorAnimator.SetBool(openBoolParameter, false);

        if (!string.IsNullOrWhiteSpace(closedStateName))
            doorAnimator.Play(closedStateName, animatorLayer, 0f);

        doorAnimator.Update(0f);

        if (disableAnimatorWhileLocked && !IsUnlocked())
            doorAnimator.enabled = false;
        else
            doorAnimator.enabled = wasEnabled || IsUnlocked();
    }

    private void EnableAnimator()
    {
        if (doorAnimator != null && !doorAnimator.enabled)
            doorAnimator.enabled = true;
    }
}