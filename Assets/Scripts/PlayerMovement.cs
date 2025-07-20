using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float moveSpeed = 1f;
	public float jumpForce = .6f;
	public Transform playerCamera;
	private Rigidbody rb;
	private float rotationX = 0f;
	[SerializeField] private bool isGrounded;
	[SerializeField] Vector3 moveDirection;
	[SerializeField] LayerMask groundLayer;
	[SerializeField] float maxDistance = 1.3f;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		groundLayer = LayerMask.GetMask("Default");
	}

	void Update()
	{
		// Handle mouse look (for rotating camera)
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");

		rotationX -= mouseY;
		rotationX = Mathf.Clamp(rotationX, -90f, 90f);
		playerCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
		transform.Rotate(Vector3.up * mouseX);

		if (Physics.Raycast(transform.position, Vector3.down, maxDistance, groundLayer))
			isGrounded = true;
		else
			isGrounded = false;

		if (!isGrounded)
			rb.linearDamping = 1;
		else
			rb.linearDamping = 5;	
		// Handle movement (WASD)

		float speed = 10;
		var move = new Vector3(
				Input.GetAxis("Horizontal"),
				0,
				Input.GetAxis("Vertical")
			).normalized*speed/20;
		if (Input.GetKey(KeyCode.LeftShift))
			speed =15;
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
			rb.linearVelocity += Vector3.up * jumpForce*2;

		Vector3 vector3 = playerCamera.forward * move.z + playerCamera.right * move.x;
		vector3.y = 0;
		if ((rb.linearVelocity + vector3).magnitude <= speed)
			rb.linearVelocity += vector3;
	
	}
}