using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayableBehavior : MonoBehaviour
{
	public static List<PlayableBehavior> Containers = new(4);
	public static List<GameObject> Players = new(4);
	public GameObject ControllablePref, UncontrollablePref;
	void Start()
	{
		Containers.Add(this);
	}
	// TODO: verify if it works fine with players joining/leaving in random order
	public void Possess()
	{
		var p = Instantiate(ControllablePref, gameObject.transform, new());
		Players.Add(p);
		p.GetComponent<NetworkIdentity>().isOwner = true;
		Debug.Log("possessed " + p.transform.position);
	}
	public void SummonPlayer()
	{
		var p = Instantiate(UncontrollablePref, gameObject.transform, new());
		Players.Add(p);
		p.GetComponent<NetworkIdentity>().isOwner = false;
		Debug.Log("summoned " + p.transform.position);
	}
	public static void AddPLayers(int playersOnline)
	{
		playersOnline--;
		Containers[playersOnline].Possess();
		for (int i = playersOnline - 1; i >= 0; i--)
			Containers[i].SummonPlayer();
	}
}
