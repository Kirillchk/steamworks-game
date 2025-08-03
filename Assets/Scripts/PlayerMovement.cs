using Unity.Mathematics;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
	public float jumpForce = 8,
		walkSpeed = 10, runMultiplier = 2, acceler,
		daming = 5, penalty = 1, gravityScale = 3,
		rayCastLengh = 1.1f, magnitude, g = -9.81f,
		speedLimit, drag;
	public bool isGrounded, jump;
	float rotationX = 0f, mouseX, mouseY;
	Vector3 move;
	Rigidbody rb;
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
			jump = true;
		speedLimit = Input.GetKey(KeyCode.LeftShift) ?
			isGrounded ? walkSpeed * runMultiplier : walkSpeed / penalty * runMultiplier
			: isGrounded ? walkSpeed : walkSpeed / penalty;
		move = new(
			Input.GetAxisRaw("Horizontal"),
			0,
			Input.GetAxisRaw("Vertical")
		);
		move = move.normalized;
		mouseX = Input.GetAxis("Mouse X") * 2;
		mouseY = Input.GetAxis("Mouse Y") * 2;
	}
    void FixedUpdate()
    {
        // Handle mouse look (for rotating camera)
		rotationX -= mouseY;
		rotationX = Mathf.Clamp(rotationX, -90f, 90f);
		Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
		transform.Rotate(Vector3.up * mouseX);

		isGrounded = Physics.Raycast(
			transform.position,
			Vector3.down,
			rayCastLengh,
			LayerMask.GetMask("Default")
		);

		g = -9.81f * gravityScale;
		Physics.gravity = Vector3.up * g;
		if (!isGrounded)
			jump = false;
		if (jump)
			rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

		drag = Mathf.Clamp01(1f - daming * Time.deltaTime);

		acceler = speedLimit / (drag * 10);
		var wishDir = //wasd fix
			(transform.right * move.x +
			transform.forward * move.z).normalized * acceler;
		var XZSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
		float root = wishDir.magnitude;
		if (XZSpeed.magnitude < speedLimit && move != Vector3.zero)
		{
			if ((XZSpeed + wishDir).magnitude < speedLimit)
				rb.AddForce(wishDir, ForceMode.VelocityChange);
			else if (root != 0)
			{
				//add speed exactly to limit
				float mult = (speedLimit / drag - XZSpeed.magnitude) / root;
				rb.AddForce(wishDir * mult, ForceMode.VelocityChange);
			}
		}
		else if (drag != 0 && root != 0)
		{
			//wasd keeps a litle speed after limit
			float dif = speedLimit / drag - speedLimit;
			float mult = dif / root;
			rb.AddForce(wishDir * mult, ForceMode.VelocityChange);
		}
		
		rb.linearVelocity = new (rb.linearVelocity.x * drag, rb.linearVelocity.y, rb.linearVelocity.z * drag);
		magnitude = (float)System.Math.Round(rb.linearVelocity.magnitude, 2);
    }
    private void OnDrawGizmos()
	{
		if (rb == null) return;
		Vector3 startPos = transform.position;
		Vector3 endPos = transform.position + rb.linearVelocity;
		Gizmos.color = Color.red;
		Gizmos.DrawLine(startPos, endPos);
	}
}