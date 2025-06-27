using UnityEngine;

public class NetworkIdentity : MonoBehaviour
{
	internal Vector3 uniqueVector;
	internal bool isOwner;
	void Start() =>
		uniqueVector = transform.position;
}
