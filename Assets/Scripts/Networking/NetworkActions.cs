using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using P2PMessages;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class NetworkActions : MonoBehaviour
{
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
		
		//P2PBase.networkActionScripts[ID] = this;
		var methodsWargs = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(m => m.GetCustomAttributes(typeof(CanTriggerSyncWargs), false).Length > 0);
		foreach (var method in methodsWargs)
			actions.Add((Action)Delegate.CreateDelegate(typeof(Delegate), this, method));
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
	
	internal void InvokeFromBytes(byte[] data, Delegate targetMethod)
	{
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
	}

	private static object BytesToStruct(ReadOnlySpan<byte> bytes, Type structType)
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


	public static object SpanToStruct<T>(ReadOnlySpan<byte> bytes) where T : unmanaged
	{
		if (bytes.Length < Marshal.SizeOf<T>())
			throw new ArgumentException("Byte array is too small for the target type", nameof(bytes));

		return MemoryMarshal.Read<T>(bytes);
	}


	[AttributeUsage(AttributeTargets.Method)]
	protected class CanTriggerSync : Attribute { }
	[AttributeUsage(AttributeTargets.Method)]
	protected class CanTriggerSyncWargs : Attribute { }
}
