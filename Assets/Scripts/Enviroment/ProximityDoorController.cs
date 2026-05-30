using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class ProximityDoorController : MonoBehaviour
{
    private enum DoorMotionMode
    {
        AnimatorBool,
        LocalRotation
    }

    [Header("Gate")]
    [SerializeField] private TaskCompletionGate unlockGate;

    [Header("Motion mode")]
    [Tooltip("AnimatorBool uses your existing Animator once per open/close request. LocalRotation rotates a door pivot in code and does not require a separate close clip.")]
    [SerializeField] private DoorMotionMode motionMode = DoorMotionMode.AnimatorBool;

    [Header("Animator motion")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openBoolParameter = "IsOpen";
    [Tooltip("Optional animator state that represents the fully closed door pose. Leave empty if the controller already starts closed when IsOpen is false.")]
    [SerializeField] private string closedStateName = "";
    [SerializeField] private int animatorLayer = 0;
    [SerializeField] private float animatorOpenDuration = 1f;
    [SerializeField] private float animatorCloseDuration = 1f;
    [SerializeField] private bool disableAnimatorWhenIdle = true;

    [Header("Procedural rotation motion")]
    [Tooltip("Pivot to rotate when Motion Mode is LocalRotation. If empty, this object's transform is used.")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private Vector3 closedLocalEulerAngles;
    [SerializeField] private Vector3 openLocalEulerAngles = new Vector3(0f, 90f, 0f);
    [SerializeField] private float proceduralOpenDuration = 0.8f;
    [SerializeField] private float proceduralCloseDuration = 0.8f;
    [SerializeField] private AnimationCurve proceduralCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float openDistance = 3f;
    [SerializeField] private float closeDistance = 4f;

    private bool _isOpen;
    private bool _wasUnlocked;
    private Coroutine _motionRoutine;
    private Quaternion _closedLocalRotation;
    private Quaternion _openLocalRotation;

    private void Awake()
    {
        if (unlockGate == null)
            unlockGate = GetComponent<TaskCompletionGate>();

        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

        if (doorPivot == null)
            doorPivot = transform;

        _closedLocalRotation = Quaternion.Euler(closedLocalEulerAngles);
        _openLocalRotation = Quaternion.Euler(openLocalEulerAngles);
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
            ForceClosed();
            _wasUnlocked = true;
        }

        if (player == null)
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

        if (!unlocked && forceClosed)
        {
            ForceClosed();
        }
        else if (disableAnimatorWhenIdle)
        {
            DisableAnimator();
        }
    }

    private bool IsUnlocked()
    {
        return unlockGate != null && unlockGate.IsUnlocked();
    }

    private void SetOpen(bool open)
    {
        if (_isOpen == open)
            return;

        _isOpen = open;

        if (_motionRoutine != null)
            StopCoroutine(_motionRoutine);

        if (motionMode == DoorMotionMode.LocalRotation)
        {
            _motionRoutine = StartCoroutine(PlayProceduralRotation(open));
        }
        else
        {
            _motionRoutine = StartCoroutine(PlayAnimatorBool(open));
        }
    }

    private IEnumerator PlayAnimatorBool(bool open)
    {
        if (doorAnimator == null)
            yield break;

        EnableAnimator();
        doorAnimator.SetBool(openBoolParameter, open);

        float duration = Mathf.Max(0f, open ? animatorOpenDuration : animatorCloseDuration);
        if (duration > 0f)
            yield return new WaitForSeconds(duration);

        if (disableAnimatorWhenIdle)
            DisableAnimator();

        _motionRoutine = null;
    }

    private IEnumerator PlayProceduralRotation(bool open)
    {
        if (doorPivot == null)
            yield break;

        DisableAnimator();

        Quaternion from = doorPivot.localRotation;
        Quaternion to = open ? _openLocalRotation : _closedLocalRotation;
        float duration = Mathf.Max(0.01f, open ? proceduralOpenDuration : proceduralCloseDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float curvedProgress = proceduralCurve != null ? proceduralCurve.Evaluate(progress) : progress;
            doorPivot.localRotation = Quaternion.Slerp(from, to, curvedProgress);
            yield return null;
        }

        doorPivot.localRotation = to;
        _motionRoutine = null;
    }

    private void ForceClosed()
    {
        _isOpen = false;

        if (_motionRoutine != null)
        {
            StopCoroutine(_motionRoutine);
            _motionRoutine = null;
        }

        if (motionMode == DoorMotionMode.LocalRotation)
        {
            DisableAnimator();
            if (doorPivot != null)
                doorPivot.localRotation = _closedLocalRotation;
            return;
        }

        if (doorAnimator == null)
            return;

        EnableAnimator();
        doorAnimator.SetBool(openBoolParameter, false);

        if (!string.IsNullOrWhiteSpace(closedStateName))
            doorAnimator.Play(closedStateName, animatorLayer, 0f);

        doorAnimator.Update(0f);

        if (disableAnimatorWhenIdle)
            DisableAnimator();
    }

    private void EnableAnimator()
    {
        if (doorAnimator != null && !doorAnimator.enabled)
            doorAnimator.enabled = true;
    }

    private void DisableAnimator()
    {
        if (doorAnimator != null && doorAnimator.enabled)
            doorAnimator.enabled = false;
    }
}