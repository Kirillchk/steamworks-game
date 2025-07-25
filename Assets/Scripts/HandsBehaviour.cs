using UnityEngine;

public class HandsBehaviour : MonoBehaviour
{
    SpringJoint spring;
    public Camera playerCamera;
	Rigidbody rb;
	void Start() => rb = GetComponent<Rigidbody>();
	void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CastRayFromCameraCenter();
        }
		if(!Input.GetMouseButton(0)) // Left mouse button released
        {
            Releasespring();
        }
		if (Input.GetMouseButton(1))
		{
			Quaternion targetRotation = Quaternion.Euler(0, rb.rotation.eulerAngles.y, 0);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 0.1f);
		}

        // Continuously draw the spring connection in Play mode
		if (spring != null && spring.connectedBody != null)
		{
			Debug.DrawLine(
				spring.transform.TransformPoint(spring.anchor),
				spring.connectedAnchor,
				Color.green
			);
		}
    }

	void CastRayFromCameraCenter()
	{
		Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;

		// Modified Raycast to ignore triggers
		if (Physics.Raycast(ray, out hit, 10, ~0, QueryTriggerInteraction.Ignore))
		{
			var target = hit.transform.gameObject.GetComponent<Rigidbody>();

			if (target != null)
			{
				spring ??= gameObject.AddComponent<SpringJoint>();
				spring.autoConfigureConnectedAnchor = false;
				spring.spring = 300;
				spring.connectedMassScale = 3;
				spring.massScale = 3;
				spring.maxDistance = .05f;
				spring.minDistance = .01f;
				spring.anchor = new(0,1f,0);
				spring.damper = 50;
				Debug.DrawLine(ray.origin, hit.point, Color.red, 2f);
				spring.connectedBody = target;
			}
			//else  
			//	spring.connectedAnchor = hit.point;
		}
		else
		{
			Debug.Log("No object hit");
		}
		//rb.constraints = RigidbodyConstraints.None;

    }
	void Releasespring()
	{
		Destroy(spring);
		spring = null;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
	}
    // Optional: Draw Gizmos in the Scene view for better visualization
	private void OnDrawGizmos()
	{
		if (spring != null && spring.connectedBody != null)
		{
			Gizmos.color = Color.cyan;
			Vector3 anchorPos = spring.transform.TransformPoint(spring.anchor);
			Gizmos.DrawLine(anchorPos, spring.connectedAnchor);
			Gizmos.DrawSphere(anchorPos, 0.05f);
			Gizmos.DrawSphere(spring.connectedAnchor, 0.05f);
		}
	}
}