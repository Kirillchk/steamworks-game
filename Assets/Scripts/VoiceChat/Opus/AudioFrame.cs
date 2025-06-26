using System;
using System.Runtime.InteropServices;

namespace Adrenak.UniVoice {
    [System.Serializable]
    /// <summary>
    /// A data structure representing the audio transmitted over the network.
    /// </summary>
    public struct AudioFrame
    {
        public byte id;
        public int frequency;
        public int channelCount;

        public int samplesLength;
        byte[] _samples;
        public byte[] samples
        {
            get { return _samples; }
            set
            {
                if (value != null)
                {
                    intPtr = IntPtr.Zero;
                    try
                    {
                        intPtr = Marshal.AllocHGlobal(samplesLength);
                        Marshal.Copy(value, 0, intPtr, samplesLength);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(intPtr);
                    }
                }
                _samples = value;
            }
        }
        public IntPtr intPtr;
    }
}