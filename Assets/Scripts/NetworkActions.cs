using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using P2PMessages;
public class NetworkActions : MonoBehaviour
{
    Vector3 ID;
	protected List<Action> actions = new();
    void Awake()
	{
		ID = GetComponent<NetworkIdentity>().uniqueVector;
		P2PBase.networkActionScripts[ID] = this;
		
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(m => m.GetCustomAttributes(typeof(CanTriggerSync), false).Length > 0);
        foreach (var method in methods)
			actions.Add((Action)Delegate.CreateDelegate(typeof(Action), this, method));
	}
	internal void TriggerSync(Action a)
	{
		if (!actions.Contains(a)) 
			return;
		a.Invoke();
		P2PBase.networkActions.Add(new ActionInvokeMessage(ID,actions.IndexOf(a)));
	}
	internal void TriggerByIndex(in int index){
		if (index>actions.Count)
			return;
		actions[index].Invoke();
	}
	[AttributeUsage(AttributeTargets.Method)]
	protected class CanTriggerSync : Attribute { }
}
