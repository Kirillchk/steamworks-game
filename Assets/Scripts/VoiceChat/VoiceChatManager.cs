using UnityEngine;
using Concentus;
using System;
public class VoiceChatManager : MonoBehaviour
{
    int sampleRate = 48000;
    int channels =1;
    [SerializeField]int frequency = 22050;
    string selectedDevice = "Микрофон (3- fifine Microphone)";
    AudioClip audioClip;
    [SerializeField]AudioSource audioSource;
    public void Start()
    {
        InvokeRepeating("Test1",0,1);
        InvokeRepeating("Test2",1,1);
    }
    public void Test1()
    {
        audioClip = Microphone.Start(selectedDevice, false, 1, sampleRate);
    }
    void Test2()
    {
        audioSource.clip = AudioClip.Create("test", audioClip.samples, 1, sampleRate, false);
        float[] floatData = new float[audioClip.samples * channels];

        audioClip.GetData(floatData,0);
        audioSource.clip.SetData(floatData,0);

        audioSource.Play();
    }
    public void Update()
    {
         
    }
}