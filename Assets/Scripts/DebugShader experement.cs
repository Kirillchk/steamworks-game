using UnityEngine;
public class DebugShaderexperement : MonoBehaviour
{
	public Color DetailsColor;
	public RenderTexture renderTexture;
	static Color currentColor;
	static float totalRes;
	void Start()
	{
		totalRes = renderTexture.width * renderTexture.height;
		Shader.SetGlobalColor("_Details", currentColor);
	}
	void FixedUpdate()
	{
		var isDark = isDarkEnough();
		currentColor = isDark ? Color.Lerp(currentColor, DetailsColor, .0001f) : Color.black;
		Shader.SetGlobalColor("_Details", currentColor);
		Shader.SetGlobalFloat("_DetailsAlpha", isDark ? 1 : 0);
	}
	bool isDarkEnough()
	{
		const float tolerance = .0025f;
		RenderTexture previous = RenderTexture.active;
		try
		{
			// Create a temporary Texture2D to read pixels
			Texture2D tempTexture = new(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

			RenderTexture.active = renderTexture;
			tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			tempTexture.Apply();

			int blackPixelCount = 0;
			int glowPixelCount = 0;
			int lightPixelCount = 0;
			Color[] pixels = tempTexture.GetPixels();
			foreach (Color pixel in pixels)
			{
				var isBlack = colorsApprox(pixel, Color.black);
				var isGlow = colorsApprox(pixel, currentColor);
				if (isBlack)
					blackPixelCount++;
				else if (isGlow)
					glowPixelCount++;
				else
					lightPixelCount++;
			}
			Destroy(tempTexture);

			var all = totalRes - glowPixelCount;
			var res = lightPixelCount / all <= 0;
			//Debug.Log($"{res}:{lightPixelCount}/{all}={lightPixelCount / all}");
			return res;
		}
		finally
		{
			// Restore previous render texture
			RenderTexture.active = previous;
		}
		bool colorsApprox(Color a, Color b) =>
			Mathf.Abs(a.r - b.r) <= tolerance &&
			Mathf.Abs(a.g - b.g) <= tolerance &&
			Mathf.Abs(a.b - b.b) <= tolerance;
	}
	[ExecuteInEditMode, ContextMenu("TurnOn")]
	void TurnOn()
	{
		Shader.SetGlobalColor("_Details", DetailsColor);
		Shader.SetGlobalFloat("_DetailsAlpha", 1);
	}
	[ExecuteInEditMode, ContextMenu("TurnOff")]
	void TurnOff()
	{
		Shader.SetGlobalColor("_Details", Color.black);
		Shader.SetGlobalFloat("_DetailsAlpha", 0);
	}
}