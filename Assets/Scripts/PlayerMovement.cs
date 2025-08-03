using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float jumpForce = .6f,
		walkSpeed = 15, runMultiplier = 2, acceler = 2,
		groundDamping = 5, airDamping = 1, distance = 0.1f;
	float rotationX = 0f;
	[SerializeField]bool isGrounded;
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

		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
			rb.linearVelocity += Vector3.up * jumpForce;

		rb.linearDamping = isGrounded ? groundDamping : airDamping;
		
		var move = new Vector3(
				Input.GetAxis("Horizontal"),
				0,
				Input.GetAxis("Vertical")
			).normalized * acceler;

		Vector3 cameraVec = //wasd fix
			playerCamera.right * move.x +
			playerCamera.forward * move.z;
		cameraVec.y = 0;

		var speedLimit = Input.GetKey(KeyCode.LeftShift) ? walkSpeed * runMultiplier : walkSpeed;

		if ((rb.linearVelocity + cameraVec).magnitude <= speedLimit)
			rb.linearVelocity += cameraVec;
		if (move == Vector3.zero)
			rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, new(0, rb.linearVelocity.y), distance);
	}
}