using UnityEngine;

public class NetworkIdentity : MonoBehaviour
{
	internal Vector3 uniqueVector;
	public bool isOwner = false; //wha
	void Start()
	{
		uniqueVector = transform.position;
		isOwner = P2PBase.isHost;
	}
}
