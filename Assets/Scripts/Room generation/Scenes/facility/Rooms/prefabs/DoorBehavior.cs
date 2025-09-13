using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
	void Start()
	{
		var joint = gameObject.AddComponent<HingeJoint>();
		joint.anchor = new (0,0.5f,-1);
		joint.axis = Vector3.up;
    }

}
