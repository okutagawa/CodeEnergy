using UnityEngine;

public class FPCameraController : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 200f;
    [SerializeField] private Transform _player;
    [SerializeField] private float _verticalMin = -80f;
    [SerializeField] private float _verticalMax = 80f;
    private float _currentVerticalAngle;

    // pause flag
    private bool isPaused = false;

    void Start()
    {
        // Базовое состояние — игровое управление, курсор скрыт и залочен
        if (CursorUIManager.Instance != null)
            CursorUIManager.Instance.ForceRestore();
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // Если активен UI-режим — игнорируем ввод мыши для камеры
        if (isPaused) return; // <-- игнорируем ввод в паузе

        if (CursorUIManager.Instance != null && CursorUIManager.Instance.IsUiFocused)
            return;

        var mouseY = -Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;
        var mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;

        _currentVerticalAngle = Mathf.Clamp(_currentVerticalAngle + mouseY, _verticalMin, _verticalMax);
        transform.localRotation = Quaternion.Euler(_currentVerticalAngle, 0f, 0f);

        if (_player != null)
            _player.Rotate(Vector3.up * mouseX);
    }

    public void OnGamePaused(bool paused)
    {
        isPaused = paused;

        // при паузе разблокируем курсор, при возобновлении — вернуть состояние
        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (CursorUIManager.Instance != null)
                CursorUIManager.Instance.ForceRestore();
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
