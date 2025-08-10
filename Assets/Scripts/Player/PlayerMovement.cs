using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
	public float jumpForce = 8,
		walkSpeed = 10, runMultiplier = 2, acceler,
		daming = 5, penalty = 1, gravityScale = 3,
		rayCastLengh = 1.01f, magnitude, g = -9.81f,
		speedLimit, drag, jumpBufferTime = 0.1f, jumpBufferCount = 0,
		staminaRegPerSecond= 10;
	public float sprintPerSecond = -10;
	public bool isGrounded, jump;
	public Vector3 move, wishDir, vel;
	float rotationX = 0f, mouseX, mouseY;
	int staminaForJump = -10;
	Rigidbody rb;
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		speedLimit = walkSpeed;
	}
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			jumpBufferCount = jumpBufferTime;
		jumpBufferCount -= Time.deltaTime;
		if (jumpBufferCount > 0)
			jump = true;
		else
			jump = false;


		move = new(
			Input.GetAxisRaw("Horizontal"),
			0,
			Input.GetAxisRaw("Vertical")
		);
		move = move.normalized;
		mouseX = Input.GetAxis("Mouse X") * 2;
		mouseY = Input.GetAxis("Mouse Y") * 2;
		if (Input.GetKey(KeyCode.LeftShift) && move != Vector3.zero)
		{
			//sprint

			speedLimit = walkSpeed * runMultiplier;
			if (!StaminaSystem.staminaPerSecondList.Contains(("sprint", sprintPerSecond, null)))
				StaminaSystem.staminaPerSecondList.Add(("sprint", sprintPerSecond, null));
		}
		else
		{
			//dont sprint
			speedLimit = walkSpeed;
			if(StaminaSystem.staminaPerSecondList.Contains(("sprint", sprintPerSecond, null)))
				StaminaSystem.staminaPerSecondList.Remove(("sprint", sprintPerSecond, null));
		}
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

		if (jump && isGrounded)
		{
			rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
			rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
			jump = false;
			jumpBufferCount = 0;
			StaminaSystem.Instant(staminaForJump);
		}

		drag = Mathf.Clamp01(1f - daming * Time.deltaTime);

		acceler = speedLimit / (drag * 10);
		wishDir = //wasd fix
			(transform.right * move.x +
			transform.forward * move.z).normalized * acceler;
		var XZSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
		float root = wishDir.magnitude;

		
		if (XZSpeed.magnitude < speedLimit && move != Vector3.zero)
		{
			if ((XZSpeed + wishDir).magnitude < speedLimit)
				rb.AddForce(isGrounded ? wishDir : wishDir / penalty, ForceMode.VelocityChange);
			else if (root != 0)
			{
				//add speed exactly to limit
				float mult = (speedLimit / drag - XZSpeed.magnitude) / root;
				rb.AddForce(isGrounded ? wishDir * mult : wishDir * mult / penalty, ForceMode.VelocityChange);
			}
		}
		else if (XZSpeed.normalized != wishDir.normalized && wishDir != Vector3.zero)
		{
			var velToAdd = wishDir.normalized - XZSpeed.normalized;
			rb.AddForce(isGrounded ? velToAdd : velToAdd / penalty, ForceMode.VelocityChange);
		}
		else if (drag != 0 && root != 0 && isGrounded)
		{
			//wasd keeps a litle speed after limit
			float dif = speedLimit / drag - speedLimit;
			float mult = dif / root;
			rb.AddForce(isGrounded ? wishDir * mult : wishDir * mult / penalty, ForceMode.VelocityChange);
		}
		if (!isGrounded)
			drag = 1;

		rb.linearVelocity = new(rb.linearVelocity.x * drag, rb.linearVelocity.y, rb.linearVelocity.z * drag);
		magnitude = (float)System.Math.Round(rb.linearVelocity.magnitude, 2);
		vel = new Vector3((float)System.Math.Round(rb.linearVelocity.x,2), (float)System.Math.Round(rb.linearVelocity.y,2),
		(float)System.Math.Round(rb.linearVelocity.z,2));
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