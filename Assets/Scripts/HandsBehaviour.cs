using UnityEngine;

public class HandsBehaviour : MonoBehaviour
{
	public GameObject HandPref;
	Hand leftHand = new(), rightHand = new();
	Rigidbody rb;
	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
			CastRayFromCameraCenter(leftHand);
		if (!Input.GetMouseButton(0))
			Releasespring(leftHand);
		if (Input.GetMouseButtonDown(1))
			CastRayFromCameraCenter(rightHand);
		if (!Input.GetMouseButton(1))
			Releasespring(rightHand);

	}

	void CastRayFromCameraCenter(Hand hand)
	{
		Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;
		if (!Physics.Raycast(ray, out hit, 10, ~0, QueryTriggerInteraction.Ignore))
			return;

		if (hit.transform.gameObject.tag != "Grab" && hit.transform.gameObject.tag != "Pickup")
			return;

		var target = hit.transform.gameObject;

		hand.HandJoint ??= gameObject.AddComponent<SpringJoint>();
		hand.HandJoint.autoConfigureConnectedAnchor = false;
		hand.HandJoint.spring = 300;
		hand.HandJoint.connectedMassScale = 3;
		hand.HandJoint.massScale = 3;
		hand.HandJoint.maxDistance = .05f;
		hand.HandJoint.minDistance = .01f;
		hand.HandJoint.anchor = new(0, 1f, 0);
		hand.HandJoint.damper = 50;
		Debug.DrawLine(ray.origin, hit.point, Color.red, 2f);
		if (target.GetComponent<Rigidbody>() != null)
		{
			hand.HandJoint.connectedBody = target.GetComponent<Rigidbody>();
			hand.HandTexture = Instantiate(HandPref, hit.point, new(), target.transform);
		}
		else
		{
			hand.HandJoint.connectedAnchor = hit.point;
			hand.HandTexture = Instantiate(HandPref, hit.point, new());
		}
	}
	void Releasespring(Hand hand)
	{
		Destroy(hand.HandTexture);
		Destroy(hand.HandJoint);
		hand.HandJoint = null;
	}
}
public class Hand
{
	public GameObject HandTexture = null;
	public SpringJoint HandJoint = null;
	public bool isEmpty = true;
}