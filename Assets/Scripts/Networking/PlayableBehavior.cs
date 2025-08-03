using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayableBehavior : MonoBehaviour
{
	public static List<PlayableBehavior> Players = new(4);
	public GameObject ControllablePref, UncontrollablePref;
	void Start()
	{
		Players.Add(this);
		//Debug.Log(gameObject.transform.position);
	}
	public void Possess()
	{
		var p = Instantiate(ControllablePref, gameObject.transform, new());
		p.GetComponent<NetworkIdentity>().isOwner = true;
		Debug.Log("possessed " + p.transform.position);
	}
	public void SummonPlayer()
	{
		var p = Instantiate(UncontrollablePref, gameObject.transform, new());
		p.GetComponent<NetworkIdentity>().isOwner = false;
		Debug.Log("summoned " + p.transform.position);
	}
	public static void AddPLayers(int playersOnline)
	{
		playersOnline--;
		Players[playersOnline].Possess();
		for (int i = playersOnline - 1; i >= 0; i--)
			Players[i].SummonPlayer();
	}
}
