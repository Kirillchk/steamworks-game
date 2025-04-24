using UnityEngine;
using P2PMessages;

public class NetworkTransform : MonoBehaviour
{	
	const int Flags = (int)k_nSteamNetworkingSend.ReliableNoNagle;

	static int AutoID = 0;
	[SerializeField] int ID; 
	
	Vector3 lastPosition;
	Quaternion lastRotation;
    P2PBase manager;
	bool sync = false;
    void Awake() 
	{
		ID = AutoID++;
		manager = GameObject.FindWithTag("MainManager").GetComponent<P2PBase>();
		manager.cubes.Add(this);
	}
    void Update()
    {
		Vector3 currentPosition = transform.position;
		Quaternion currentRotation = transform.rotation;

		bool moved = lastPosition != currentPosition;
		bool rotated = lastRotation != currentRotation;

		lastPosition = currentPosition;
		lastRotation = currentRotation;

		if (!(moved || rotated) || !sync) 
		{
			sync = true;
			return;
		}
		
		ITransformMessage transformMessage = null;

		if (moved && rotated)
			transformMessage = new P2PTransformPositionAndRotation(currentPosition, currentRotation, ID);
		else if (moved && !rotated)
			transformMessage = new P2PTransformPosition(currentPosition, ID);
		else if (!moved && rotated)
			transformMessage = new P2PTransformRotation(currentRotation, ID);

		manager.SendMessageToConnection(transformMessage.GetBinaryRepresentation(), Flags);
    }
	internal void MoveToSync(Quaternion? rotate = null, Vector3? move = null)
	{
		if (rotate != null)
			transform.rotation = (Quaternion)rotate;
		if (move != null)
			transform.position = (Vector3)move;
		sync = false;
	}
	
}
