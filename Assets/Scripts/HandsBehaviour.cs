using System;
using UnityEngine;

public class HandsBehaviour : MonoBehaviour
{
	public GameObject HandPref;
	protected GameObject holding = null, HandTexture = null;
	protected Quaternion HandRotation;
	static Rigidbody rb;
	SpringJoint HandJoint = null;
	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}
	public void Grab(GameObject target, Vector3 point)
	{
		if (target.tag != "Grab" && target.tag != "Pickup")
			return;
		HandJoint = gameObject.AddComponent<SpringJoint>();
		HandJoint.autoConfigureConnectedAnchor = false;
		HandJoint.spring = 10;
		HandJoint.connectedMassScale = 3;
		HandJoint.massScale = 3;
		HandJoint.maxDistance = .05f;
		HandJoint.minDistance = 0;
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
		Destroy(HandTexture);
		Destroy(HandJoint);
		HandJoint = null;
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