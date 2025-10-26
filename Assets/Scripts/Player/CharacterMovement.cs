using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public float jumpHeight = 2.0f;
    private CharacterController controller;
    private Vector3 velocity = Vector3.zero;
    private float gravity = 9.81f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller.isGrounded)
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            // движение относительно поворота персонажа
            Vector3 move = transform.right * moveHorizontal + transform.forward * moveVertical;
            move = move.normalized * speed;

            // сохран€ем вертикальную составл€ющую отдельно
            velocity.x = move.x;
            velocity.z = move.z;

            if (Input.GetButton("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * 2.0f * gravity);
            }
            else
            {
                velocity.y = -0.1f; // небольша€ "прилипша€" сила, чтобы CharacterController считалс€ grounded
            }
        }

        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
