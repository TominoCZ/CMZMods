using DNA.Collections;
using FastBitmapLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using Bitmap = System.Drawing.Bitmap;
using Graphics = System.Drawing.Graphics;

namespace ResourcePacks
{
    public static class Extensions
    {
        public static T Get<T>(this FieldInfo field, object obj)
        {
            return (T)field.GetValue(obj);
        }

        public static Color ToXNA(this System.Drawing.Color c)
        {
            return new Color(c.R, c.G, c.B, c.A);
        }

        public static System.Drawing.Color ToSystem(this Color c)
        {
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static bool IsPowerOfTwo(this int x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        private static void SwapChannels(this byte[] data)
        {
            for (int index = 0; index < data.Length; index += 4)
            {
                byte num = data[index];
                data[index] = data[index + 2];
                data[index + 2] = num;
            }
        }

        private static Byte4[] MakeMipmap(this Byte4[] source, int width, int height, bool normalize, int axisTiles = 8)
        {
            if (width <= 1 || height <= 1)
                return null;

            Byte4[] result = new Byte4[width * height / 4];
            var offsets = new[] { 0, 1, width, width + 1 };
            var sizeTile = width / (double)axisTiles;
            var sizeRow = sizeTile * sizeTile * axisTiles;
            var indexOut = 0;

            try
            {
                for (int y = 0; y < height; y += 2)
                {
                    for (int x = 0; x < width; x += 2)
                    {
                        var indexIn = y * width + x;
                        var cellX = (int)(indexIn % width / sizeTile);
                        var cellY = (int)(indexIn / sizeRow);

                        var sum = Vector4.Zero;
                        var samples = 0;

                        for (int i = 0; i < offsets.Length; i++)
                        {
                            var index = indexIn + offsets[i];
                            var cx = (int)(index % width / sizeTile);
                            var cy = (int)(index / sizeRow);
                            if (index < 0 || index >= source.Length)
                                continue;
                            if (cx != cellX || cy != cellY)
                                continue;
                            if (normalize && source[index].ToVector4().W <= 5)
                                continue;
                            sum += source[index].ToVector4();

                            samples++;
                        }

                        if (normalize)
                        {
                            float alpha = sum.W / 4;
                            sum.W = 0;
                            if (sum.Length() > 0.001)
                                sum.Normalize();
                            sum *= 255;
                            sum.W = alpha;
                        }
                        else
                        {
                            sum /= Math.Max(1, samples);
                        }

                        result[indexOut] = new Byte4(sum);

                        indexOut++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            return result;
        }

        public static Color[] ToColors(this Bitmap bmp)
        {
            var w = bmp.Width;
            var h = bmp.Height;
            var data = new Color[w * h];
            using (var img = new FastBitmap(bmp))
            {
                img.Lock();
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        var index = y * w + x;

                        data[index] = img.GetPixel(x, y).ToXNA();
                    }
                }
            }
            return data;
        }

        public static Bitmap ToBitmap(this Texture2D tex, int level = 0)
        {
            var div = (int)Math.Pow(2, level);
            var w = tex.Width / div;
            var h = tex.Height / div;

            var colors = new Color[w * h];
            tex.GetData(level, new Rectangle(0, 0, w, h), colors, 0, colors.Length);
            return colors.ToBitmap(w, h);
        }

        public static Bitmap ToBitmap(this Color[] data, int w, int h)
        {
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (var img = new FastBitmap(bmp))
            {
                img.Lock();
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        var index = y * w + x;
                        var c = data[index];

                        img.SetPixel(x, y, c.ToSystem());
                    }
                }
            }
            return bmp;
        }

        public static unsafe Texture2D ToTexture(this Bitmap bmp, GraphicsDevice gd)
        {
            var data = new int[bmp.Width * bmp.Height];
            var tex = new Texture2D(gd, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            uint* scan0 = (uint*)(void*)bmpData.Scan0;
            for (int index = 0; index < data.Length; ++index)
                scan0[index] = (uint)(((int)scan0[index] & (int)byte.MaxValue) << 16 | (int)scan0[index] & 65280 | (int)((scan0[index] & 16711680U) >> 16) | (int)scan0[index] & -16777216);
            Marshal.Copy(bmpData.Scan0, data, 0, bmp.Width * bmp.Height);
            bmp.UnlockBits(bmpData);
            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateMipmap(this Bitmap bitmap, GraphicsDevice gd, bool normalize = false)
        {
            var format = bitmap.PixelFormat;
            if (format != PixelFormat.Format32bppArgb && format != PixelFormat.Format32bppRgb)
                throw new FormatException("Map has wrong format (" + bitmap.PixelFormat.ToString() + ") must be 32 bit ARGB");

            var width = bitmap.Width;
            var height = bitmap.Height;
            var bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var raw = new byte[bmpData.Stride * bmpData.Height];
            Marshal.Copy(bmpData.Scan0, raw, 0, raw.Length);
            bitmap.UnlockBits(bmpData);
            if (format == PixelFormat.Format32bppRgb)
            {
                for (int i = 3; i < raw.Length; i += 4)
                {
                    raw[i] = byte.MaxValue;
                }
            }

            SwapChannels(raw);
            var mipMap = IsPowerOfTwo(width) && IsPowerOfTwo(height);
            var result = new Texture2D(gd, width, height, mipMap, SurfaceFormat.Color);
            if (mipMap)
            {
                var data = new Byte4[width * height];
                data.AsUintArray(delegate (uint[] value)
                {
                    Buffer.BlockCopy(raw, 0, value, 0, raw.Length);
                });

                int level = 0;
                int w = width;
                int h = height;
                while (data != null)
                {
                    try
                    {
                        result.SetData(level, new Rectangle(0, 0, w, h), data, 0, data.Length);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    data = data.MakeMipmap(w, h, normalize);

                    w >>= 1;
                    h >>= 1;
                    level++;
                }
            }
            else
            {
                raw.AsByte4Array(delegate (Byte4[] value)
                {
                    result.SetData(value);
                });
            }

            return result;
        }

        public static Bitmap GetResized(this System.Drawing.Image image, int width, int height)
        {
            if (image.Width == width && image.Height == height)
                return (Bitmap)image.Clone();

            var rect = new System.Drawing.Rectangle(0, 0, width, height);
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;

                using (var attr = new ImageAttributes())
                {
                    attr.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, rect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, attr);
                }
            }
            return bitmap;
        }

        public static Bitmap ApplyMask(this Bitmap bmpColor, Bitmap bmpMask)
        {
            var sizeColor = bmpColor.Size;
            var sizeMask = bmpMask.Size;
            if (sizeColor != sizeMask)
                return null;
            var result = new Bitmap(sizeColor.Width, sizeColor.Height, PixelFormat.Format32bppArgb);
            using (var fbColor = new FastBitmap(bmpColor))
            using (var fbMask = new FastBitmap(bmpMask))
            using (var fb = new FastBitmap(result))
            {
                fbColor.Lock();
                fbMask.Lock();
                fb.Lock();
                for (int y = 0; y < sizeColor.Height; ++y)
                {
                    for (int x = 0; x < sizeColor.Width; ++x)
                    {
                        var color = fbColor.GetPixel(x, y);
                        var mask = fbMask.GetPixel(x, y);

                        fb.SetPixel(x, y, System.Drawing.Color.FromArgb(mask.A, color.R, color.G, color.B));
                    }
                }
            }
            return result;
        }
    }
}
