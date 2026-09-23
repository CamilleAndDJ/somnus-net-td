using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class StargazerRegistry
    {
        public const string DesignationLabel = "Stargazer";

        const string Designation = DesignationLabel;

        static DreamBlob _stargazer;

        public static DreamBlob Current => _stargazer;

        public static bool IsStargazer(DreamBlob blob) =>
            blob != null && blob == _stargazer;

        public static void AssignInitialStargazer(DreamBlob blob)
        {
            if (!StoryModeRules.Active || blob == null || !blob.IsAlive || blob.Kind != BlobKind.Gatekeeper)
                return;

            if (_stargazer != null)
                return;

            Assign(blob);
        }

        public static void NotifyDeleted(DreamBlob blob)
        {
            if (!StoryModeRules.Active || blob == null || _stargazer != blob)
                return;

            _stargazer = null;
            blob.ClearStoryDesignation();
            ReassignToLongestBasic();
        }

        static void ReassignToLongestBasic()
        {
            var grid = GridManager.Instance;
            if (grid == null)
                return;

            DreamBlob oldest = null;
            grid.ForEachBlob(candidate =>
            {
                if (candidate == null || !candidate.IsAlive || candidate.Kind != BlobKind.Gatekeeper)
                    return;

                if (oldest == null || candidate.PlacedAtUnscaledTime < oldest.PlacedAtUnscaledTime)
                    oldest = candidate;
            });

            if (oldest != null)
                Assign(oldest);
        }

        static void Assign(DreamBlob blob)
        {
            if (_stargazer != null && _stargazer != blob)
                _stargazer.ClearStoryDesignation();

            _stargazer = blob;
            blob.SetStoryDesignation(Designation);
        }
    }
}
