using UnityEngine;

public class HandsBehaviour : MonoBehaviour
{
    public Camera playerCamera;
    SpringJoint leftHand;
    SpringJoint rightHand;
	Rigidbody rb;
	void Start() => rb = GetComponent<Rigidbody>();
	void Update()
    {
        if (Input.GetMouseButtonDown(0))
            CastRayFromCameraCenter(ref leftHand);
		if(!Input.GetMouseButton(0))
            Releasespring(ref leftHand);
        if (Input.GetMouseButtonDown(1))
            CastRayFromCameraCenter(ref rightHand);
		if(!Input.GetMouseButton(1))
            Releasespring(ref rightHand);
        
    }

	void CastRayFromCameraCenter(ref SpringJoint spring)
	{
		Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 10, ~0, QueryTriggerInteraction.Ignore))
		{
			var target = hit.transform.gameObject.GetComponent<Rigidbody>();

			if (target != null)
			{
				if (target.gameObject.tag != "Grab" && target.gameObject.tag != "Pickup")
					return;
				spring ??= gameObject.AddComponent<SpringJoint>();
				spring.autoConfigureConnectedAnchor = false;
				spring.spring = 300;
				spring.connectedMassScale = 3;
				spring.massScale = 3;
				spring.maxDistance = .05f;
				spring.minDistance = .01f;
				spring.anchor = new(0, 1f, 0);
				spring.damper = 50;
				Debug.DrawLine(ray.origin, hit.point, Color.red, 2f);
				spring.connectedBody = target;
			}
		}
    }
	void Releasespring(ref SpringJoint spring)
	{
		Destroy(spring);
		spring = null;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
	}
}