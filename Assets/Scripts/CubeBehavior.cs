using System;
using TMPro;
using UnityEngine;

public class CubeBehavior : MonoBehaviour
{	
	static long AutoID = 0;
	[SerializeField] long ID; 
	Vector3 lastPosition;
	Quaternion lastRotation;
    public P2PBase manager;
	bool sync = false;
    void Start() {
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
		TransformMessage transformMessage = new(transform.position, transform.rotation);
        manager.SendMessageToConnection(transformMessage.GetBinaryRepresentation(), 0 | 2);
    }
    [ContextMenu("Send")]
	void toggle() => sync = !sync; 
}
