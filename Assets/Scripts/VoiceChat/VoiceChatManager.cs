using UnityEngine;
using Steamworks;
using System;
public class VoiceChatManager : MonoBehaviour
{
    [SerializeField]private AudioSource audioSource;
    private uint dataSize = 0;
    private uint bytesWritten = 0;
    private uint sampleRate;
    public int check = 2;
    [ContextMenu("Record Voice")]
    private void Start()
    {
        sampleRate = 48000;
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

                uint decDataSize = sampleRate;
                byte[] decData = new byte[decDataSize];
                voiceResult = SteamUser.DecompressVoice(
                    data,// data to decompress
                    bytesWritten,// how many bytes data has
                    decData,// data of decompressed data
                    decDataSize,// size of decompressed data
                    out uint decBytesWritten, // how many bytes in decompressed data
                    sampleRate);
                Debug.Log(voiceResult);

                if (voiceResult == EVoiceResult.k_EVoiceResultOK && decBytesWritten > 0)
                {
                    int sampleCount = (int)decBytesWritten / check;
                    float[] floatData = new float[sampleCount];

                    for (int i = 0; i < sampleCount; i++)
                    {
                        short pcmSample = (short)((decData[i * 2 + 1] << 8) | decData[i * 2]); 
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