using System.Collections.Generic;
using UnityEngine;

namespace SomnusNet.Units
{
    static class SpritesheetLoader
    {
        const int MinGapColumns = 2;
        const int MinFrameWidth = 16;

        public static Sprite[] LoadHorizontalStrip(string resourceName, int frameCount, float pixelsPerUnit,
            Vector2 pivot)
        {
            var sheet = Resources.Load<Texture2D>(resourceName);
            if (sheet == null || frameCount <= 0)
                return null;

            var working = PrepareSheet(sheet);
            if (working == null)
                return null;

            var frameWidth = working.width / (float)frameCount;
            var frameHeight = working.height;
            var frames = new Sprite[frameCount];
            for (var i = 0; i < frameCount; i++)
            {
                var rect = new Rect(i * frameWidth, 0f, frameWidth, frameHeight);
                frames[i] = Sprite.Create(working, rect, pivot, pixelsPerUnit);
            }

            return frames;
        }

        public static Sprite[] LoadHorizontalStrip(string resourceName, int frameCount, int frameWidth, int frameHeight,
            float pixelsPerUnit, Vector2 pivot)
        {
            var sheet = Resources.Load<Texture2D>(resourceName);
            if (sheet == null || frameCount <= 0)
                return null;

            var working = PrepareSheet(sheet);
            if (working == null)
                return null;

            var frames = new Sprite[frameCount];
            for (var i = 0; i < frameCount; i++)
            {
                var rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight);
                frames[i] = Sprite.Create(working, rect, pivot, pixelsPerUnit);
            }

            return frames;
        }

        public static Sprite[] LoadHorizontalStripFromRects(string resourceName, Rect[] frameRects,
            float pixelsPerUnit, Vector2 pivot)
        {
            if (frameRects == null || frameRects.Length == 0)
                return null;

            var sheet = Resources.Load<Texture2D>(resourceName);
            if (sheet == null)
                return null;

            var working = PrepareSheet(sheet);
            if (working == null)
                return null;

            var frames = new Sprite[frameRects.Length];
            for (var i = 0; i < frameRects.Length; i++)
                frames[i] = Sprite.Create(working, frameRects[i], pivot, pixelsPerUnit);
            return frames;
        }

        public static Sprite[] LoadHorizontalStripFromRects(string resourceName, Rect[] frameRects,
            Vector2[] pivots, float pixelsPerUnit)
        {
            if (frameRects == null || frameRects.Length == 0)
                return null;

            if (pivots == null || pivots.Length != frameRects.Length)
                return null;

            var sheet = Resources.Load<Texture2D>(resourceName);
            if (sheet == null)
                return null;

            var working = PrepareSheet(sheet);
            if (working == null)
                return null;

            var frames = new Sprite[frameRects.Length];
            for (var i = 0; i < frameRects.Length; i++)
                frames[i] = Sprite.Create(working, frameRects[i], pivots[i], pixelsPerUnit);
            return frames;
        }

        public static Sprite[] LoadHorizontalStripFromRectsFullRect(string resourceName, Rect[] frameRects,
            float pixelsPerUnit, Vector2 pivot)
        {
            if (frameRects == null || frameRects.Length == 0)
                return null;

            var sheet = Resources.Load<Texture2D>(resourceName);
            if (sheet == null)
                return null;

            var working = PrepareSheet(sheet);
            if (working == null)
                return null;

            var frames = new Sprite[frameRects.Length];
            for (var i = 0; i < frameRects.Length; i++)
            {
                frames[i] = Sprite.Create(
                    working,
                    frameRects[i],
                    pivot,
                    pixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect);
            }

            return frames;
        }

