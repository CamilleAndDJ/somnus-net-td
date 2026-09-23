using System.Collections.Generic;
using SomnusNet.Data;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class CheerfulBestFriendSynergy
    {
        public static DreamBlob SelectBestFriend(DreamBlob cheerful)
        {
            if (cheerful == null || !cheerful.IsAlive || cheerful.Kind != BlobKind.Cheerful)
                return null;

            var claims = BuildBestFriendClaims();
            return claims.TryGetValue(cheerful, out var friend) ? friend : null;
        }

        public static bool HasAdjacentFriend(DreamBlob cheerful) => SelectBestFriend(cheerful) != null;

        public static bool HasAdjacentBlob(DreamBlob cheerful)
        {
            if (cheerful == null || !cheerful.IsAlive || cheerful.Kind != BlobKind.Cheerful ||
                GridManager.Instance == null)
                return false;

            var found = false;
            GridManager.Instance.ForEachAdjacentBlob(cheerful.Column, cheerful.Lane, blob =>
            {
                if (blob != null && blob.IsAlive && blob != cheerful)
                    found = true;
            });
            return found;
        }

        static Dictionary<DreamBlob, DreamBlob> BuildBestFriendClaims()
        {
            var claims = new Dictionary<DreamBlob, DreamBlob>();
            if (GridManager.Instance == null)
                return claims;

            var cheerfuls = new List<DreamBlob>();
            GridManager.Instance.ForEachBlob(blob =>
            {
                if (blob != null && blob.IsAlive && blob.Kind == BlobKind.Cheerful)
                    cheerfuls.Add(blob);
            });

            if (cheerfuls.Count == 0)
                return claims;

            cheerfuls.Sort((a, b) => a.PlacedAtUnscaledTime.CompareTo(b.PlacedAtUnscaledTime));

            var claimedFriends = new HashSet<DreamBlob>();
            foreach (var cheerful in cheerfuls)
            {
                var friend = PickFriendForCheerful(cheerful, claimedFriends);
                if (friend == null)
                    continue;

                claims[cheerful] = friend;
                claimedFriends.Add(friend);
            }

            return claims;
        }

        static DreamBlob PickFriendForCheerful(DreamBlob cheerful, HashSet<DreamBlob> claimedFriends)
        {
            if (GridManager.Instance == null)
                return null;

            var adjacent = new List<DreamBlob>();
            GridManager.Instance.ForEachAdjacentBlob(cheerful.Column, cheerful.Lane, blob =>
            {
                if (CanBeBestFriend(blob) && blob != cheerful && !claimedFriends.Contains(blob))
                    adjacent.Add(blob);
            });

            if (adjacent.Count == 0)
                return null;

            DreamBlob pick = null;
            var bestPlacedAt = float.MaxValue;
            foreach (var blob in adjacent)
            {
                if (blob.Kind != BlobKind.Dealer)
                    continue;

                if (blob.PlacedAtUnscaledTime < bestPlacedAt)
                {
                    bestPlacedAt = blob.PlacedAtUnscaledTime;
                    pick = blob;
                }
            }

            if (pick != null)
                return pick;

            bestPlacedAt = float.MaxValue;
            foreach (var blob in adjacent)
            {
                if (blob.PlacedAtUnscaledTime >= bestPlacedAt)
                    continue;

                bestPlacedAt = blob.PlacedAtUnscaledTime;
                pick = blob;
            }

            return pick;
        }

        public static DreamBlob SelectViscousBounceTarget(DreamBlob primaryFriend, DreamBlob cheerful)
        {
            if (primaryFriend == null || !primaryFriend.IsAlive || GridManager.Instance == null)
                return null;

            DreamBlob pick = null;
            var bestPlacedAt = float.MaxValue;
            GridManager.Instance.ForEachAdjacentBlob(primaryFriend.Column, primaryFriend.Lane, blob =>
            {
                if (!CanBeBestFriend(blob) || blob == primaryFriend)
                    return;

                if (blob.PlacedAtUnscaledTime >= bestPlacedAt)
                    return;

                bestPlacedAt = blob.PlacedAtUnscaledTime;
                pick = blob;
            });

            return pick;
        }

        static bool CanBeBestFriend(DreamBlob blob) =>
            blob != null && blob.IsAlive && blob.Kind != BlobKind.Cheerful;
    }
}
