using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkActions : MonoBehaviour
{
    Vector3 ID;
	protected List<Action> actions = new();
    void Awake()
	{
		ID = GetComponent<NetworkIdentity>().uniqueVector;
		P2PBase.networkActionScripts[ID] = this;
	}
	
	void RndmFunc(){

	}
}
