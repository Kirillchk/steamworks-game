using System;
using UnityEngine;

public class HandsBehaviour : MonoBehaviour
{
	public GameObject HandPref;
	Hand leftHand = new(), rightHand = new();
	Rigidbody rb;
	void Start()
	{
		leftHand.HandOffset = new(-.5f, 0, 1);
		rightHand.HandOffset = new(.5f, 0, 1);
		rb = GetComponent<Rigidbody>();
		Hand.HandPref = HandPref;
	}
	void Update()
	{
		bool res = Physics.Raycast(
			Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)),
			out RaycastHit hit,
			10, ~0, QueryTriggerInteraction.Ignore
		);
		var target = hit.transform.gameObject;
		if (!res)
			return;
		if (Input.GetMouseButtonDown(0))
			leftHand.Grab(target, hit.point, this);
		if (!Input.GetMouseButton(0))
			leftHand.Relese();
		if (Input.GetMouseButtonDown(1))
			rightHand.Grab(target, hit.point, this);
		if (!Input.GetMouseButton(1))
			rightHand.Relese();
		if (Input.GetKeyDown(KeyCode.Q))
			leftHand.PickUp(target, gameObject);
		if (Input.GetKeyDown(KeyCode.E))
			rightHand.PickUp(target, gameObject);
	}
	public SpringJoint newSpring()
	{
		SpringJoint spring = gameObject.AddComponent<SpringJoint>();
		spring.autoConfigureConnectedAnchor = false;
		spring.spring = 300;
		spring.connectedMassScale = 3;
		spring.massScale = 3;
		spring.maxDistance = .05f;
		spring.minDistance = .01f;
		spring.anchor = new(0, 1f, 0);
		spring.damper = 50;

		return spring;
	}
	public class Hand
	{
		public static GameObject HandPref;
		public GameObject HandTexture = null;
		public SpringJoint HandJoint = null;
		public Vector3 HandOffset;
		public void Grab(GameObject target, Vector3 point, HandsBehaviour handBeh)
		{
			if (target.tag != "Grab")
				return;
			HandJoint = handBeh.newSpring();
			if (target.GetComponent<Rigidbody>() != null)
			{
				HandJoint.connectedBody = target.GetComponent<Rigidbody>();
				HandTexture = Instantiate(HandPref, point, new(), target.transform);
			}
			else
			{
				HandJoint.connectedAnchor = point;
				HandTexture = Instantiate(HandPref, point, new());
			}
		}
		public void Relese()
		{
			Destroy(HandTexture);
			Destroy(HandJoint);
			HandJoint = null;
		}
		public void PickUp(GameObject target, GameObject gameObject)
		{
			if (target.tag != "Pickup")
				return;
				
			target.transform.parent = gameObject.transform;
			target.transform.rotation = new();
			target.transform.localPosition = HandOffset;
			var targetRB = target.GetComponent<Rigidbody>();
			targetRB.useGravity = false;
			targetRB.isKinematic = true;
			var colliders = target.GetComponents<Collider>();
			foreach (var collider in colliders)
				collider.enabled = false;
		}
	}
}