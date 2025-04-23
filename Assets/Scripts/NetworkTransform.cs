using UnityEngine;
using P2PMessages;

public class NetworkTransform : MonoBehaviour
{	
	static int AutoID = 0;
	[SerializeField] internal bool sync = true;
	[SerializeField] internal int ID;
    internal P2PBase manager;
    void Awake() 
	{
		ID = AutoID++;
		manager = GameObject.FindWithTag("MainManager").GetComponent<P2PBase>();
		manager.cubes.Add(this);
	}
	[ContextMenu("SYNC")]
	void Toggle() => sync = !sync;
}
