using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.UI
{
    public static class DreamTeamSymbolSprites
    {
        const string BaseResourceName = "DreamTeamSymbol";

        /// <summary>Full roster icon. Used on the Types info page.</summary>
        public static Sprite Icon => GroupTypeSymbolSprites.GetFullRosterIcon(BaseResourceName);

        public static Sprite GetHudIcon(GridManager grid) =>
            GroupTypeSymbolSprites.GetHudIcon(BaseResourceName, DreamTeamTypeRules.MemberKinds, grid);
    }
}
