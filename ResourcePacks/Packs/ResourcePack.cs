using Microsoft.Xna.Framework.Graphics;
using Modding;
using System;
using System.Drawing;
using System.IO;

namespace ResourcePacks.Packs
{
    public class ResourcePack : IDisposable
    {
        public static ResourcePack Default { get; private set; }

        public string Name { get; private set; }

        public string Author { get; private set; } = "Unknown";

        public string Description { get; private set; } = "N/A";

        public TextureSet Terrain { get; private set; }

        public bool Disposed { get; private set; }

        public ResourcePack(string name, TextureSet terrain)
        {
            Name = name;
            Terrain = terrain;

            if (name == "Default" && Default == null)
                Default = this;
        }

        private static Bitmap LoadTexture(string dir, string name)
        {
            dir = Path.Combine(dir, name + ".png");
            if (!File.Exists(dir))
                return null;

            try
            {
                return (Bitmap)System.Drawing.Image.FromFile(dir); ;
            }
            catch (Exception e)
            {
                PackMod.Instance.Log($"Failed to load {dir}:\n{e}", LogType.Error);
            }

            return null;
        }

        public static bool TryLoad(string name, out ResourcePack pack)
        {
            pack = null;

            var dir = Path.Combine(PackManager.Folder, name);
            var dirTex = Path.Combine(dir, "textures/terrain");
            if (!Directory.Exists(dir) || !Directory.Exists(dirTex))
                return false;

            PackMod.Instance.Log($"Preloading pack: " + name);

            var now = DateTime.Now;

            TextureSet terrain;
            using (var diffuse = LoadTexture(dirTex, "diffuse"))
            using (var normal = LoadTexture(dirTex, "normal"))
            using (var metal = LoadTexture(dirTex, "metal"))
                terrain = TextureSet.Create(diffuse, normal, metal);//diffuse?.ToTexture(ModBase.Instance.Game.GraphicsDevice), normal?.ToTexture(ModBase.Instance.Game.GraphicsDevice), metal?.ToTexture(ModBase.Instance.Game.GraphicsDevice));

            pack = new ResourcePack(name, terrain);

            PackMod.Instance.Log($"Preloading pack took {(DateTime.Now - now).TotalSeconds} seconds", LogType.Success);

            return true;
        }

        public void Dispose()
        {
            if (Disposed || this == Default)
                return;

            Disposed = true;
        }
    }
}
