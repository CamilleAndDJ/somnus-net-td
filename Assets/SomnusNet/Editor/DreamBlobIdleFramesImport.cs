using UnityEditor;
using UnityEngine;

namespace SomnusNet.Editor
{
    public static class DreamBlobIdleFramesImport
    {
        public const string FolderPath = "Assets/SomnusNet/Resources/DreamBlobIdleFrames";
        static readonly float PixelsPerUnit =
            200f / (SomnusNet.Core.GridManager.BlobSpriteWorldSize * 1.12f);
        static readonly Vector2 Pivot = new Vector2(0.5f, 0.32f);

        [MenuItem("SomnusNet/Reimport Dream Blob Idle Frames")]
        public static void ReimportAll()
        {
            if (!AssetDatabase.IsValidFolder(FolderPath))
                return;

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { FolderPath });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                ApplySettings(importer);
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }

        public static void ApplySettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMode = (int)SpriteImportMode.Single;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = Pivot;
            importer.SetTextureSettings(settings);
        }
    }

    class DreamBlobIdleFramesTexturePostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("/DreamBlobIdleFrames/"))
                return;

            if (!assetPath.EndsWith(".png"))
                return;

            DreamBlobIdleFramesImport.ApplySettings((TextureImporter)assetImporter);
        }
    }
}
