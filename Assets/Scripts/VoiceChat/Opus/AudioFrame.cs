using System.Runtime.InteropServices;

namespace Adrenak.UniVoice {
    [System.Serializable]
    /// <summary>
    /// A data structure representing the audio transmitted over the network.
    /// </summary>
    public struct AudioFrame
    {
        public byte id;
        /// <summary>
        /// The frequency (or sampling rate) of the audio
        /// </summary>
        public int frequency;

        /// <summary>
        /// The number of channels in the audio
        /// </summary>
        public int channelCount;
        /// <summary>
        /// A byte array representing the audio data
        /// </summary>
        public byte[] samples;
    }
}