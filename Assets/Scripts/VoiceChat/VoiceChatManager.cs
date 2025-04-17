using UnityEngine;
using Steamworks;
public class VoiceChatManager : MonoBehaviour
{
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
        if(voiceResult==EVoiceResult.k_EVoiceResultOK && compressedSize>0)
        {
            byte[] compressedVoiceBuffer = new byte[compressedSize];
            voiceResult = SteamUser.GetVoice(true,compressedVoiceBuffer,compressedSize,out bytesWritten);
            if(voiceResult==EVoiceResult.k_EVoiceResultOK && bytesWritten>0)
            {
                uint decompressedSize = SteamUser.GetVoiceOptimalSampleRate();
                byte[] decompressedBuffer = new byte[compressedSize];
                EVoiceResult result = SteamUser.DecompressVoice(compressedVoiceBuffer,compressedSize,);
            }
        }
    }

}
