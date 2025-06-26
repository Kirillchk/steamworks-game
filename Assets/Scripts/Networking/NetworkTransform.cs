using UnityEngine;
using P2PMessages;
using System.Threading.Tasks;
public class NetworkTransform : MonoBehaviour
{
	[SerializeField] Vector3 ID;
	Vector3 lastScale;
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
		Vector3 currentScale = lastScale;

		bool moved = lastPosition != currentPosition;
		bool rotated = lastRotation != currentRotation;
		bool scaled = lastScale != currentScale;

		lastPosition = currentPosition;
		lastRotation = currentRotation;
		lastScale = currentScale;

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
			if (scaled)
				P2PBase.TransformBulk.AddRange(INetworkMessage.StructToSpan(
					new TransformScl()
					{
						purpuse = TransformScl.Purpuse,
						ID = ID,
						scl = currentScale
					}
				).ToArray());
		}
		sendTransform = true;
	}
	internal void MoveToSync(Vector3 move)
	{
		transform.position = move;
		sendTransform = false;
	}
	internal void RotateToSync(Quaternion rotate)
	{
		transform.rotation = rotate;
		sendTransform = false;
	}
	internal void ScaleToSync(Vector3 scale)
	{
		transform.localScale = scale;
		sendTransform = false;
	}
}
