using Microsoft.Xna.Framework.Graphics;
using Modding;
using System;
using System.Drawing;

namespace ResourcePacks
{
    public class TextureSet : IDisposable
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
    }
}