        /// <summary>
        /// Bakes equal-size frames onto separate canvases, pinning the core body to the pivot column.
        /// Side particles can animate without shifting the blob left/right.
        /// </summary>
        public static Sprite[] LoadHorizontalStripFromRectsPinnedBody(string resourceName, Rect[] frameRects,
            float pixelsPerUnit, Vector2 pivot)
        {
            if (frameRects == null || frameRects.Length == 0)
                return null;

            var sheet = Resources.Load<Texture2D>(resourceName);
            if (sheet == null)
                return null;

            var working = PrepareSheet(sheet);
            if (working == null)
                return null;

            var sheetWidth = working.width;
            var sheetHeight = working.height;
            var sheetPixels = working.GetPixels();

            var canvasWidth = 0;
            var canvasHeight = 0;
            for (var i = 0; i < frameRects.Length; i++)
            {
                canvasWidth = Mathf.Max(canvasWidth, Mathf.CeilToInt(frameRects[i].width));
                canvasHeight = Mathf.Max(canvasHeight, Mathf.CeilToInt(frameRects[i].height));
            }

            if (canvasWidth <= 0 || canvasHeight <= 0)
                return null;

            var coreBodyLocalXs = new float[frameRects.Length];
            var measuredCount = 0;
            for (var i = 0; i < frameRects.Length; i++)
            {
                if (!TryGetCoreBodyLocalX(working, frameRects[i], out var measuredCoreBodyLocalX))
                    continue;

                coreBodyLocalXs[measuredCount++] = measuredCoreBodyLocalX;
            }

            var referenceCoreBodyX = canvasWidth * pivot.x;
            if (measuredCount > 0)
            {
                System.Array.Sort(coreBodyLocalXs, 0, measuredCount);
                referenceCoreBodyX = coreBodyLocalXs[measuredCount / 2];
            }

            var pinnedPivot = new Vector2(referenceCoreBodyX / canvasWidth, pivot.y);
            var transparent = new Color(0f, 0f, 0f, 0f);
            var destXs = new int[frameRects.Length];
            var minDestX = 0;
            var maxDestX = 0;
            for (var i = 0; i < frameRects.Length; i++)
            {
                var frameRect = frameRects[i];
                var coreBodyLocalX = referenceCoreBodyX;
                TryGetCoreBodyLocalX(working, frameRect, out coreBodyLocalX);

                var destX = Mathf.RoundToInt(referenceCoreBodyX - coreBodyLocalX);
                destXs[i] = destX;
                minDestX = Mathf.Min(minDestX, destX);
                maxDestX = Mathf.Max(maxDestX, destX + Mathf.CeilToInt(frameRect.width) - 1);
            }

            var leftPad = minDestX < 0 ? -minDestX : 0;
            var bakedWidth = Mathf.Max(canvasWidth, maxDestX + 1 + leftPad);
            referenceCoreBodyX += leftPad;
            pinnedPivot = new Vector2(referenceCoreBodyX / bakedWidth, pivot.y);
            var frames = new Sprite[frameRects.Length];

            for (var i = 0; i < frameRects.Length; i++)
            {
                var frameRect = frameRects[i];
                var destX = destXs[i] + leftPad;
                var canvas = new Texture2D(bakedWidth, canvasHeight, TextureFormat.RGBA32, false);
                canvas.filterMode = FilterMode.Point;

                var canvasPixels = new Color[bakedWidth * canvasHeight];
                for (var p = 0; p < canvasPixels.Length; p++)
                    canvasPixels[p] = transparent;

                var sourceX = Mathf.RoundToInt(frameRect.xMin);
                var sourceY = Mathf.RoundToInt(frameRect.yMin);
                var sourceWidth = Mathf.RoundToInt(frameRect.width);
                var sourceHeight = Mathf.RoundToInt(frameRect.height);

                for (var localY = 0; localY < sourceHeight; localY++)
                {
                    var sy = Mathf.Clamp(sourceY + localY, 0, sheetHeight - 1);
                    for (var localX = 0; localX < sourceWidth; localX++)
                    {
                        var sx = Mathf.Clamp(sourceX + localX, 0, sheetWidth - 1);
                        var pixel = sheetPixels[sy * sheetWidth + sx];
                        if (pixel.a <= 0.05f)
                            continue;

                        var dx = destX + localX;
                        var dy = localY;
                        if (dx < 0 || dx >= bakedWidth || dy < 0 || dy >= canvasHeight)
                            continue;

                        canvasPixels[dy * bakedWidth + dx] = pixel;
                    }
                }

                canvas.SetPixels(canvasPixels);
                canvas.Apply();
                frames[i] = Sprite.Create(
                    canvas,
                    new Rect(0f, 0f, bakedWidth, canvasHeight),
                    pinnedPivot,
                    pixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect);
            }

            return frames;
        }

        static bool TryGetCoreBodyLocalX(Texture2D sheet, Rect frameRect, out float coreBodyLocalX)
        {
            coreBodyLocalX = frameRect.width * 0.5f;
            if (!TryGetContentBoundsInRect(sheet, GetCoreBodyProbe(frameRect), out var coreBounds))
                return false;

            coreBodyLocalX = (coreBounds.xMin - frameRect.xMin) + coreBounds.width * 0.5f;
            return true;
        }

        static Rect GetCoreBodyProbe(Rect frameRect)
        {
            // Lower torso only — ignore hood and orbiting particles when pinning X.
            var probeWidth = frameRect.width * 0.32f;
            var probeHeight = frameRect.height * 0.38f;
            return new Rect(
                frameRect.xMin + (frameRect.width - probeWidth) * 0.5f,
                frameRect.yMin,
                probeWidth,
                probeHeight);
        }

