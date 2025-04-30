using UnityEngine;

public class NetworkIdentity : MonoBehaviour
{
    
	static int AutoID = 0;
	[SerializeField] int ID; 
	Vector3 lastPosition;
	Quaternion lastRotation;
	bool sendTransform = false;
    void Awake()
	{
		ID = AutoID++;
	}
}
