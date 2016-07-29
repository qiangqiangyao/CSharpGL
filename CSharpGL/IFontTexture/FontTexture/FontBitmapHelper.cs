﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace CSharpGL
{
    /// <summary>
    /// helper class.
    /// </summary>
    public static class FontBitmapHelper
    {
        /// <summary>
        /// Gets a <see cref="FontBitmap"/>'s intance.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="charSet"></param>
        /// <returns></returns>
        public static FontBitmap GetFontBitmap(this Font font, string charSet)
        {
            var fontBitmap = new FontBitmap();
            fontBitmap.GlyphFont = font;

            // 以下三步，不能调换先后顺序。
            // Don't change the order in which these 3 functions invoked.
            GetGlyphSizes(fontBitmap, charSet);
            int width, height;
            GetGlyphPositions(fontBitmap, charSet, out width, out height);
            PrintBitmap(fontBitmap, charSet, width, height);

            fontBitmap.GlyphBitmap.Save("TestFontBitmap.bmp");
            //var fontResource = new FontResource();
            //fontResource.FontHeight = pixelSize + yInterval;
            //fontResource.CharInfoDict = dict;
            //fontResource.InitTexture(finalBitmap);
            //finalBitmap.Dispose();
            //return fontResource;
            return fontBitmap;
        }

        private static void PrintBitmap(FontBitmap fontBitmap, string charSet, int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                foreach (KeyValuePair<char, GlyphInfo> item in fontBitmap.GlyphInfoDictionary)
                {
                    graphics.DrawString(item.Key.ToString(), fontBitmap.GlyphFont, Brushes.White, item.Value.xoffset, item.Value.yoffset);
                    // draw a tectangle that displays glyph's area.
                    //graphics.DrawRectangle(Pens.Red, item.Value.xoffset, item.Value.yoffset, item.Value.width, item.Value.height);
                }
            }

            fontBitmap.GlyphBitmap = bitmap;
        }

        private static void GetGlyphPositions(FontBitmap fontBitmap, string charSet, out int width, out int height)
        {
            int sideLength;
            float maxGlyphHeight = 0.0f;
            {
                float totalWidth = 0.0f;
                foreach (GlyphInfo item in fontBitmap.GlyphInfoDictionary.Values)
                {
                    totalWidth += item.width;
                    if (maxGlyphHeight < item.height) { maxGlyphHeight = item.height; }
                }
                float area = totalWidth * maxGlyphHeight;
                sideLength = (int)Math.Ceiling(Math.Sqrt(area));
                fontBitmap.GlyphHeight = maxGlyphHeight;
            }
            {
                float maxWidth = 0, maxHeight = 0;
                float currentX = 0, currentY = 0;
                foreach (var item in fontBitmap.GlyphInfoDictionary)
                {
                    if (currentX + item.Value.width < sideLength)
                    {
                        item.Value.xoffset = currentX;
                        item.Value.yoffset = currentY;
                        currentX += item.Value.width;
                    }
                    else
                    {
                        if (maxWidth < currentX) { maxWidth = currentX; }
                        currentX = 0;
                        currentY += maxGlyphHeight;
                        item.Value.xoffset = currentX;
                        item.Value.yoffset = currentY;
                        currentX += item.Value.width;
                    }
                }
                maxHeight = currentY + maxGlyphHeight;
                width = (int)Math.Ceiling(maxWidth);
                height = (int)Math.Ceiling(maxHeight);
            }
        }

        private static void GetGlyphSizes(FontBitmap fontBitmap, string charSet)
        {
            float fontSize = fontBitmap.GlyphFont.Size;

            using (var bitmap = new Bitmap(1, 1, PixelFormat.Format24bppRgb))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    foreach (char c in charSet)
                    {
                        SizeF size = graphics.MeasureString(c.ToString(), fontBitmap.GlyphFont);
                        // glyph's position is not settled yet.
                        var info = new GlyphInfo(0, 0, size.Width, size.Height);
                        fontBitmap.GlyphInfoDictionary.Add(c, info);
                    }
                }
            }
        }

    }
}