        /// <summary>
        /// Bakes every frame onto one fixed canvas. Each slice is centered on the same canvas,
        /// so side particles cannot change placement frame-to-frame.
        /// </summary>
        public static Sprite[] LoadHorizontalStripFromRectsFixedCanvas(string resourceName, Rect[] frameRects,
            float pixelsPerUnit, Vector2 pivot)
        {
            if (frameRects == null || frameRects.Length == 0)
                return null;

            var sheet = Resources.Load<Texture2D>(resourceName);
            if (sheet == null)
                return null;

            var working = PrepareSheet(sheet);
            if (working == null)
                return null;

            var sheetWidth = working.width;
            var sheetHeight = working.height;
            var sheetPixels = working.GetPixels();
            var frameCount = frameRects.Length;
            var uniformRects = ExpandToUniformFrameCells(working, frameRects);
            var canvasHeight = 0;
            var canvasWidth = 0;

            for (var i = 0; i < frameCount; i++)
            {
                canvasHeight = Mathf.Max(canvasHeight, Mathf.CeilToInt(uniformRects[i].height));
                canvasWidth = Mathf.Max(canvasWidth, Mathf.CeilToInt(uniformRects[i].width));
            }

            if (canvasHeight <= 0 || canvasWidth <= 0)
                return null;

            var transparent = new Color(0f, 0f, 0f, 0f);
            var frames = new Sprite[frameCount];

            for (var i = 0; i < frameCount; i++)
            {
                var frameRect = uniformRects[i];
                var canvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
                canvas.filterMode = FilterMode.Point;

                var canvasPixels = new Color[canvasWidth * canvasHeight];
                for (var p = 0; p < canvasPixels.Length; p++)
                    canvasPixels[p] = transparent;

                var sourceX = Mathf.RoundToInt(frameRect.xMin);
                var sourceY = Mathf.RoundToInt(frameRect.yMin);
                var sourceWidth = Mathf.RoundToInt(frameRect.width);
                var sourceHeight = Mathf.RoundToInt(frameRect.height);

                for (var localY = 0; localY < sourceHeight; localY++)
                {
                    var sy = Mathf.Clamp(sourceY + localY, 0, sheetHeight - 1);
                    for (var localX = 0; localX < sourceWidth; localX++)
                    {
                        var sx = Mathf.Clamp(sourceX + localX, 0, sheetWidth - 1);
                        var pixel = sheetPixels[sy * sheetWidth + sx];
                        if (pixel.a <= 0.05f)
                            continue;

                        var dx = localX;
                        var dy = localY;
                        if (dx < 0 || dx >= canvasWidth || dy < 0 || dy >= canvasHeight)
                            continue;

                        canvasPixels[dy * canvasWidth + dx] = pixel;
                    }
                }

                canvas.SetPixels(canvasPixels);
                canvas.Apply();
                frames[i] = Sprite.Create(
                    canvas,
                    new Rect(0f, 0f, canvasWidth, canvasHeight),
                    pivot,
                    pixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect);
            }

            return frames;
        }

        static Rect[] ExpandToUniformFrameCells(Texture2D working, Rect[] frameRects)
        {
            var maxWidth = 0f;
            for (var i = 0; i < frameRects.Length; i++)
                maxWidth = Mathf.Max(maxWidth, frameRects[i].width);

            var uniformRects = new Rect[frameRects.Length];
            for (var i = 0; i < frameRects.Length; i++)
            {
                var frameRect = frameRects[i];
                var centerX = frameRect.xMin + frameRect.width * 0.5f;
                if (TryGetContentBoundsInRect(working, GetCoreBodyProbe(frameRect), out var coreBounds))
                    centerX = coreBounds.xMin + coreBounds.width * 0.5f;

                uniformRects[i] = new Rect(
                    centerX - maxWidth * 0.5f,
                    frameRect.yMin,
                    maxWidth,
                    frameRect.height);
            }

            return uniformRects;
        }

        public static Sprite[] LoadHorizontalStripByContentBounds(string resourceName, float pixelsPerUnit,
            Vector2 pivot)
        {
            var sheet = Resources.Load<Texture2D>(resourceName);
            if (sheet == null)
                return null;

            var working = PrepareSheet(sheet);
            if (working == null)
                return null;

            var rects = FindHorizontalFrameRects(working);
            if (rects.Count == 0)
                return null;

            var frames = new Sprite[rects.Count];
            for (var i = 0; i < rects.Count; i++)
                frames[i] = Sprite.Create(working, rects[i], pivot, pixelsPerUnit);
            return frames;
        }

        public static Sprite LoadSprite(string resourceName, float pixelsPerUnit, Vector2 pivot)
        {
            var sheet = Resources.Load<Texture2D>(resourceName);
            if (sheet == null)
                return null;

            var working = PrepareSheet(sheet);
            if (working == null)
                return null;

            return Sprite.Create(working, new Rect(0, 0, working.width, working.height), pivot, pixelsPerUnit);
        }

