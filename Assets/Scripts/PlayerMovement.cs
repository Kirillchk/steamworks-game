using System;
using System.Collections;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float jumpForce = .6f,
		walkSpeed = 7, runMultiplier = 2, acceler = 2,
		groundDamping = 5, airDamping = 5, penalty = 1f,
		timeToLimit = 1;
	float rotationX = 0f;
	public Vector3 move, wishDir, transVec, velToAdd = Vector3.zero, velFromWASD, vel;

	public ForceMode forceMode = ForceMode.Acceleration;
	public float g = -9.81f;
	[SerializeField] bool isGrounded;
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
		isGrounded = Physics.Raycast(
			transform.position,
			Vector3.down,
			1.1f,
			LayerMask.GetMask("Default")
		);
		if (isGrounded)
			Physics.gravity = Vector3.up * -1;
		if (!isGrounded)
			Physics.gravity = Vector3.up * -9.81f;
		g=Physics.gravity.y;
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
			rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

		// rb.linearDamping = isGrounded ? groundDamping : airDamping;

		move = new Vector3(
				Input.GetAxisRaw("Horizontal"),
				0,
				Input.GetAxisRaw("Vertical")
			).normalized;
		wishDir = //wasd fix
			(transform.right * move.x +
			transform.forward * move.z).normalized;

		var speedLimit = Input.GetKey(KeyCode.LeftShift) ?
		isGrounded ? walkSpeed * runMultiplier : walkSpeed / penalty * runMultiplier
		: isGrounded ? walkSpeed : walkSpeed / penalty;

		if ((rb.linearVelocity + wishDir).magnitude <= speedLimit)
			rb.AddForce(wishDir, ForceMode.VelocityChange);
		float drag = Mathf.Clamp01(1f - groundDamping * Time.deltaTime);
		rb.linearVelocity = new Vector3(rb.linearVelocity.x * drag, rb.linearVelocity.y, rb.linearVelocity.z * drag);

		// rb.linearVelocity = rb.linearVelocity-
		vel = rb.linearVelocity;
		transVec = transform.forward + transform.right;
		
	}
	private void OnDrawGizmos()
	{
		if (rb != null)
		{
			Vector3 startPos = transform.position;
			Vector3 endPos = transform.position + rb.linearVelocity;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(startPos, endPos);
			// Debug.Log(rb.linearVelocity);
			// float rad = Mathf.Deg2Rad * 30;
			// Vector3 vec3 = new Vector3(
			// 	Mathf.Cos(rad) * endPos.x + Mathf.Sin(rad) * endPos.x,
			// 	1 * endPos.y,
			// 	-Mathf.Sin(rad) * endPos.z + Mathf.Cos(rad) * endPos.z);
			// Gizmos.DrawLine(endPos, vec3);
			
		}

	}
}