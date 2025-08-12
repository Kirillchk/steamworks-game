using System;
using UnityEngine;

public class HandsBehaviour : MonoBehaviour
{
	public GameObject HandPref, PlayerRef, DragPointRef;
	protected GameObject holding = null, HandTexture = null;
	public Quaternion HandRotation;
	public KeyCode PickButton;
	public int DragButton;
	SpringJoint HandJoint = null;
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
			HandJoint.connectedBody = target.GetComponent<Rigidbody>();
		}
		else
		{
			HandTexture = Instantiate(HandPref, point, HandRotation);
			HandJoint.connectedAnchor = point;
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
		target.transform.localPosition = new(0, 0, .5f);

		var item = target.GetComponent<ItemBehaviour>();
		item.TakeOwnership();
		item.Sync(item.PickUp);
	}
	public void Drop()
	{
		holding.transform.parent = null;
		var item = holding.GetComponent<ItemBehaviour>();
		item.Sync(item.Drop);
		holding = null;
	}
}
