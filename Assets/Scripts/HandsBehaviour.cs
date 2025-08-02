using System;
using UnityEngine;

public class HandsBehaviour : MonoBehaviour
{
	public GameObject HandPref, PlayerRef, DragPointRef;
	protected GameObject holding = null, HandTexture = null;
	protected Quaternion HandRotation;
	protected KeyCode PickButton;
	protected int DragButton;
	static Rigidbody rb;
	SpringJoint HandJoint = null;
	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}
    void Update()
    {
		if (Input.GetMouseButtonDown(DragButton) && holding != null)
			holding.GetComponent<ItemBehaviour>()?.UseItem.Invoke();
		if (Input.GetMouseButtonUp(DragButton))
			Relese();
		if (Input.GetKeyDown(PickButton) && holding != null)
			Drop();
		// TODO: turn into a helper or smth
			bool res = Physics.Raycast(
			Camera.main.ViewportPointToRay(new (0.5f, 0.5f, 0)),
			out RaycastHit hit,
			10, ~0, QueryTriggerInteraction.Ignore
		);
		if (!res)
			return;
		var target = hit.transform.gameObject;
		if (Input.GetMouseButtonDown(DragButton) && holding == null)
		{
			if (target.tag == "Pickup")
				Drag(target, hit.point);
			else if (target.tag == "Grab")
				Grapple(target, hit.point);
		}
		if (Input.GetKeyDown(PickButton) && holding == null)
			PickUp(target);
    }
	public void Drag(GameObject target, Vector3 point)
	{
		if (target.tag != "Pickup")
			return;
		HandJoint = DragPointRef.AddComponent<SpringJoint>();
		HandJoint.autoConfigureConnectedAnchor = false;
		HandJoint.spring = 300;
		HandJoint.connectedMassScale = 3;
		HandJoint.massScale = 3;
		HandJoint.maxDistance = .1f;
		HandJoint.minDistance = 0;
		HandJoint.anchor = new(0, 0, .5f);
		HandJoint.damper = 300;
		if (target.GetComponent<Rigidbody>() != null)
		{
			HandTexture = Instantiate(HandPref, point, HandRotation, target.transform);
			HandJoint.connectedBody = HandTexture.GetComponent<Rigidbody>();
			HandTexture.GetComponent<FixedJoint>().connectedBody = target.GetComponent<Rigidbody>();
		}
	}
	public void Grapple(GameObject target, Vector3 point)
	{
		if (target.tag != "Grab")
			return;
		HandJoint = PlayerRef.AddComponent<SpringJoint>();
		HandJoint.autoConfigureConnectedAnchor = false;
		HandJoint.spring = 310;
		HandJoint.connectedMassScale = 3;
		HandJoint.massScale = 3;
		HandJoint.maxDistance = .1f;
		HandJoint.minDistance = .05f;
		HandJoint.anchor = new(0, 0, .5f);
		HandJoint.damper = 50;
		if (target.GetComponent<Rigidbody>() != null)
		{
			HandJoint.connectedBody = target.GetComponent<Rigidbody>();
			HandTexture = Instantiate(HandPref, point, HandRotation, target.transform);
		}
		else
		{
			HandJoint.connectedAnchor = point;
			HandTexture = Instantiate(HandPref, point, HandRotation);
		}
	}
	public void Relese()
	{
		Debug.Log("RELEASING");
		Destroy(HandTexture);
		Destroy(HandJoint);
		HandJoint = null;
		Debug.Log("RELEASED");
	}
	public void PickUp(GameObject target)
	{
		if (target.tag != "Pickup")
			return;
		holding = target;
		target.transform.parent = gameObject.transform;
		target.transform.rotation = new();
		target.transform.localPosition = new(0,0,.5f);
		var targetRB = target.GetComponent<Rigidbody>();
		targetRB.useGravity = false;
		targetRB.isKinematic = true;
		var colliders = target.GetComponents<Collider>();
		foreach (var collider in colliders)
			collider.enabled = false;
	}
	public void Drop()
	{
		holding.transform.parent = null;
		var RB = holding.GetComponent<Rigidbody>();
		RB.useGravity = true;
		RB.isKinematic = false;
		var colliders = holding.GetComponents<Collider>();
		foreach (var collider in colliders)
			collider.enabled = true;
		holding = null;
	}
}