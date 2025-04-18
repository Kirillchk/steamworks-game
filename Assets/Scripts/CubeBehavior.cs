using System;
using UnityEngine;

public class CubeBehavior : MonoBehaviour
{
    public P2PBase manager;
    void Start()
    {
        manager = GameObject.FindWithTag("MainManager").GetComponent<P2PBase>();
    }
    void Update()
    {
		byte[] data = new byte[29];
		data[0] = 0;
		
		Vector3 posit = transform.position;
		Quaternion quatern = transform.rotation;
		float[] farr = { 
			posit.x, posit.y, posit.z,
			quatern.x, quatern.y, quatern.z, quatern.w
		};
        for (int i = 0; i < 7; i++)
		    Array.Copy(BitConverter.GetBytes(farr[i]), 0, data, i * 4 + 1, 4);
        manager.SendMessageToConnection(data, 0 | 2);
    }
}
