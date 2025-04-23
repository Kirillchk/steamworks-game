using UnityEngine;
using P2PMessages;

public class NetworkHostTransform : NetworkTransform
{	
	const int Flags = (int)(k_nSteamNetworkingSend.NoDelay | k_nSteamNetworkingSend.NoNagle);
	Vector3 lastPosition;
	Quaternion lastRotation;
    void Update()
    {
		Vector3 currentPosition = transform.position;
		Quaternion currentRotation = transform.rotation;

		bool moved = lastPosition != currentPosition;
		bool rotated = lastRotation != currentRotation;

		lastPosition = currentPosition;
		lastRotation = currentRotation;

		if (!(moved || rotated) || sync) return;
		
		ITransformMessage transformMessage = null;

		if (moved && rotated)
			transformMessage = new P2PTransformPositionAndRotation(currentPosition, currentRotation, ID);
		else if (moved && !rotated)
			transformMessage = new P2PTransformPosition(currentPosition, ID);
		else if (!moved && rotated)
			transformMessage = new P2PTransformRotation(currentRotation, ID);

		manager.SendMessageToConnection(transformMessage.GetBinaryRepresentation(), Flags);
    }
}
