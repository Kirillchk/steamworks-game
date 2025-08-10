using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class VentLinker : MonoBehaviour
{
	public static List<GameObject> Vents = new();
	void Start()
	{
		Vents.Add(gameObject);
	}
	public static void LinkAllVents()
	{
		for (int i = 0; i <= Vents.Count; i++)
		{
			for (int n = i+1; n < Vents.Count; n++)
			{
				var from = Vents[i].transform;
				var to = Vents[n].transform;
				var link = Vents[i].AddComponent<NavMeshLink>();
				link.area = NavMesh.GetAreaFromName("Vent");
				link.costModifier = 0;
				link.startTransform = from;
				link.endTransform = to;
			}
		}
	}
}
