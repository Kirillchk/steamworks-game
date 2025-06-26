using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using P2PMessages;
using System.Threading.Tasks;
using System.Linq.Expressions;
using MessagePack;

public class NetworkActions : MonoBehaviour
{
	// TODO: AAAAAAAAAAAAAAAAAAAAAA wtf is this piss
    Vector3 ID;
	protected List<Action> actions = new();
	protected List<Delegate> delegates = new();
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
		
		var methodsWargs = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(m => m.GetCustomAttributes(typeof(CanTriggerSyncWargs), false).Length > 0);
		foreach (var method in methodsWargs)
			delegates.Add(Delegate.CreateDelegate(Expression.GetActionType(method.GetParameters().Select(p => p.ParameterType).ToArray()), this, method));
		Debug.Log($"{methods.Count()}, {ID}");
	}
	// wraper
	internal void TriggerSync(in Action act)
	{
		if (!actions.Contains(act))
			return;
		act.Invoke();
		P2PBase.ActionBulk.AddRange(
			INetworkMessage.StructToSpan(
				new ActionInvokeMessage()
				{
					ID = ID,
					Index = actions.IndexOf(act)
				}
			).ToArray()
		);
	}
	// for invoking method after package
	internal void TriggerByIndex(in int index) =>
		actions[index].Invoke();
	// wraper
	internal void TriggerSyncWargs(in Delegate del, params object[] wow)
	{
		del.DynamicInvoke(wow);
		byte[] data = MessagePackSerializer.Serialize(wow);
		P2PBase.DelegateBulk.AddRange(
			new DelegateInvokeMessage()
			{
				ID = ID,
				Index = delegates.IndexOf(del),
				Length = data.Length,
				Args = data
			}.GetBinary()
		);
	}
	// for invoking method after package
	internal void InvokeFromBytes(in int ind, in byte[] data) =>
		delegates[ind].DynamicInvoke(MessagePackSerializer.Deserialize<object[]>(data));
	[AttributeUsage(AttributeTargets.Method)]
	protected class CanTriggerSync : Attribute { }
	[AttributeUsage(AttributeTargets.Method)]
	protected class CanTriggerSyncWargs : Attribute { }
}
