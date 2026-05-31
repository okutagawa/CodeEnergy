using UnityEngine;

public class CursorUIManager : MonoBehaviour
{
    public static CursorUIManager Instance { get; private set; }

    // Счётчик активных UI (модальные панели, диалоги, квизы и т.п.)
    private int _uiFocusCount = 0;

    // Сохранение предыдущего состояния курсора для корректного возврата
    private CursorLockMode _prevLockState = CursorLockMode.Locked;
    private bool _prevVisible = false;
    private bool _hasPrev = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Базовое состояние: игра управляется мышью, курсор скрыт и залочен
        ApplyCursor(visible: false, lockMode: CursorLockMode.Locked);
    }

    // Включение UI-режима: курсор видим по всему экрану, разблокирован; камера должна игнорировать ввод
    public void EnterUiFocus()
    {
        if (!_hasPrev)
        {
            _prevLockState = Cursor.lockState;
            _prevVisible = Cursor.visible;
            _hasPrev = true;
        }
        _uiFocusCount = Mathf.Max(0, _uiFocusCount) + 1;
        ApplyCursor(visible: true, lockMode: CursorLockMode.None);
    }

    // Выход из UI-режима: уменьшаем счётчик; если он ноль — восстанавливаем прошлое состояние
    public void ExitUiFocus()
    {
        if (_uiFocusCount <= 0)
        {
            _uiFocusCount = 0;
            return;
        }
        _uiFocusCount--;

        if (_uiFocusCount == 0 && _hasPrev)
        {
            ApplyCursor(_prevVisible, _prevLockState);
            _hasPrev = false;
        }
    }

    // Принудительное восстановление (например, при смене сцены)
    public void ForceRestore()
    {
        _uiFocusCount = 0;
        if (_hasPrev)
        {
            ApplyCursor(_prevVisible, _prevLockState);
            _hasPrev = false;
        }
        else
        {
            ApplyCursor(visible: false, lockMode: CursorLockMode.Locked);
        }
    }

    // Защитный хук: если при активном UI кто-то внезапно залочил курсор — вернём состояние
    void LateUpdate()
    {
        if (_uiFocusCount > 0)
        {
            if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
            {
                ApplyCursor(visible: true, lockMode: CursorLockMode.None);
            }
        }
    }

    private void ApplyCursor(bool visible, CursorLockMode lockMode)
    {
        Cursor.visible = visible;
        Cursor.lockState = lockMode;
    }

    public bool IsUiFocused => _uiFocusCount > 0;
    public int GetFocusCount() => _uiFocusCount;
}
