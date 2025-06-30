using UnityEngine;

public class NetworkIdentity : MonoBehaviour
{
	internal Vector3 uniqueVector;
	internal bool isOwner = false;
	void Start() =>
		uniqueVector = transform.position;
}
