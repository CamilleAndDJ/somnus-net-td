using System.Collections.Generic;
using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.Data
{
    public class GameCatalog : MonoBehaviour
    {
        public List<BlobDefinition> blobs = new();
        public List<GlitchDefinition> glitches = new();
        public LevelWaveSet level01Waves;

        readonly Dictionary<BlobKind, BlobDefinition> _blobMap = new();
        readonly Dictionary<GlitchKind, GlitchDefinition> _glitchMap = new();

        void Awake()
        {
            foreach (var b in blobs)
            {
                if (b == null) continue;
                _blobMap[BlobKinds.Normalize(b.kind)] = b;
            }

            foreach (var g in glitches)
                RegisterGlitch(g);

            foreach (GlitchKind kind in System.Enum.GetValues(typeof(GlitchKind)))
            {
                if (_glitchMap.ContainsKey(kind))
                    continue;

                var fallback = GlitchRuntimeDefaults.TryCreate(kind);
                if (fallback != null)
                    RegisterGlitch(fallback);
            }
        }

        void RegisterGlitch(GlitchDefinition def)
        {
            if (def == null) return;
            _glitchMap[def.kind] = def;
        }

        public BlobDefinition GetBlob(BlobKind kind) => _blobMap[BlobKinds.Normalize(kind)];
        public bool TryGetBlob(BlobKind kind, out BlobDefinition def) =>
            _blobMap.TryGetValue(BlobKinds.Normalize(kind), out def);
        public GlitchDefinition GetGlitch(GlitchKind kind) => _glitchMap[kind];

        public bool TryGetGlitch(GlitchKind kind, out GlitchDefinition def) => _glitchMap.TryGetValue(kind, out def);
    }
}
