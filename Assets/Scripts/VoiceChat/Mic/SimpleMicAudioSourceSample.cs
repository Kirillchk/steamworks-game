using System;
using UnityEngine;

namespace Adrenak.UniMic.Samples 
{
    public class SimpleMicAudioSourceSample : MonoBehaviour
    {
        [SerializeField] MicAudioSource micAudioSource;
        GameObject manager;
        SystemManager sysManager;
        void Start()
        {
            manager = GameObject.Find("Manager");
            Mic.Init();
            sysManager = manager.GetComponent<SystemManager>();
            sysManager.OnMicChange += SetupMic;
            SetupMic();
            
        }
        void SetupMic()
        {
            if (Mic.AvailableDevices.Count > 0)
            {
                foreach(var mic in Mic.AvailableDevices)
                    mic.StopRecording();
                if (sysManager.mic == null)
                {
                    Mic.AvailableDevices[0].StartRecording();
                    micAudioSource.Device = Mic.AvailableDevices[0];
                    return;
                }
                else
                {
                    int i = 0;
                    foreach (var device in Mic.AvailableDevices)
                    {
                        if (device.Name == sysManager.mic)
                        {
                            Mic.AvailableDevices[i].StartRecording();
                            micAudioSource.Device = Mic.AvailableDevices[i];
                            return;
                        }
                        i++;
                    }
                }
            }
        }
    }
}
