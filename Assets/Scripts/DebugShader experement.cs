using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class DebugShaderexperement : MonoBehaviour
{
	public Color DetailsColor;
	[ExecuteInEditMode, ContextMenu("FUN")]
	public async void fun()
	{
		Debug.Log("changing shit");
		Shader.SetGlobalColor("_Base", Color.black);

		for (float t = 0; t < 1; t += Time.deltaTime / 3)
		{
			Shader.SetGlobalColor("_Details", Color.Lerp(Color.black, DetailsColor, t));
			await Task.Delay((int)(Time.deltaTime * 500));
		}
		Shader.SetGlobalColor("_Details", DetailsColor);
	}

	[ExecuteInEditMode, ContextMenu("Reset")]
	public void Reset()
	{
		Shader.SetGlobalColor("_Base", Color.black);
		Shader.SetGlobalColor("_Details", Color.black);
	}
	
}