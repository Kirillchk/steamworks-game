using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq.Expressions;
using MessagePack;

public class NetworkActions : MonoBehaviour
{
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
	internal void TriggerSync(in Delegate del, params object[] args)
	{
		del.DynamicInvoke(args);
		byte[] data = MessagePackSerializer.Serialize(args);
		P2PBase.DelegatePacks.Add(
			new P2PBase.DelegatePack()
			{
				ID = ID,
				Index = delegates.IndexOf(del),
				Length = data.Length,
				Args = data
			}
		);
	}
	internal void InvokeFromBytes(in int ind, in byte[] data) =>
		delegates[ind].DynamicInvoke(MessagePackSerializer.Deserialize<object[]>(data));
	[AttributeUsage(AttributeTargets.Method)]
	protected class CanTriggerSync : Attribute { }
}
public static class NetworkSyncExtensions
{
    public static void Sync(this NetworkActions na, Action action)
        => na.TriggerSync(action);
    
    public static void Sync<T>(this NetworkActions na, Action<T> action)
        => na.TriggerSync(action);
    
    public static void Sync<T1, T2>(this NetworkActions na, Action<T1, T2> action, T1 arg1, T2 arg2)
        => na.TriggerSync(action, arg1, arg2);
    
    public static void Sync<T1, T2, T3>(this NetworkActions na, Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        => na.TriggerSync(action, arg1, arg2, arg3);
    
    public static void Sync<T1, T2, T3, T4>(this NetworkActions na, Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        => na.TriggerSync(action, arg1, arg2, arg3, arg4);
    
    public static void Sync<T1, T2, T3, T4, T5>(this NetworkActions na, Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        => na.TriggerSync(action, arg1, arg2, arg3, arg4, arg5);
}