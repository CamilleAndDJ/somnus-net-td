using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class EntertainerSynergy
    {
        public static int GetBonusDamage(DreamBlob recipient)
        {
            if (!EntertainerTypeRules.FeatureEnabled || recipient == null || !recipient.IsAlive ||
                GridManager.Instance == null)
                return 0;

            var hasAdjacentEntertainer = false;
            GridManager.Instance.ForEachAdjacentBlob(recipient.Column, recipient.Lane, blob =>
            {
                if (blob != null && blob.IsAlive && blob != recipient && blob.HasType(EntertainerTypeRules.Label))
                    hasAdjacentEntertainer = true;
            });

            return hasAdjacentEntertainer ? EntertainerTypeRules.DamagePerAdjacentEntertainer : 0;
        }
    }
}
