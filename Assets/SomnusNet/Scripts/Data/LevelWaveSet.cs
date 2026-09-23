using System.Collections.Generic;
using UnityEngine;

namespace SomnusNet.Data
{
    [CreateAssetMenu(fileName = "LevelWaveSet", menuName = "Somnus Net/Level Wave Set")]
    public class LevelWaveSet : ScriptableObject
    {
        public string levelTitle = "Level 01 — First Breach";
        [TextArea] public string briefing =
            "The Bug Swarm has reached the Somnus Net. Place Dream Blobs on the grid. Protect the Dream Core on the left.";

        public float delayBetweenWaves = 8f;
        public List<WaveSpawnEntry> spawns = new();
    }
}
