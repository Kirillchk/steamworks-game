using UnityEngine;
using Steamworks;
public class VoiceChatManager : MonoBehaviour
{
    [SerializeField]private AudioSource audioSource;
    private uint compressedSize = 0;
    private uint bytesWritten = 0;
    [ContextMenu("Record Voice")]
    private void StartRecord()
    {
        SteamUser.StartVoiceRecording();
    }
    [ContextMenu("Stop recording")]
    private void StopRecording()
    {
        SteamUser.StopVoiceRecording();
    }
    [ContextMenu("Get voice")]
private void GetRecordedVoice()
    {
        EVoiceResult voiceResult = SteamUser.GetAvailableVoice(out compressedSize);
        Debug.Log($"voiceResult{voiceResult}");
        if(voiceResult == EVoiceResult.k_EVoiceResultOK && compressedSize > 0)
        {
            byte[] compressedVoiceBuffer = new byte[compressedSize];
            voiceResult = SteamUser.GetVoice(true, compressedVoiceBuffer, compressedSize, out bytesWritten);
            Debug.Log($"voiceResult{voiceResult}");
            if(voiceResult == EVoiceResult.k_EVoiceResultOK && bytesWritten > 0)
            {
                // Get optimal sample rate and calculate a safe buffer size
                uint optimalSampleRate = SteamUser.GetVoiceOptimalSampleRate();
                
                // A safe estimate for decompressed size (2 bytes per sample, 1 channel)
                uint decompressedSizeEstimate = optimalSampleRate * 2; // 1 second of audio
                byte[] decompressedBuffer = new byte[decompressedSizeEstimate];
                
                EVoiceResult result = SteamUser.DecompressVoice(
                    compressedVoiceBuffer,
                    bytesWritten, // Use the actual bytes written from GetVoice
                    decompressedBuffer,
                    decompressedSizeEstimate,
                    out uint decompressedBytesWritten,
                    optimalSampleRate);
                Debug.Log($"result{result}");
                if(result == EVoiceResult.k_EVoiceResultOK && decompressedBytesWritten > 0)
                {
                    // Convert to float array (2 bytes per sample)
                    int sampleCount = (int)decompressedBytesWritten / 2;
                    float[] floatData = new float[sampleCount];
                    
                    for (int i = 0; i < sampleCount; i++)
                    {
                        if((i * 2 + 1) < decompressedBytesWritten)
                        {
                            floatData[i] = (short)(decompressedBuffer[i * 2] | (decompressedBuffer[i * 2 + 1] << 8)) / 32768.0f;
                        }
                    }
                    
                    AudioClip clip = AudioClip.Create(
                        "Voice", 
                        sampleCount,
                        1,
                        (int)optimalSampleRate,
                        false);
                    
                    clip.SetData(floatData, 0);
                    audioSource.PlayOneShot(clip);
                }
            }
        }
    }
}
