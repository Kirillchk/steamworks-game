using Unity.Mathematics;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
	public float jumpForce = 8,
		walkSpeed = 10, runMultiplier = 2, acceler,
		daming = 5, penalty = 1, gravityScale = 3,
		rayCastLengh = 1.1f, magnitude, g = -9.81f,
		speedLimit, drag;
	float rotationX = 0f;
	public Vector3 move, wishDir, vel;
	public bool isGrounded, jump;
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
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
			jump = true;
		speedLimit = Input.GetKey(KeyCode.LeftShift) ?
			isGrounded ? walkSpeed * runMultiplier : walkSpeed / penalty * runMultiplier
			: isGrounded ? walkSpeed : walkSpeed / penalty;
	}
    void FixedUpdate()
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

		move = new Vector3(
			Input.GetAxisRaw("Horizontal"),
			0,
			Input.GetAxisRaw("Vertical")
		).normalized;
		acceler = speedLimit / (drag * 10);
		wishDir = //wasd fix
			(transform.right * move.x +
			transform.forward * move.z).normalized * acceler;
			
		if (new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude < speedLimit && move!=Vector3.zero)
		{
			if ((new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) + wishDir).magnitude < speedLimit)
			{
				rb.AddForce(wishDir, ForceMode.VelocityChange);
			}
			else
			{
				//add speed exactly to limit
				float root = Mathf.Sqrt(wishDir.x * wishDir.x + wishDir.z * wishDir.z);
				if (root != 0)
				{
					float mult = (speedLimit / drag - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude) / root;
					rb.AddForce(wishDir * mult, ForceMode.VelocityChange);
				}
			}
		}
		else
		{
			//wasd keeps a litle speed after limit
			float root = Mathf.Sqrt(wishDir.x * wishDir.x + wishDir.z * wishDir.z);
			if (drag != 0 && root != 0)
			{
				float dif = speedLimit / drag - speedLimit;
				float mult = dif / root;
				rb.AddForce(wishDir * mult, ForceMode.VelocityChange);
			}
		}
		
		rb.linearVelocity = new Vector3(rb.linearVelocity.x * drag, rb.linearVelocity.y, rb.linearVelocity.z * drag);

		vel = rb.linearVelocity;
		magnitude = (float)System.Math.Round(rb.linearVelocity.magnitude, 2);
    }
    private void OnDrawGizmos()
	{
		if (rb != null)
		{
			Vector3 startPos = transform.position;
			Vector3 endPos = transform.position + rb.linearVelocity;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(startPos, endPos);
		}

	}
}