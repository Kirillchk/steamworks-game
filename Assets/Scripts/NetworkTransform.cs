using UnityEngine;
using P2PMessages;

public class NetworkTransform : MonoBehaviour
{
	[SerializeField] Vector3 ID; 
	Vector3 lastPosition;
	Quaternion lastRotation;
	bool sendTransform = false;
    void Awake()
	{
		ID = GetComponent<NetworkIdentity>().uniqueVector;
		P2PBase.networkTransforms[ID] = this;
	}
	void Update()
    {
		Vector3 currentPosition = transform.position;
		Quaternion currentRotation = transform.rotation;

		bool moved = lastPosition != currentPosition;
		bool rotated = lastRotation != currentRotation;

		lastPosition = currentPosition;
		lastRotation = currentRotation;

		if (!(moved || rotated) || !sendTransform) 
		{
			sendTransform = true;
			return;
		}
		P2PBase.transformMessages.Add(
			new TransformMessage(ID, 
			moved ? currentPosition : null, 
			rotated ? currentRotation : null)
		);	
    }
	internal void MoveToSync(Quaternion? rotate = null, Vector3? move = null)
	{
		if (rotate != null)
			transform.rotation = (Quaternion)rotate;
		if (move != null)
			transform.position = (Vector3)move;
		sendTransform = false;
	}
}
