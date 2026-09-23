using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.UI
{
    public static class BrokenRingsSymbolSprites
    {
        const string BaseResourceName = "BrokenRingsSymbol";

        /// <summary>Full roster icon. Used on the Types info page.</summary>
        public static Sprite Icon => GroupTypeSymbolSprites.GetFullRosterIcon(BaseResourceName);

        public static Sprite GetHudIcon(GridManager grid) =>
            GroupTypeSymbolSprites.GetHudIcon(BaseResourceName, BrokenRingsTypeRules.MemberKinds, grid);
    }
}
