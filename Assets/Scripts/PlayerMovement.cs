using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float lookSpeedX = 2f;
    public float lookSpeedY = 2f;
    public Transform playerCamera;

    private Rigidbody rb;
    private float rotationX = 0f;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Handle mouse look (for rotating camera)
        float mouseX = Input.GetAxis("Mouse X") * lookSpeedX;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeedY;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Handle movement (WASD)
        float moveX = Input.GetAxis("Horizontal");  // A/D or Left/Right arrow keys
        float moveZ = Input.GetAxis("Vertical");    // W/S or Up/Down arrow keys
        Vector3 moveDirection = (transform.right * moveX + transform.forward * moveZ).normalized;

        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionStay()
    {
        isGrounded = true;
    }

    void OnCollisionExit()
    {
        isGrounded = false;
    }
}