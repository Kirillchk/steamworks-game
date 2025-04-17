using UnityEngine;
using Steamworks;

public class VoiceChatManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    private uint compressedSize = 0;
    private uint bytesWritten = 0;

    [ContextMenu("Record Voice")]
    private void StartRecord()
    {
        Debug.Log("[Voice] Starting voice recording...");
        SteamUser.StartVoiceRecording();
    }

    [ContextMenu("Stop recording")]
    private void StopRecording()
    {
        Debug.Log("[Voice] Stopping voice recording...");
        SteamUser.StopVoiceRecording();
    }

[ContextMenu("Get voice")]
private void GetRecordedVoice()
{
    Debug.Log("[Voice] === Starting voice processing ===");
    
    // 1. Check for available voice data
    EVoiceResult voiceResult = SteamUser.GetAvailableVoice(out compressedSize);
    Debug.Log($"[Voice] [1/6] GetAvailableVoice - Result: {voiceResult}, Size: {compressedSize} bytes");
    
    if(voiceResult != EVoiceResult.k_EVoiceResultOK || compressedSize == 0)
    {
        Debug.LogWarning($"[Voice] No voice data available (Result: {voiceResult}, Size: {compressedSize})");
        return;
    }

    // 2. Get compressed data
    byte[] compressedVoiceBuffer = new byte[compressedSize];
    voiceResult = SteamUser.GetVoice(true, compressedVoiceBuffer, compressedSize, out bytesWritten);
    Debug.Log($"[Voice] [2/6] GetVoice - Result: {voiceResult}, BytesWritten: {bytesWritten}, BufferSize: {compressedVoiceBuffer.Length}");

    if(voiceResult != EVoiceResult.k_EVoiceResultOK || bytesWritten == 0)
    {
        Debug.LogWarning($"[Voice] Failed to get compressed voice (Result: {voiceResult}, Bytes: {bytesWritten})");
        return;
    }

    // 3. Prepare for decompression
    uint optimalSampleRate = SteamUser.GetVoiceOptimalSampleRate();
    Debug.Log($"[Voice] [3/6] Optimal Sample Rate: {optimalSampleRate}Hz");

    // Calculate buffer size for 120ms of audio (typical Steam voice packet)
    uint decompressedSizeEstimate = (optimalSampleRate * 12 / 100) * 2; // 120ms * 2 bytes/sample
    byte[] decompressedBuffer = new byte[decompressedSizeEstimate];
    Debug.Log($"[Voice] [4/6] Decompression Buffer: {decompressedBuffer.Length} bytes");

    // 4. Decompress the audio
    EVoiceResult decompressResult = SteamUser.DecompressVoice(
        compressedVoiceBuffer,
        bytesWritten,
        decompressedBuffer,
        decompressedSizeEstimate,
        out uint decompressedBytesWritten,
        optimalSampleRate);

    Debug.Log($"[Voice] [5/6] DecompressVoice - Result: {decompressResult}, BytesWritten: {decompressedBytesWritten}");

    if(decompressResult != EVoiceResult.k_EVoiceResultOK || decompressedBytesWritten == 0)
    {
        Debug.LogWarning($"[Voice] Decompression failed (Result: {decompressResult}, Bytes: {decompressedBytesWritten})");
        
        // Additional troubleshooting:
        if(bytesWritten > compressedVoiceBuffer.Length)
        {
            Debug.LogError($"Buffer overflow! Got {bytesWritten} bytes but buffer is {compressedVoiceBuffer.Length}");
        }
        return;
    }

    // 5. Convert to Unity audio format
    int sampleCount = (int)decompressedBytesWritten / 2;
    float[] floatData = new float[sampleCount];
    Debug.Log($"[Voice] [6/6] Converting {sampleCount} samples to float");

    for(int i = 0; i < sampleCount; i++)
    {
        int bytePos = i * 2;
        if(bytePos + 1 >= decompressedBytesWritten) break;
        
        short sample = (short)(decompressedBuffer[bytePos] | (decompressedBuffer[bytePos + 1] << 8));
        floatData[i] = Mathf.Clamp(sample / 32768.0f, -1f, 1f);
    }

    // 6. Create and play audio clip
    AudioClip clip = AudioClip.Create("Voice", sampleCount, 1, (int)optimalSampleRate, false);
    clip.SetData(floatData, 0);
    
    if(audioSource == null)
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        Debug.Log("[Voice] Created new AudioSource");
    }

    Debug.Log($"[Voice] Playing clip: {sampleCount} samples at {optimalSampleRate}Hz");
    audioSource.PlayOneShot(clip);
}

    void OnDestroy()
    {
        Debug.Log("[Voice] Cleaning up - stopping any active recording");
        SteamUser.StopVoiceRecording();
    }
    [ContextMenu("Check")]
    private void Check()
    {
        Debug.Log("Available Unity microphones:");
        foreach (string device in Microphone.devices)
        {
            Debug.Log($"- {device}");
        }
    }
}