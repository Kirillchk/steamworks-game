using UnityEngine;
using System.Threading.Tasks;
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
		networkIdentity = GetComponent<NetworkIdentity>(); 
		//TODO: FIX! This should not be necessary
		await Task.Yield();
		ID = networkIdentity.uniqueVector;
		P2PBase.networkTransforms[ID] = this;
	}
	void LateUpdate() =>
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

		if (doSendTransform && (moved || rotated || scaled))
			P2PBase.TransformPacks.Add(
				new() {
					ID = ID,
					newPos = moved ? currentPosition : null,
					newRot = rotated ? currentRotation : null,
					newScl = scaled ? currentScale : null
				}
			);
		doSendTransform = true;
	}
	internal void TransformSync(in P2PBase.TransformPack tp)
	{
		if (networkIdentity.isOwner)
			return;
		if (tp.newPos != null)
			transform.position = Vector3.Lerp(transform.position, tp.newPos.Value, 0.5f);
		if (tp.newRot != null)
			transform.rotation = tp.newRot.Value;
		if (tp.newScl != null)
			transform.localScale = tp.newScl.Value;
		doSendTransform = false;
	}
}
