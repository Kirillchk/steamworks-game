using UnityEngine;
public class NetworkIdentity : MonoBehaviour
{
	internal Vector3 uniqueVector;
	public bool isOwner = false;
	void Start() {
		uniqueVector = transform.position;
	}
}
