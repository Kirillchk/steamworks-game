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
	public Camera targetCamera;
    public int downscaleWidth = 64;
    public int downscaleHeight = 64;
    
    private RenderTexture renderTexture;
    private Texture2D tempTexture;

    void Start()
    {
        // Create a downscaled render texture
        renderTexture = new RenderTexture(downscaleWidth, downscaleHeight, 0);
        targetCamera.targetTexture = renderTexture;
        
        // Create a Texture2D for reading pixels
        tempTexture = new Texture2D(downscaleWidth, downscaleHeight, TextureFormat.RGB24, false);
    }

    public int CountBlackPixels(float blackThreshold = 0.1f)
    {
        // Save current render target
        RenderTexture previous = RenderTexture.active;
        
        // Set our render texture as active
        RenderTexture.active = renderTexture;
        
        // Read the pixels into the Texture2D
        tempTexture.ReadPixels(new Rect(0, 0, downscaleWidth, downscaleHeight), 0, 0);
        tempTexture.Apply();
        
        // Restore previous render target
        RenderTexture.active = previous;
        
        // Count black pixels
        int blackPixelCount = 0;
        Color[] pixels = tempTexture.GetPixels();
        
        foreach (Color pixel in pixels)
        {
            // Consider pixel black if all color channels are below threshold
            if (pixel.r <= blackThreshold && 
                pixel.g <= blackThreshold && 
                pixel.b <= blackThreshold)
            {
                blackPixelCount++;
            }
        }
        
        return blackPixelCount;
    }

    void Update()
    {
        // Example usage
        int blackPixels = CountBlackPixels();
        Debug.Log("Black pixels: " + blackPixels);
    }

    void OnDestroy()
    {
        if (renderTexture != null)
            renderTexture.Release();
    }
}