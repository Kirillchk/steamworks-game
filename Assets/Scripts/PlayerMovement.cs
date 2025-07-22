using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float jumpForce = .6f,
		moveSpeed = 15, walkSpeed = 15, runMultiplier = 2, acceler = 2,
		groundDamping = 5, airDamping = 1;
	float rotationX = 0f;
	internal Transform playerCamera;
	Rigidbody rb;
	void Start()
	{
		playerCamera = Camera.main.transform;
		rb = GetComponent<Rigidbody>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
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

		bool isGrounded = Physics.Raycast(
			transform.position,
			Vector3.down,
			1.1f,
			LayerMask.GetMask("Default")
		);

		rb.linearDamping = isGrounded ? groundDamping : airDamping;
		// Handle movement (WASD)
		var move = new Vector3(
				Input.GetAxis("Horizontal"),
				0,
				Input.GetAxis("Vertical")
			).normalized * acceler;

		var speedLimit = Input.GetKey(KeyCode.LeftShift) ? walkSpeed * runMultiplier : walkSpeed;

		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
			rb.linearVelocity += Vector3.up * jumpForce;

		Vector3 vector3 =
			playerCamera.forward * move.z +
			playerCamera.right * move.x;
		vector3.y = 0;

		if ((rb.linearVelocity + vector3).magnitude <= speedLimit)
			rb.linearVelocity += vector3;
		if (move == Vector3.zero)
			rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new(0, rb.linearVelocity.y), 1);
	
	}
}