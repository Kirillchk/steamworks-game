using UnityEngine;
public class NetworkIdentity : MonoBehaviour
{
	internal Vector3 uniqueVector;
	public bool autoset = true;
	public bool isOwner = false;
	void Start()
	{
		uniqueVector = transform.position;
		if (autoset)
			isOwner = P2PBase.isHost;
	}
}
