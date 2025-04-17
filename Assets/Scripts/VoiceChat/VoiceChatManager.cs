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
    Debug.Log($"GetAvailableVoice: {voiceResult}, Size: {compressedSize}");

    if (voiceResult == EVoiceResult.k_EVoiceResultOK && compressedSize > 0)
    {
        byte[] compressedBuffer = new byte[compressedSize];
        voiceResult = SteamUser.GetVoice(
            true, //compressed?
            compressedBuffer, //data 
            compressedSize,  //data size
            out bytesWritten); // bytes count
        Debug.Log($"GetVoice: {voiceResult}, Bytes Written: {bytesWritten}");

        if (voiceResult == EVoiceResult.k_EVoiceResultOK && bytesWritten > 0)
        {
            uint sampleRate = 48000; // Steam's recommended rate
            uint decompressedBufferSize = sampleRate;
            byte[] decompressedBuffer = new byte[decompressedBufferSize];
            voiceResult = SteamUser.DecompressVoice(
                compressedBuffer,// data to decompress
                bytesWritten,// how many bytes data has
                decompressedBuffer,// data of decompressed data
                decompressedBufferSize,// size of decompressed data
                out uint decompressedBytesWritten, // how many bytes in decompressed data
                sampleRate);
            Debug.Log(voiceResult);

            if (voiceResult == EVoiceResult.k_EVoiceResultOK && decompressedBytesWritten > 0)
            {
                int sampleCount = (int)decompressedBytesWritten / 2;
                float[] floatData = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    short pcmSample = (short)(decompressedBuffer[i * 2] | (decompressedBuffer[i * 2 + 1] << 8));
                    floatData[i] = pcmSample / 1024*32;
                }

                AudioClip clip = AudioClip.Create(
                    "VoiceClip",
                    sampleCount,
                    1,
                    (int)sampleRate,
                    false
                );

                clip.SetData(floatData, 0);
                audioSource.PlayOneShot(clip);
                Debug.Log("Playing voice clip!");
            }
        }
    }
}
}
