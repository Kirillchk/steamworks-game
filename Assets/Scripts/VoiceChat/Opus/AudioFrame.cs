using System;
using System.Runtime.InteropServices;

namespace Adrenak.UniVoice {
    [System.Serializable]
    /// <summary>
    /// A data structure representing the audio transmitted over the network.
    /// </summary>
    public struct AudioFrame
    {
        public int id;
        public int frequency;
        public int channelCount;
        public byte[] samples;
    }
}