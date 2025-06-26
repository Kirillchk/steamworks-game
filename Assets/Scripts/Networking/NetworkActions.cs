using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using P2PMessages;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using System.IO;
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
	internal void TriggerSync(Action act)
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
	internal void TriggerByIndex(in int index)
	{
		if (index > actions.Count)
			return;
		actions[index].Invoke();
	}
	// wraper
	internal void TriggerSyncWargs(Delegate del, params object[] wow)
	{
		//Delegate del = delegates[ind];
		if (!delegates.Contains(del))
			return;
		List<byte> data = new();
		del.Method.GetParameters();
		data.AddRange(ConvertObjectsToBinary(wow));
		// del.DynamicInvoke(wow);
		P2PBase.DelegateBulk.AddRange(
			new DelegateInvokeMessage()
			{
				ID = ID,
				Index = delegates.IndexOf(del),
				Length = data.Count,
				Args = data.ToArray()
			}.GetBinary()
		);
		Debug.Log(data.Count);
		InvokeFromBytes(delegates.IndexOf(del), data.ToArray());
		byte[] ConvertObjectsToBinary(params object[] objects)
		{
			string deb = "";
			deb += $"Length:{objects.Length}";
			// TODO: check if this is sexual harasment
			if (objects == null || objects.Length == 0)
				return Array.Empty<byte>();
			byte[] bytes = MessagePackSerializer.Serialize(objects);
			deb += $"len in bytes:{bytes.Length}";
			deb += "decrypted";
			foreach (object o in MessagePackSerializer.Deserialize<object[]>(bytes))
				deb += $"-{o}-";
			Debug.Log(deb);
			return bytes;
		}
	}
	// for invoking method after package
	internal void InvokeFromBytes(int ind, byte[] data)
	{
		Delegate targetMethod = delegates[ind];
		MethodInfo method = targetMethod.Method;
		ParameterInfo[] parameters = method.GetParameters();

		if (data.Length != CalculateExpectedSize())
			throw new ArgumentException("Data length does not match expected size.");

		object[] args = new object[parameters.Length];
		ReadOnlySpan<byte> dataSpan = data.AsSpan();
		int offset = 0;

		for (int i = 0; i < parameters.Length; i++)
		{
			Type paramType = parameters[i].ParameterType;
			int paramSize = Marshal.SizeOf(paramType);

			if (offset + paramSize > data.Length)
				throw new ArgumentException("Data buffer too small for parameter.");

			ReadOnlySpan<byte> paramBytes = dataSpan.Slice(offset, paramSize);
			offset += paramSize;

			args[i] = BytesToStruct(paramBytes, paramType);
		}

		method.Invoke(targetMethod.Target, args);
		int CalculateExpectedSize()
		{
			int size = 0;
			foreach (var param in parameters)
				size += Marshal.SizeOf(param.ParameterType);
			return size;
		}
		object BytesToStruct(ReadOnlySpan<byte> bytes, Type structType)
		{
			// Allocate unmanaged memory and copy bytes
			IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
			try
			{
				Marshal.Copy(bytes.ToArray(), 0, ptr, bytes.Length);
				return Marshal.PtrToStructure(ptr, structType);
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}
		}
	}
	[AttributeUsage(AttributeTargets.Method)]
	protected class CanTriggerSync : Attribute { }
	[AttributeUsage(AttributeTargets.Method)]
	protected class CanTriggerSyncWargs : Attribute { }
}
