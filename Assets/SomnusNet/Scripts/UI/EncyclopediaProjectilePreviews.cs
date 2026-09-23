using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.UI
{
    public enum BlobProjectilePreviewKind
    {
        None,
        GatekeeperAttack,
        StarBombFlight,
        StarBombAttached,
        CrescentTrapFlight,
        CrescentTrapAttached,
        CreatorShot,
        TooSlowShot,
        PuppeteerShot,
        PuppetMindControl,
        FourOhFourStandard,
        FourOhFourSpores,
        TintedPulse
    }

    public static class EncyclopediaProjectilePreviews
    {
        public const float GatekeeperAttackFps = 12f;
        public const float StarBombFlightFps = 24f;
        public const float StarBombAttachedFps = 6f;
        public const float CrescentTrapFlightFps = 24f;
        public const float CrescentTrapAttachedFps = 3f;
        public const float CreatorShotFps = CreatorShotSprites.FramesPerSecond;
        public const float TooSlowShotFps = TooSlowShotSprites.FramesPerSecond;
        public const float PuppeteerShotFps = PuppeteerSprites.FramesPerSecond;
        public const float PuppetMindControlFps = PuppetMindControlSprites.FramesPerSecond;

        public static Sprite GetFrame(BlobProjectilePreviewKind kind, float elapsedSeconds, Color tint = default)
        {
            return kind switch
            {
                BlobProjectilePreviewKind.GatekeeperAttack =>
                    GatekeeperAttackSprites.GetFrame(elapsedSeconds, GatekeeperAttackFps),
                BlobProjectilePreviewKind.StarBombFlight =>
                    StarBombFlightSprites.GetFrame(elapsedSeconds, StarBombFlightFps),
                BlobProjectilePreviewKind.StarBombAttached =>
                    StarBombAttachedSprites.GetFrame(elapsedSeconds, StarBombAttachedFps, loop: true),
                BlobProjectilePreviewKind.CrescentTrapFlight =>
                    CrescentTrapFlightSprites.GetFrame(elapsedSeconds, CrescentTrapFlightFps),
                BlobProjectilePreviewKind.CrescentTrapAttached =>
                    CrescentTrapSprites.GetFrame(elapsedSeconds, CrescentTrapAttachedFps, loop: true),
                BlobProjectilePreviewKind.CreatorShot =>
                    CreatorShotSprites.GetFrame(elapsedSeconds, CreatorShotFps),
                BlobProjectilePreviewKind.TooSlowShot =>
                    TooSlowShotSprites.GetFrame(elapsedSeconds, TooSlowShotFps),
                BlobProjectilePreviewKind.PuppeteerShot =>
                    PuppeteerSprites.GetFrame(elapsedSeconds, PuppeteerShotFps),
                BlobProjectilePreviewKind.PuppetMindControl =>
                    PuppetMindControlSprites.GetFrame(elapsedSeconds, PuppetMindControlFps),
                BlobProjectilePreviewKind.FourOhFourStandard =>
                    FourOhFourShotSprites.StandardSprite,
                BlobProjectilePreviewKind.FourOhFourSpores =>
                    FourOhFourShotSprites.SporesSprite,
                BlobProjectilePreviewKind.TintedPulse =>
                    DreamBlob.FallbackProjectileSprite,
                _ => null
            };
        }

        public static float GetFramesPerSecond(BlobProjectilePreviewKind kind) => kind switch
        {
            BlobProjectilePreviewKind.GatekeeperAttack => GatekeeperAttackFps,
            BlobProjectilePreviewKind.StarBombFlight => StarBombFlightFps,
            BlobProjectilePreviewKind.StarBombAttached => StarBombAttachedFps,
            BlobProjectilePreviewKind.CrescentTrapFlight => CrescentTrapFlightFps,
            BlobProjectilePreviewKind.CrescentTrapAttached => CrescentTrapAttachedFps,
            BlobProjectilePreviewKind.CreatorShot => CreatorShotFps,
            BlobProjectilePreviewKind.TooSlowShot => TooSlowShotFps,
            BlobProjectilePreviewKind.PuppeteerShot => PuppeteerShotFps,
            BlobProjectilePreviewKind.PuppetMindControl => PuppetMindControlFps,
            _ => 0f
        };

        public static bool IsAnimated(BlobProjectilePreviewKind kind) => GetFramesPerSecond(kind) > 0f;

        public static Color GetTint(BlobProjectilePreviewKind kind, BlobKind blobKind) => kind switch
        {
            BlobProjectilePreviewKind.TintedPulse when blobKind == BlobKind.Blaze =>
                new Color(1f, 0.55f, 0.2f, 0.95f),
            BlobProjectilePreviewKind.TintedPulse when blobKind == BlobKind.Gatekeeper =>
                new Color(1f, 0.92f, 0.15f, 0.95f),
            _ => Color.white
        };
    }
}
