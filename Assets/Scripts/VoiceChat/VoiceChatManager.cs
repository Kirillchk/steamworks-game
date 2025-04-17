using Steamworks;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEngine;

public class VoiceChatManager : MonoBehaviour
{
    public byte[] buffer = new byte[1024];
    public uint bufferSize = 1024;
    public uint bytesWritten = 0;
    [ContextMenu("Record Voice")]
    private void Record()
    {
        SteamUser.StartVoiceRecording();
    }
    [ContextMenu("Stop recording")]
    private void Stop()
    {
        SteamUser.StopVoiceRecording();
    }
    [ContextMenu("Get voice")]
    private void GetVoice()
    {
        Debug.Log(SteamUser.GetVoice(true, buffer,bufferSize, out bytesWritten));
    }
}
