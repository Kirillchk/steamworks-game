using UnityEngine;
using P2PMessages;

public class NetworkTransform : MonoBehaviour
{
	static int AutoID = 0;
	[SerializeField] int ID; 
	Vector3 lastPosition;
	Quaternion lastRotation;
	bool sendTransform = false;
    void Awake()
	{
		P2PBase.networkTransforms.Add(this);
		ID = AutoID++;
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
		
		ITransformMessage transformMessage = null;

		if (moved && rotated)
			transformMessage = new P2PTransformPositionAndRotation(currentPosition, currentRotation, ID);
		else if (moved && !rotated)
			transformMessage = new P2PTransformPosition(currentPosition, ID);
		else if (!moved && rotated)
			transformMessage = new P2PTransformRotation(currentRotation, ID);

		P2PBase.transformMessages.Add(transformMessage);	
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
