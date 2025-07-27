using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class DebugShaderexperement : MonoBehaviour
{
	public Color DetailsColor;
	[ExecuteInEditMode, ContextMenu("FUN")]
	void yes() => fun(true);
	public void fun(bool toDark)
	{
		Shader.SetGlobalColor("_Details", toDark ? DetailsColor : Color.black);
		Shader.SetGlobalFloat("_DetailsAlpha", 1);
	}
	[ExecuteInEditMode, ContextMenu("Reset")]
	public void Reset()
	{
		Shader.SetGlobalColor("_Details", new Color(0, 0, 0, 0));
		Shader.SetGlobalFloat("_DetailsAlpha", 0);
	}

	public GameObject quad; // Reference to your quad GameObject
	float blackThreshold = .05f; // Adjust this to define what you consider "black"
								 // Call this method to count black pixels
	RenderTexture renderTexture;
	Renderer quadRenderer;
	int totalRes;
	bool isDarkEnough()
	{
		// Save current active render texture
		RenderTexture previous = RenderTexture.active;
		try
		{
			// Create a temporary Texture2D to read pixels
			Texture2D tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

			// Set the active render texture and read pixels
			RenderTexture.active = renderTexture;
			tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			tempTexture.Apply();

			// Count black pixels
			int blackPixelCount = 0;
			Color[] pixels = tempTexture.GetPixels();

			foreach (Color pixel in pixels)
			{
				// Check if pixel is below threshold in all channels
				if ((pixel.r <= blackThreshold &&
					pixel.g <= blackThreshold &&
					pixel.b <= blackThreshold)
					|| colorsApprox(pixel, DetailsColor, .05f))
				{
					blackPixelCount++;
				}
			}
			// Clean up
			Destroy(tempTexture);

			return blackPixelCount / totalRes >= 1;
		}
		finally
		{
			// Restore previous render texture
			RenderTexture.active = previous;
		}
	}
	// Example usage
	void FixedUpdate()
	{
		var isDark = isDarkEnough();
		Debug.Log(isDark ? "dark" : "not dark");
		//fun(isDark);
	}
	void Start()
	{
		quadRenderer = quad.GetComponent<Renderer>();
		renderTexture = quadRenderer.material.mainTexture as RenderTexture;
		totalRes = renderTexture.width * renderTexture.height;
	}
	// Add this method to your DebugShaderexperement class
	bool colorsApprox(Color a, Color b, float tolerance = 0.05f)
	{
		return Mathf.Abs(a.r - b.r) < tolerance &&
			Mathf.Abs(a.g - b.g) < tolerance &&
			Mathf.Abs(a.b - b.b) < tolerance;
		// Note: We're not comparing alpha unless you need to
	}
}