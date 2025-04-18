using UnityEngine;
using Steamworks;
public class VoiceChatManager : MonoBehaviour
{
    [SerializeField]private AudioSource audioSource;
    private uint dataSize = 0;
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
    private void Update()
    {
        EVoiceResult voiceResult = SteamUser.GetAvailableVoice(out dataSize);

        if (voiceResult == EVoiceResult.k_EVoiceResultOK && dataSize > 0)
        {
            byte[] data = new byte[dataSize];
            voiceResult = SteamUser.GetVoice(
                true, //compressed?
                data, //data 
                dataSize,  //data size
                out bytesWritten); // bytes count

            if (voiceResult == EVoiceResult.k_EVoiceResultOK && bytesWritten > 0)
            {
                uint sampleRate = 48000; // Steam's recommended rate
                uint decDataSize = sampleRate;
                byte[] decData = new byte[decDataSize];
                voiceResult = SteamUser.DecompressVoice(
                    data,// data to decompress
                    bytesWritten,// how many bytes data has
                    decData,// data of decompressed data
                    decDataSize,// size of decompressed data
                    out uint decompressedBytesWritten, // how many bytes in decompressed data
                    sampleRate);
                Debug.Log(voiceResult);

                if (voiceResult == EVoiceResult.k_EVoiceResultOK && decompressedBytesWritten > 0)
                {
                    int sampleCount = (int)decompressedBytesWritten / 2;
                    float[] floatData = new float[sampleCount];

                    for (int i = 0; i < sampleCount; i++)
                    {
                        short pcmSample = (short)(decData[i * 2] | (decData[i * 2 + 1] << 8));
                        floatData[i] = pcmSample / 32768.0f;
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