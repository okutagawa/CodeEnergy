using UnityEngine;
using System.Diagnostics;

public class CursorUIManager : MonoBehaviour
{
    public static CursorUIManager Instance { get; private set; }

    private int _requests = 0;
    private CursorLockMode _prevLockState;
    private bool _prevVisible;
    private bool _hasSavedPrevState = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private float _lastStackLog = -10f;
    private void Update()
    {
        // ≈сли есть активные запросы Ч убеждаемс€, что курсор видим (защищает от случайных внешних переключений)
        if (_requests > 0)
        {
            if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
            {
                // Throttle stacktrace logs to once per second to avoid spam
                if (Time.unscaledTime - _lastStackLog > 1f)
                {
                    _lastStackLog = Time.unscaledTime;
                    UnityEngine.Debug.LogWarning($"[CursorUIManager] Forcing cursor visible. Before: visible={Cursor.visible}, lockState={Cursor.lockState}, requests={_requests}\nStack:\n{new StackTrace(1, true)}");
                }
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // ¬ызвать, когда UI хочет показать курсор (инкремент)
    public void ShowCursor()
    {
        if (!_hasSavedPrevState)
        {
            _prevLockState = Cursor.lockState;
            _prevVisible = Cursor.visible;
            _hasSavedPrevState = true;
        }

        _requests = Mathf.Max(0, _requests) + 1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ¬ызвать, когда UI больше не нуждаетс€ в курсоре (декремент)
    public void HideCursor()
    {
        _requests = Mathf.Max(0, _requests - 1);
        if (_requests == 0 && _hasSavedPrevState)
        {
            Cursor.lockState = _prevLockState;
            Cursor.visible = _prevVisible;
            _hasSavedPrevState = false;
        }
    }

    // ѕринудительное восстановление состо€ни€ (очищает все запросы)
    public void ForceRestore()
    {
        _requests = 0;
        if (_hasSavedPrevState)
        {
            Cursor.lockState = _prevLockState;
            Cursor.visible = _prevVisible;
            _hasSavedPrevState = false;
        }
    }

    private void LateUpdate()
    {
        if (_requests > 0)
        {
            if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
            {
                if (Time.unscaledTime - _lastStackLog > 1f)
                {
                    _lastStackLog = Time.unscaledTime;
                    UnityEngine.Debug.LogWarning($"[CursorUIManager] LateUpdate force. Before: visible={Cursor.visible}, lockState={Cursor.lockState}, requests={_requests}\nStack:\n{new StackTrace(1, true)}");
                }
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (_requests > 0 && hasFocus)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public int GetRequestCount() => _requests;
}
