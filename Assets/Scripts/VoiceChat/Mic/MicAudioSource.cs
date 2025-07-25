using UnityEngine;
using Adrenak.UniVoice;
using Adrenak.UniVoice.Filters;
using System.Runtime.InteropServices;
using System;
namespace Adrenak.UniMic
{
    /// <summary>
    /// A wrapper over StreamedAudioSource to play what a <see cref="Mic.Device"/>
    /// is capturing. 
    /// </summary>
    [RequireComponent(typeof(StreamedAudioSource))]
    public class MicAudioSource : MonoBehaviour
    {
		public static MicAudioSource only;
        [SerializeField] Mic.Device device;
        public Mic.Device Device
        {
            get => device;
            set
            {
                if (device != null)
                {
                    device.OnStartRecording -= OnStartRecording;
                    device.OnFrameCollected -= OnFrameCollected;
                    device.OnStopRecording -= OnStopRecording;
                    Debug.Log("Device removed from MicAudioSource", gameObject);
                }
                if (value != null)
                {
                    device = value;
                    device.OnStartRecording += OnStartRecording;
                    device.OnFrameCollected += OnFrameCollected;
                    device.OnStopRecording += OnStopRecording;
                    if (device.IsRecording)
                        StreamedAudioSource.Play();
                    else
                        StreamedAudioSource.Stop();
                    Debug.Log("MicAudioSource shifted to " + device.Name, gameObject);
                }
                else
                    StreamedAudioSource.Stop();
            }
        }

        StreamedAudioSource streamedAudioSource;
        public StreamedAudioSource StreamedAudioSource
        {
            get
            {
                if (streamedAudioSource == null)
                    streamedAudioSource = gameObject.GetComponent<StreamedAudioSource>();
                if (streamedAudioSource == null)
                    streamedAudioSource = gameObject.AddComponent<StreamedAudioSource>();
                return streamedAudioSource;
            }
        }
        void OnStartRecording()
        {
            StreamedAudioSource.Play();
        }
        ConcentusEncodeFilter encoder;
        ConcentusDecodeFilter decoder;
        AudioFrame audioFrame;
        AudioFrame encodedAudio;
        AudioFrame decodedAudio;
		void Start()
		{
			P2PBase.source = this;
			Debug.Log("added only");
			encoder = new ConcentusEncodeFilter(
			ConcentusFrequencies.Frequency_48000,
			8,
			8,
			64000,
			46080);
			decoder = new ConcentusDecodeFilter();
			audioFrame = new AudioFrame();
        }
        void OnFrameCollected(int frequency, int channels, float[] samples)
        {
            audioFrame.frequency = frequency;
            audioFrame.channelCount = channels;
            audioFrame.samples = Utils.Bytes.FloatsToBytes(samples);
            encodedAudio = encoder.Run(audioFrame);

            SendFrame(encodedAudio);
        }
        void SendFrame(AudioFrame audioFrame)
        {
            P2PBase.audioFrame = audioFrame;
        }
        public void RecieveFrame(AudioFrame audioFrame)
        {
            decodedAudio = decoder.Run(audioFrame);
            float[] audio = Utils.Bytes.BytesToFloats(decodedAudio.samples);
            StreamedAudioSource.Feed(decodedAudio.frequency, decodedAudio.channelCount, audio);
        }

        void OnStopRecording()
        {
            StreamedAudioSource.Stop();
        }
    }
    
}