        public static Sprite LoadSpriteTrimmed(string resourceName, float targetWorldHeight, Vector2 pivot)
        {
            var sheet = Resources.Load<Texture2D>(resourceName);
            if (sheet == null)
                return null;

            var working = PrepareSheet(sheet);
            if (working == null)
                return null;

            if (!TryGetContentBounds(working, out var bounds))
                return Sprite.Create(working, new Rect(0, 0, working.width, working.height), pivot,
                    working.height / targetWorldHeight);

            var pixelsPerUnit = bounds.height / targetWorldHeight;
            return Sprite.Create(working, bounds, pivot, pixelsPerUnit);
        }

        static Texture2D PrepareSheet(Texture2D source)
        {
            var working = CloneTexture(source);
            if (working == null)
                return null;

            MakeBlackTransparent(working);
            return working;
        }

        static List<Rect> FindHorizontalFrameRects(Texture2D sheet)
        {
            var width = sheet.width;
            var height = sheet.height;
            var pixels = sheet.GetPixels();
            var rects = new List<Rect>();

            var frameStart = -1;
            var emptyColumns = 0;

            for (var x = 0; x < width; x++)
            {
                if (ColumnHasContent(pixels, width, height, x))
                {
                    emptyColumns = 0;
                    if (frameStart < 0)
                        frameStart = x;
                    continue;
                }

                if (frameStart < 0)
                    continue;

                emptyColumns++;
                if (emptyColumns < MinGapColumns)
                    continue;

                var frameWidth = x - emptyColumns - frameStart + 1;
                if (frameWidth >= MinFrameWidth)
                    rects.Add(new Rect(frameStart, 0, frameWidth, height));

                frameStart = -1;
                emptyColumns = 0;
            }

            if (frameStart >= 0)
            {
                var frameWidth = width - frameStart;
                if (frameWidth >= MinFrameWidth)
                    rects.Add(new Rect(frameStart, 0, frameWidth, height));
            }

            return rects;
        }

        static bool TryGetContentBoundsInRect(Texture2D sheet, Rect frameRect, out Rect bounds)
        {
            var width = sheet.width;
            var height = sheet.height;
            var pixels = sheet.GetPixels();

            var minX = width;
            var minY = height;
            var maxX = -1;
            var maxY = -1;

            var startX = Mathf.Clamp(Mathf.FloorToInt(frameRect.xMin), 0, width - 1);
            var endX = Mathf.Clamp(Mathf.CeilToInt(frameRect.xMax) - 1, 0, width - 1);
            var startY = Mathf.Clamp(Mathf.FloorToInt(frameRect.yMin), 0, height - 1);
            var endY = Mathf.Clamp(Mathf.CeilToInt(frameRect.yMax) - 1, 0, height - 1);

            for (var y = startY; y <= endY; y++)
            {
                for (var x = startX; x <= endX; x++)
                {
                    if (pixels[y * width + x].a <= 0.05f)
                        continue;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
            {
                bounds = default;
                return false;
            }

            bounds = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
            return true;
        }

        static bool TryGetContentBounds(Texture2D sheet, out Rect bounds)
        {
            var width = sheet.width;
            var height = sheet.height;
            var pixels = sheet.GetPixels();

            var minX = width;
            var minY = height;
            var maxX = -1;
            var maxY = -1;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (pixels[y * width + x].a <= 0.05f)
                        continue;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
            {
                bounds = default;
                return false;
            }

            bounds = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
            return true;
        }

        static bool ColumnHasContent(Color[] pixels, int width, int height, int x)
        {
            for (var y = 0; y < height; y++)
            {
                if (pixels[y * width + x].a > 0.05f)
                    return true;
            }

            return false;
        }

        static Texture2D CloneTexture(Texture2D source)
        {
            if (source == null)
                return null;

            if (source.isReadable)
            {
                var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                copy.filterMode = FilterMode.Point;
                copy.SetPixels(source.GetPixels());
                copy.Apply();
                return copy;
            }

            // Fallback when Read/Write is off — still allow custom sheets to load.
            var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            var prev = RenderTexture.active;
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;
            var blitted = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            blitted.filterMode = FilterMode.Point;
            blitted.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            blitted.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return blitted;
        }

        static void MakeBlackTransparent(Texture2D sheet)
        {
            if (!sheet.isReadable)
                return;

            var pixels = sheet.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                if (pixel.r > 0.05f || pixel.g > 0.05f || pixel.b > 0.05f)
                    continue;
                pixel.a = 0f;
                pixels[i] = pixel;
            }

            sheet.SetPixels(pixels);
            sheet.Apply();
        }
    }
}
