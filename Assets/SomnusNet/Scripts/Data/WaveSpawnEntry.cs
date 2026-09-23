using System;
using UnityEngine;

namespace SomnusNet.Data
{
    [Serializable]
    public class WaveSpawnEntry
    {
        public GlitchKind glitch;
        public int lane;
        public float delayFromWaveStart;
    }
}
