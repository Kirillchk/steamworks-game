using UnityEngine;
using P2PMessages;

public class NetworkTransform : MonoBehaviour
{	
	const int Flags = (int)(k_nSteamNetworkingSend.NoDelay | k_nSteamNetworkingSend.NoNagle);
	static int AutoID = 0;
	[SerializeField] bool sync = true;
	[SerializeField] int ID; 
	Vector3 lastPosition;
	Quaternion lastRotation;
    public P2PBase manager;
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

		ITransformMessage transformMessage = null;

		if (!(moved || rotated) || sync) return;

		if (moved && rotated)
			transformMessage = new P2PTransformPositionAndRotation(currentPosition, currentRotation, ID);
		else if (moved && !rotated)
			transformMessage = new P2PTransformPosition(currentPosition, ID);
		else if (!moved && rotated)
			transformMessage = new P2PTransformRotation(currentRotation, ID);

		manager.SendMessageToConnection(transformMessage.GetBinaryRepresentation(), Flags);
    }
	[ContextMenu("SYNC")]
	void Toggle() => sync = !sync;
}
