using UnityEngine;
using P2PMessages;

public class NetworkClientTransform : NetworkTransform
{	
	const int Flags = (int)(k_nSteamNetworkingSend.NoDelay | k_nSteamNetworkingSend.NoNagle);
	Vector3 lastPosition;
    void Update()
    {
		Vector3 currentPosition = transform.position;

		bool moved = lastPosition != currentPosition;

		Vector3 positionShift = currentPosition - lastPosition;
		lastPosition = currentPosition;

		if (!moved || sync) return;
		
		P2PTransformPosition transformMessage = new (positionShift, ID);
		manager.SendMessageToConnection(transformMessage.GetBinaryRepresentation(), Flags);
    }
}
