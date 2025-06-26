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
			delegates.Add(
				Delegate.CreateDelegate(
						Expression.GetActionType(method.GetParameters().Select(p => p.ParameterType).ToArray()),
						this,
						method
					));
		Debug.Log($"{methods.Count()}, {ID}");
	}
	// wraper
	internal void TriggerSync(in Delegate del, params object[] args)
	{
		del.DynamicInvoke(args);
		byte[] data = MessagePackSerializer.Serialize(args);
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
}
