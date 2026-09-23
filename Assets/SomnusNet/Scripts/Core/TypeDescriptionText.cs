using System.Collections.Generic;

namespace SomnusNet.Core
{
    public static class TypeDescriptionText
    {
        public static string PopupTitle(string typeLabel, BlobTypeCategory category) =>
            $"{typeLabel} — {BlobTypeCategoryLabels.DisplayName(category)}";

        public static string GeneralTypeBody(string mechanicsDescription) =>
            "All unique blobs with this type share the same bonus.\n\n" + mechanicsDescription;

        public static string GroupTypeBody(DreamTeamTypeRules.MemberDefinition[] members) =>
            GroupTypeBodyFromLines(members != null
                ? System.Array.ConvertAll(members, m => m.effectLine)
                : null);

        public static string GroupTypeBodyFromLines(string[] effectLines)
        {
            var lines = new List<string> { "All group members must be on the board to activate bonuses." };
            if (effectLines != null)
            {
                foreach (var line in effectLines)
                    lines.Add(line);
            }

            return string.Join("\n", lines);
        }
    }
}
