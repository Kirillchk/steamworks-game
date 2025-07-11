﻿using MessagePack;
namespace Adrenak.UniVoice {
    [System.Serializable]
    [MessagePackObject]
    /// <summary>
    /// A data structure representing the audio transmitted over the network.
    /// </summary>
    public struct AudioFrame
    {
        [IgnoreMember]
        public byte id;
        [Key(0)]
        public int frequency;
        [Key(1)]
        public int channelCount;
        [Key(2)]
        public byte[] samples;
    }
}