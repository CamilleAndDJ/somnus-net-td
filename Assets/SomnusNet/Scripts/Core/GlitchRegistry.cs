using System.Collections.Generic;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class GlitchRegistry
    {
        static readonly List<Glitch> Active = new();

        public static IReadOnlyList<Glitch> All => Active;

        public static void Register(Glitch glitch)
        {
            if (glitch == null || Active.Contains(glitch)) return;
            Active.Add(glitch);
        }

        public static void Unregister(Glitch glitch)
        {
            if (glitch == null) return;
            Active.Remove(glitch);
        }

        public static void ClearRemaining()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                var glitch = Active[i];
                if (glitch != null && glitch.IsAlive)
                    UnityEngine.Object.Destroy(glitch.gameObject);
            }

            Active.Clear();
        }
    }
}
