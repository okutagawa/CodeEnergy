using UnityEngine;

public class FPCameraController : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 200f;
    [SerializeField] private Transform _player;
    [SerializeField] private float _verticalMin = -80f;
    [SerializeField] private float _verticalMax = 80f;
    private float _currentVerticalAngle;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Обычно не умножают на Time.deltaTime для мыши, но можно делать это если подбираете чувствительность
        var mouseY = -Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;
        var mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;

        _currentVerticalAngle = Mathf.Clamp(_currentVerticalAngle + mouseY, _verticalMin, _verticalMax);
        transform.localRotation = Quaternion.Euler(_currentVerticalAngle, 0f, 0f);

        _player.Rotate(Vector3.up * mouseX);
    }
}
