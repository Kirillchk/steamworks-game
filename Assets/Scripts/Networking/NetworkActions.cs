using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using P2PMessages;
using System.Threading.Tasks;

public class NetworkActions : MonoBehaviour
{
    Vector3 ID;
	protected List<Action> actions = new();
    async void Awake()
	{
		//TODO: FIX! This should not be necessary
		await Task.Yield();
		ID = GetComponent<NetworkIdentity>().uniqueVector;
		P2PBase.networkActionScripts[ID] = this;
		
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(m => m.GetCustomAttributes(typeof(CanTriggerSync), false).Length > 0);
        foreach (var method in methods)
			actions.Add((Action)Delegate.CreateDelegate(typeof(Action), this, method));
		Debug.Log($"{methods.Count()}, {ID}");
	}
	internal void TriggerSync(Action a)
	{
		if (!actions.Contains(a))
			return;
		a.Invoke();
		P2PBase.ActionBulk.AddRange(
			INetworkMessage.StructToSpan(
				new ActionInvokeMessage()
				{
					ID = ID,
					Index = actions.IndexOf(a)
				}
			).ToArray()
		);
	}
	internal void TriggerByIndex(in int index){
		if (index>actions.Count)
			return;
		actions[index].Invoke();
	}
	[AttributeUsage(AttributeTargets.Method)]
	protected class CanTriggerSync : Attribute { }
}
