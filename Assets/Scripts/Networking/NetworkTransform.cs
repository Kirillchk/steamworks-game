using UnityEngine;
using P2PMessages;
using System.Threading.Tasks;
using MessagePack;
using System.Collections.Generic;
public class NetworkTransform : MonoBehaviour
{
	[SerializeField] Vector3 ID;
	Vector3 lastScale;
	Vector3 lastPosition;
	Quaternion lastRotation;
	NetworkIdentity networkIdentity;
	public bool doSendTransform = true;
	async void Start()
	{
		//TODO: FIX! This should not be necessary
		networkIdentity = GetComponent<NetworkIdentity>(); 
		await Task.Yield();
		ID = networkIdentity.uniqueVector;
		Debug.Log($"UNIQUE {ID}");
		P2PBase.networkTransforms[ID] = this;
	}
	void Update() =>
		sendTransform();
	void sendTransform()
	{
		if (!networkIdentity.isOwner)
			return;
		Vector3 currentPosition = transform.position;
		Quaternion currentRotation = transform.rotation;
		Vector3 currentScale = lastScale;

		bool moved = lastPosition != currentPosition;
		bool rotated = lastRotation != currentRotation;
		bool scaled = lastScale != currentScale;

		lastPosition = currentPosition;
		lastRotation = currentRotation;
		lastScale = currentScale;
		if (doSendTransform)
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
			var pack = new TransformPack()
			{
				ID = ID,
				newPos = moved ? currentPosition : null,
				newRot = rotated ? currentRotation : null,
				newScl = scaled ? currentScale : null
			};
			var list = new List<TransformPack>(){ pack };
			byte[] bytes = MessagePackSerializer.Serialize(list);
			var packs = MessagePackSerializer.Deserialize<List<TransformPack>>(bytes);
			Debug.Log($"Original pack-(id:{pack.ID} pos:{pack.newPos} rot:{pack.newRot} scl:{pack.newScl}) bytes: {bytes}");
			foreach (var p in packs)
				Debug.Log($"recived pack-(id:{p.ID} pos:{p.newPos} rot:{p.newRot} scl:{p.newScl})");	
		}
		doSendTransform = true;
	}
	internal void MoveToSync(Vector3 move)
	{
		if (networkIdentity.isOwner)
			return;
		transform.position = Vector3.Lerp(transform.position, move, 0.5f);
		doSendTransform = false;
	}
	internal void RotateToSync(Quaternion rotate)
	{
		if (networkIdentity.isOwner)
			return;
		transform.rotation = rotate;
		doSendTransform = false;
	}
	internal void ScaleToSync(Vector3 scale)
	{
		if (networkIdentity.isOwner)
			return;
		transform.localScale = scale;
		doSendTransform = false;
	}
}
