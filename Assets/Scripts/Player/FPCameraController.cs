using UnityEngine;
using UnityEngine.EventSystems;

public class FPCameraController : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 200f;
    [SerializeField] private Transform _player;
    [SerializeField] private float _verticalMin = -80f;
    [SerializeField] private float _verticalMax = 80f;
    private float _currentVerticalAngle;

    // Если у тебя есть UIManager с IsUiActive или аналогом, включи эту проверку
    [SerializeField] private bool useUiManagerCheck = true;

    void Start()
    {
        LockCursor(true);
    }

    void Update()
    {
        // Не обрабатываем ввод если указатель над UI
        if (EventSystem.current != null)
        {
#if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                EnsureCursorStateForUi(true);
                return;
            }
#else
            if (EventSystem.current.IsPointerOverGameObject())
            {
                EnsureCursorStateForUi(true);
                return;
            }
#endif
        }

        // Опциональная проверка на открытые панели через UIManager
        if (useUiManagerCheck && UIManager.Instance != null)
        {
            // Реализуй в UIManager метод IsUiActive() или используй существующий флаг.
            // Здесь предполагается, что UIManager имеет публичный метод IsAnyPanelOpen(), иначе реализуй его.
            var method = typeof(UIManager).GetMethod("IsAnyPanelOpen");
            if (method != null)
            {
                var isOpen = (bool)method.Invoke(UIManager.Instance, null);
                if (isOpen)
                {
                    EnsureCursorStateForUi(true);
                    return;
                }
            }
        }

        // Если дошли сюда — обрабатываем управление камерой и скрываем курсор
        EnsureCursorStateForUi(false);

        var mouseY = -Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;
        var mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;

        _currentVerticalAngle = Mathf.Clamp(_currentVerticalAngle + mouseY, _verticalMin, _verticalMax);
        transform.localRotation = Quaternion.Euler(_currentVerticalAngle, 0f, 0f);

        if (_player != null) _player.Rotate(Vector3.up * mouseX);
    }

    private void EnsureCursorStateForUi(bool uiActive)
    {
        if (uiActive)
        {
            LockCursor(false);
        }
        else
        {
            LockCursor(true);
        }
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
