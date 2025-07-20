using UnityEngine;

public class HandsBehaviour : MonoBehaviour
{
    SpringJoint springJoint;
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
            ReleaseSpringJoint();
        }
		if (Input.GetMouseButton(1))
		{
			Quaternion targetRotation = Quaternion.Euler(0, rb.rotation.eulerAngles.y, 0);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 0.1f);
		}

        // Continuously draw the spring connection in Play mode
		if (springJoint != null && springJoint.connectedBody != null)
		{
			Debug.DrawLine(
				springJoint.transform.TransformPoint(springJoint.anchor),
				springJoint.connectedAnchor,
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
				springJoint ??= gameObject.AddComponent<SpringJoint>();
				springJoint.autoConfigureConnectedAnchor = false;
				springJoint.spring = 200;
				springJoint.connectedMassScale = 3;
				springJoint.massScale = 3;
				springJoint.maxDistance = .05f;
				springJoint.minDistance = .01f;
				springJoint.anchor = new(0,1f,0);
				springJoint.damper = 50;
				Debug.DrawLine(ray.origin, hit.point, Color.red, 2f);
				springJoint.connectedBody = target;
			}
			//else  
			//	springJoint.connectedAnchor = hit.point;
		}
		else
		{
			Debug.Log("No object hit");
		}
		//rb.constraints = RigidbodyConstraints.None;

    }
	void ReleaseSpringJoint()
	{
		Destroy(springJoint);
		springJoint = null;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
	}
    // Optional: Draw Gizmos in the Scene view for better visualization
	private void OnDrawGizmos()
	{
		if (springJoint != null && springJoint.connectedBody != null)
		{
			Gizmos.color = Color.cyan;
			Vector3 anchorPos = springJoint.transform.TransformPoint(springJoint.anchor);
			Gizmos.DrawLine(anchorPos, springJoint.connectedAnchor);
			Gizmos.DrawSphere(anchorPos, 0.05f);
			Gizmos.DrawSphere(springJoint.connectedAnchor, 0.05f);
		}
	}
}