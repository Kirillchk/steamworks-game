using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayableBehavior : MonoBehaviour
{
	public static List<PlayableBehavior> Players = new(4);
	public GameObject ControllablePref, UncontrollablePref;
	void Start() => Players.Add(this);
	[ContextMenu("Possess")]
	public void Possess()
	{
		Instantiate(ControllablePref);
	}
	[ContextMenu("SummonPlayer")]
	public void SummonPlayer()
	{
		Instantiate(UncontrollablePref);
	}
}
