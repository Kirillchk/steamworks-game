using System;
using TMPro;
using UnityEngine;
using P2PMessages;

public class NetworkTransform : MonoBehaviour
{	
	static int AutoID = 0;
	[SerializeField] int ID; 
	Vector3 lastPosition;
	Quaternion lastRotation;
    public P2PBase manager;
	bool sync = false;
    void Awake() {
		ID = AutoID++;
		manager = GameObject.FindWithTag("MainManager").GetComponent<P2PBase>();
		manager.cubes.Add(this);
	}
    void Update()
    {
		bool moved = lastRotation != transform.rotation || lastPosition != transform.position;
		lastRotation = transform.rotation;
		lastPosition = transform.position;
		if (!sync || !moved)
			return;
		P2PTransformMessage transformMessage = new(transform.position, transform.rotation, ID);
        manager.SendMessageToConnection(transformMessage.GetBinaryRepresentation(), (int)(k_nSteamNetworkingSend.NoDelay | k_nSteamNetworkingSend.NoNagle));
    }
    [ContextMenu("Send")]
	void toggle() => sync = !sync; 
}
