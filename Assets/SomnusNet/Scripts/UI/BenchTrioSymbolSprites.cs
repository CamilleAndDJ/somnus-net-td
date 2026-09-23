using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.UI
{
    public static class BenchTrioSymbolSprites
    {
        const string BaseResourceName = "BenchTrioSymbol";

        public static Sprite Icon => GroupTypeSymbolSprites.GetFullRosterIcon(BaseResourceName);

        public static Sprite GetHudIcon(GridManager grid) =>
            GroupTypeSymbolSprites.GetHudIcon(BaseResourceName, BenchTrioTypeRules.MemberKinds, grid);
    }
}
