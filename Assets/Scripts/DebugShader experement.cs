
using System;
using UnityEngine;

public class DebugShaderexperement : MonoBehaviour
{
	public float tolerance1 = .005f, tolerance2 = .005f, percentage = .8f;
	public GameObject quad; 
	public Color DetailsColor;
	Color currentColor;
	RenderTexture renderTexture;
	Renderer quadRenderer;
	float totalRes;
	bool isDarkEnough()
	{
		RenderTexture previous = RenderTexture.active;
		try
		{
			// Create a temporary Texture2D to read pixels
			Texture2D tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

			// Set the active render texture and read pixels
			RenderTexture.active = renderTexture;
			tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			tempTexture.Apply();

			int blackPixelCount = 0;
			int glowPixelCount = 0;
			Color[] pixels = tempTexture.GetPixels();

			foreach (Color pixel in pixels)
			{
				var isBlack = colorsApprox(pixel, new(0, 0, 0), tolerance1);
				var isGlow = colorsApprox(pixel, currentColor, tolerance2);
				if (isBlack)
					blackPixelCount++;
				else if (isGlow)
					glowPixelCount++;
			}
			Destroy(tempTexture);
			var dark = blackPixelCount;
			var light = totalRes - blackPixelCount - glowPixelCount;
			var all = totalRes - glowPixelCount;
			var res = light / all <= 1-percentage;
			Debug.Log($"{res}-{light}/{all}={light / all}");
			return res;
		}
		finally
		{
			// Restore previous render texture
			RenderTexture.active = previous;
		}
	}
	bool isDark = false;
	void FixedUpdate()
	{
		isDark = isDarkEnough();
		Shader.SetGlobalColor("_Details", currentColor);
		Shader.SetGlobalFloat("_DetailsAlpha", isDark ? 1 : 0);
		currentColor = isDark ? Color.Lerp(currentColor, DetailsColor, .001f) : Color.black;
	}
	void Start()
	{
		quadRenderer = quad.GetComponent<Renderer>();
		renderTexture = quadRenderer.material.mainTexture as RenderTexture;
		totalRes = renderTexture.width * renderTexture.height;
		Shader.SetGlobalColor("_Details", currentColor);
	}
	// Add this method to your DebugShaderexperement class
	bool colorsApprox(Color a, Color b, float tolerance)=>
		Mathf.Abs(a.r - b.r) <= tolerance &&
		Mathf.Abs(a.g - b.g) <= tolerance &&
		Mathf.Abs(a.b - b.b) <= tolerance;
}