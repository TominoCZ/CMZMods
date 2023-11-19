using DNA.Drawing.Imaging.Photoshop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Modding;
using System;
using System.Drawing;
using Bitmap = System.Drawing.Bitmap;

namespace ResourcePacks
{
    public class TextureSet
    {
        public TextureWrap Diffuse;
        public TextureWrap Normal;
        public TextureWrap Metal;

        public TextureWrap DiffuseMip;
        public TextureWrap NormalMip;

        public static TextureSet Default { get; private set; }

        public TextureSet(Texture2D diff, Texture2D normal, Texture2D metal, Texture2D diffMip, Texture2D normalMip) : this(TextureWrap.FromTexture(diff), TextureWrap.FromTexture(normal), TextureWrap.FromTexture(metal), TextureWrap.FromTexture(diffMip), TextureWrap.FromTexture(normalMip))
        {

        }

        public TextureSet(TextureWrap diff, TextureWrap normal, TextureWrap metal, TextureWrap diffMip, TextureWrap normalMip)
        {
            Diffuse = diff;
            Normal = normal;
            Metal = metal;

            DiffuseMip = diffMip;
            NormalMip = normalMip;

            if (Default == null)
                Default = this;
        }

        public static TextureSet Create(Bitmap diffuse = null, Bitmap normal = null, Bitmap metal = null)
        {
            var Diffuse = TextureWrap.FromBitmap(diffuse) ?? Default.Diffuse;
            var Normal = TextureWrap.FromBitmap(normal) ?? Default.Normal;
            var Metal = TextureWrap.FromBitmap(metal) ?? Default.Metal;

            var DiffuseMip = Default.DiffuseMip;
            var NormalMip = Default.NormalMip;

            if (Normal != Default.Normal)
            {
                NormalMip = Normal.Clone();
                NormalMip.MakeMipmap(true);
            }
            if (Diffuse != Default.Diffuse)
            {
                DiffuseMip = Diffuse.Clone();
                DiffuseMip.MakeMipmap(false, true);
            }

            return new TextureSet(Diffuse, Normal, Metal, DiffuseMip, NormalMip);
        }
    }
    /*
    public class TextureSetOld : IDisposable
    {
        public Texture2D Diffuse;
        public Texture2D Normal;
        public Texture2D Metal;

        public Texture2D DiffuseMip;
        public Texture2D NormalMip;

        public static TextureSet Default;

        public TextureSet(Texture2D diff, Texture2D normal, Texture2D metal, Texture2D diffMip, Texture2D normalMip)
        {
            Diffuse = diff;
            Normal = normal;
            Metal = metal;

            DiffuseMip = diffMip;
            NormalMip = normalMip;

            if (Default == null)
                Default = this;
        }

        public static TextureSet Create(Texture2D d = null, Texture2D n = null, Texture2D m = null)
        {
            var Diffuse = d ?? Default.Diffuse;
            var Normal = n ?? Default.Normal;
            var Metal = m ?? Default.Metal;

            var DiffuseMip = Default.DiffuseMip;
            var NormalMip = Default.NormalMip;

            Bitmap normal = null;
            if (Normal != Default.Normal)
            {
                normal = Normal.ToBitmap();

                NormalMip = normal.CreateMipmap(ModBase.Instance.Game.GraphicsDevice);
            }
            Bitmap diffuse = null;
            if (Diffuse != Default.Diffuse)
            {
                //var wMin = Math.Min(1024, Default.Diffuse.Width / 2);
                //var hMin = Math.Min(1024, Default.Diffuse.Height / 2);

                using (var bmp = Diffuse.ToBitmap())
                using (var mask = Default.DiffuseMip.ToBitmap())
                using (var resized = bmp.GetResized(mask.Width, mask.Height))
                    diffuse = resized.ApplyMask(mask);

                DiffuseMip = diffuse.CreateMipmap(ModBase.Instance.Game.GraphicsDevice);
            }

            diffuse?.Dispose();
            normal?.Dispose();

            return new TextureSet(Diffuse, Normal, Metal, DiffuseMip, NormalMip);
        }

        public void Dispose()
        {
            Diffuse.Dispose();
            Normal.Dispose();
            Metal.Dispose();

            DiffuseMip.Dispose();
            NormalMip.Dispose();
        }
    }*/

}
