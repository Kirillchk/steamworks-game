using System.Collections.Generic;
using UnityEngine;

public class RoamMark : MonoBehaviour
{
	public static List<RoamMark> Marks = new();
	public string section = "";
	void Start() =>
		Marks.Add(this);
	public static Vector3 GetFarthest(Vector3 from)
	{
		Vector3 farest = new();
		float MaxDist = 0;
		foreach (var mark in Marks)
		{
			float dist = Vector3.Distance(from, mark.transform.position);
			if (dist > MaxDist)
			{
				MaxDist = dist;
				farest = mark.transform.position;
			}
		}
		return farest;
	}
}
