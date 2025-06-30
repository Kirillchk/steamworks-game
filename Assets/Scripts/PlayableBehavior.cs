using System.Collections.Generic;
using UnityEngine;

public class PlayableBehavior : MonoBehaviour
{
	[SerializeField]
	public static List<PlayableBehavior> Players = new(4);
	public GameObject CameraPrefab;
	[ContextMenu("possess")]
	public void Possess()
	{
		gameObject.AddComponent<AudioListener>();
		var rigid = gameObject.AddComponent<Rigidbody>();
		rigid.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
		var movement = gameObject.AddComponent<PlayerMovement>();
		GameObject camerainstance = Instantiate(CameraPrefab, gameObject.transform);
		movement.playerCamera = camerainstance.transform;
	}
	void Start() => Players.Add(this);
}
