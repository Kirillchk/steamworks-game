using UnityEngine;
using P2PMessages;
using System.Threading.Tasks;
public class NetworkTransform : MonoBehaviour
{
	[SerializeField] Vector3 ID;
	Vector3 lastPosition;
	Quaternion lastRotation;
	bool sendTransform = false;
	async void Awake()
	{
		//TODO: FIX! This should not be necessary
		await Task.Yield();
		ID = GetComponent<NetworkIdentity>().uniqueVector;
		Debug.Log($"UNIQUE {ID}");
		P2PBase.networkTransforms[ID] = this;
	}
	void Update()
	{
		Vector3 currentPosition = transform.position;
		Quaternion currentRotation = transform.rotation;

		bool moved = lastPosition != currentPosition;
		bool rotated = lastRotation != currentRotation;

		lastPosition = currentPosition;
		lastRotation = currentRotation;

		if (sendTransform)
		{
			if (moved)
				P2PBase.TransformBulk.AddRange(INetworkMessage.StructToSpan(
					new TransformPos()
					{
						purpuse = TransformPos.Purpuse,
						ID = ID,
						pos = currentPosition,
					}
				).ToArray());
			if (rotated)
				P2PBase.TransformBulk.AddRange(INetworkMessage.StructToSpan(
					new TransformRot()
					{
						purpuse = TransformRot.Purpuse,
						ID = ID,
						rot = currentRotation
					}
				).ToArray());
		}
		sendTransform = true;
	}
	internal void MoveToSync(Quaternion? rotate = null, Vector3? move = null)
	{
		if (rotate != null)
			transform.rotation = (Quaternion)rotate;
		if (move != null)
			transform.position = (Vector3)move;
		sendTransform = false;
	}
}
