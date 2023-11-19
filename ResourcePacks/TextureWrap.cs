using DNA.Collections;
using DNA.Drawing.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Modding;
using ResourcePacks.Packs;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using static DNA.Drawing.Imaging.Photoshop.ResolutionInfo;
using Bitmap = System.Drawing.Bitmap;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ResourcePacks
{
    public class TextureWrap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Byte4[][] Data { get; set; }

        public TextureWrap(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void ApplyTo(Texture2D tex)
        {
            var rect = new Rectangle(0, 0, Width, Height);
            for (int level = 0; level < Data.Length; level++)
            {
                var data = Data[level];

                tex.GraphicsDevice.Textures[0] = null;
                tex.GraphicsDevice.Textures[1] = null;
                tex.GraphicsDevice.Textures[2] = null;
                tex.GraphicsDevice.Textures[3] = null;
                tex.GraphicsDevice.Textures[4] = null;
                tex.SetData(level, rect, data, 0, data.Length);

                rect.Width >>= 1;
                rect.Height >>= 1;
            }
        }

        public void SetData(params Byte4[][] levels)
        {
            for (int level = 1; level < levels.Length; level++)
            {
                var last = levels[level - 1];
                var data = levels[level];
                if (data.Length != last.Length / 4)
                    throw new NotSupportedException($"Invalid texture levels supplied. Level downscale factor: {last.Length / (double)data.Length}");
            }

            Data = levels;
        }

        public Byte4[] GetData(int level = 0)
        {
            if (level >= Data.Length)
                return null;

            return Data[level];
        }

        public void ApplyMask(Byte4[] mask)
        {
            if (Data[0].Length != mask.Length)
                return;

            for (int i = 0; i < mask.Length; i++)
            {
                Data[0][i].PackedValue = (Data[0][i].PackedValue & 0x00FFFFFF) | (mask[i].PackedValue & 0xFF000000);
            }
        }

        public void GenMipmaps(bool normalize = false)
        {
            if (!Width.IsPoT() || !Height.IsPoT())
                return;

            var data = Data[0];
            var rect = new Rectangle(0, 0, Width, Height);
            var levels = new List<Byte4[]>();
            while (data != null)
            {
                levels.Add(data);

                data = MakeMipmap(data, rect.Width, rect.Height, normalize);

                rect.Width >>= 1;
                rect.Height >>= 1;
            }
            SetData(levels.ToArray());
        }

        private static Byte4[] MakeMipmap(Byte4[] source, int width, int height, bool normalize, int axisTiles = 8)
        {
            if (width <= 1 || height <= 1)
                return null;

            var result = new Byte4[width * height / 4];
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

        public void MakeMipmap(bool normalize = false, bool applyMask = false)
        {
            if (!Width.IsPoT() || !Height.IsPoT())
                return;

            Data[0] = MakeMipmap(Data[0], Width, Height, normalize);

            Width >>= 1;
            Height >>= 1;

            if (applyMask)
                using (var bmp = TextureSet.Default.DiffuseMip.ToBitmap())
                using (var mask = bmp.GetResized(Width, Height))
                    ApplyMask(FromBitmap(bmp).GetData());

            GenMipmaps(normalize);
        }

        public Bitmap ToBitmap()
        {
            var data = Data[0];
            var raw = new byte[data.Length * 4];
            for (int i = 0; i < data.Length; i++)
            {
                uint packed = data[i].PackedValue;
                int index = i * 4;

                raw[index] = (byte)(packed & 0xFFu);
                raw[index + 1] = (byte)((packed >> 8) & 0xFFu);
                raw[index + 2] = (byte)((packed >> 16) & 0xFFu);
                raw[index + 3] = (byte)((packed >> 24) & 0xFFu);

                (raw[index], raw[index + 2]) = (raw[index + 2], raw[index]);
            }
            var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(raw, 0, bmpData.Scan0, raw.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public TextureWrap Clone()
        {
            var wrap = new TextureWrap(Width, Height);
            var levels = new List<Byte4[]>();
            for (int level = 0; level < Data.Length; level++)
            {
                var data = Data[level];
                var copy = new Byte4[data.Length];

                data.CopyTo(copy, 0);

                levels.Add(copy);
            }
            wrap.SetData(levels.ToArray());

            return wrap;
        }

        public static TextureWrap FromBitmap(Bitmap bmp)//, bool makeMipmap = false, bool normalize = false)
        {
            if (bmp == null)
                return null;

            var format = bmp.PixelFormat;
            if (format != PixelFormat.Format32bppArgb && format != PixelFormat.Format32bppRgb)
                throw new FormatException("Map has wrong format (" + bmp.PixelFormat.ToString() + ") must be 32 bit ARGB");

            var width = bmp.Width;
            var height = bmp.Height;
            var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var raw = new byte[bmpData.Stride * bmpData.Height];
            Marshal.Copy(bmpData.Scan0, raw, 0, raw.Length);
            bmp.UnlockBits(bmpData);

            for (int index = 0; index < raw.Length; index += 4)
            {
                if (format == PixelFormat.Format32bppRgb)
                    raw[index + 3] = byte.MaxValue;

                (raw[index], raw[index + 2]) = (raw[index + 2], raw[index]);
            }

            var data = new Byte4[width * height];
            data.AsUintArray(delegate (uint[] value)
            {
                Buffer.BlockCopy(raw, 0, value, 0, raw.Length);
            });

            var wrap = new TextureWrap(width, height);
            wrap.SetData(data);
            return wrap;
        }

        public static TextureWrap FromTexture(Texture2D tex)//, bool makeMipmap = false, bool normalize = false)
        {
            var wrap = new TextureWrap(tex.Width, tex.Height);
            var rect = new Rectangle(0, 0, tex.Width, tex.Height);
            var levels = new List<Byte4[]>();
            for (int level = 0; level < tex.LevelCount; level++)
            {
                var data = new Byte4[rect.Width * rect.Height];
                tex.GetData(level, rect, data, 0, data.Length);

                levels.Add(data);

                rect.Width >>= 1;
                rect.Height >>= 1;
            }
            wrap.SetData(levels.ToArray());
            return wrap;
        }
    }
}
