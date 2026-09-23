using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.UI
{
    public static class SlimySupportSymbolSprites
    {
        const string BaseResourceName = "SlimySupportSymbol";

        public static readonly Color IconColor = new(0.42f, 0.88f, 0.45f);

        public static Sprite Icon => GroupTypeSymbolSprites.GetFullRosterIcon(BaseResourceName);

        public static Sprite GetHudIcon(GridManager grid) =>
            GroupTypeSymbolSprites.GetHudIcon(BaseResourceName, SlimySupportTypeRules.MemberKinds, grid);
    }
}
