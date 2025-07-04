using UnityEngine;

public class NetworkIdentity : MonoBehaviour
{
	internal Vector3 uniqueVector;
	public bool isOwner = true;
	void Start() =>
		uniqueVector = transform.position;
}
