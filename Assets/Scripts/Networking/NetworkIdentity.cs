using UnityEngine;

public class NetworkIdentity : MonoBehaviour
{
	internal Vector3 uniqueVector;
	void Start() =>
		uniqueVector = transform.position;
}
