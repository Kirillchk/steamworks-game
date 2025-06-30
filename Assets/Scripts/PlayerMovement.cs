using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float jumpForce = 5f;
    public float lookSpeedX = 2f;
    public float lookSpeedY = 2f;
    public Transform playerCamera;
    float moveX;
    float moveZ;
    float prevMoveX;
    float prevMoveZ;
    private Rigidbody rb;
    private float rotationX = 0f;
    private bool isGrounded;
    [SerializeField] bool sprint = false;
    [SerializeField] Vector3 moveDirection;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float maxDistance = 1;
    [SerializeField] Vector3 gravity;
    [SerializeField] float gravityScale;
    [SerializeField] float acceleration = 1;
    [SerializeField] float speedLimit = 5;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        gravity = Physics.gravity;
        // Handle mouse look (for rotating camera)
        float mouseX = Input.GetAxis("Mouse X") * lookSpeedX;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeedY;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);


        if (Physics.Raycast(transform.position, Vector3.down, maxDistance, groundLayer))
            isGrounded = true;
        else
            isGrounded = false;

        // Handle movement (WASD)
        moveX = Input.GetAxis("Horizontal");  // A/D or Left/Right arrow keys
        moveZ = Input.GetAxis("Vertical");    // W/S or Up/Down arrow keys
        if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && isGrounded)
            moveX = 0;
        if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) && isGrounded)
            moveZ = 0;

        sprint = Input.GetKey(KeyCode.LeftShift);
        // if (sprint) moveSpeed = 10;
        // else moveSpeed = 5;
        if (moveSpeed < speedLimit && ((Input.GetKey(KeyCode.W) ^ Input.GetKey(KeyCode.S)) || (Input.GetKey(KeyCode.A) ^ Input.GetKey(KeyCode.D))))
            moveSpeed += acceleration;
        if (isGrounded && !((Input.GetKey(KeyCode.W) ^ Input.GetKey(KeyCode.S)) || (Input.GetKey(KeyCode.A) ^ Input.GetKey(KeyCode.D))))
            moveSpeed = 1;





        if (!isGrounded)
        {
            Physics.gravity += Vector3.up * gravityScale;
            if (moveX == 0)
                moveX = prevMoveX;

            if (moveZ == 0)
                moveZ = prevMoveZ;
            moveSpeed -= acceleration * 0.5f;
        }
        else
        {
            // Physics.gravity = Vector3.up * -3;
            if (moveX == 0 && moveZ == 0)
                moveSpeed -= acceleration;
        }
        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            rb.linearVelocity += Vector3.up * jumpForce;
        }
        if (moveSpeed < 1)
            moveSpeed = 1;



        moveDirection = (transform.right * moveX + transform.forward * moveZ).normalized;

        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        Debug.Log(rb.linearVelocity);
        prevMoveX = moveX;
        prevMoveZ = moveZ;
    }
}