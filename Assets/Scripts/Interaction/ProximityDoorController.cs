using UnityEngine;

[DisallowMultipleComponent]
public class ProximityDoorController : MonoBehaviour
{
    [Header("Gate")]
    [SerializeField] private TaskCompletionGate unlockGate;

    [Header("Door animation")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openBoolParameter = "IsOpen";

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float openDistance = 3f;
    [SerializeField] private float closeDistance = 4f;

    private bool _isOpen;

    private void Awake()
    {
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (player == null)
        {
            var taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
                player = taggedPlayer.transform;
        }

        ForceClosed();
    }

    private void OnEnable()
    {
        GameState.OnTaskCompleted += HandleTaskCompleted;
    }

    private void OnDisable()
    {
        GameState.OnTaskCompleted -= HandleTaskCompleted;
    }

    private void Update()
    {
        if (!IsUnlocked() || player == null || doorAnimator == null)
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
        if (!IsUnlocked())
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
        _isOpen = open;
        doorAnimator.SetBool(openBoolParameter, open);
    }

    private void ForceClosed()
    {
        _isOpen = false;
        if (doorAnimator != null)
            doorAnimator.SetBool(openBoolParameter, false);
    }
}