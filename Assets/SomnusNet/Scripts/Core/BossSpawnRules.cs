using SomnusNet.Data;

namespace SomnusNet.Core
{
    public static class BossSpawnRules
    {
        public static GlitchKind ResolveLastSpawnKind(int round) =>
            round == CicadianRhythmRules.SpawnRound
                ? GlitchKind.CicadianRhythm
                : MiniBossRules.PickRandom();
    }
}
