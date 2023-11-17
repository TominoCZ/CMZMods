﻿using Modding;
using System;
using System.Drawing;
using System.IO;

namespace ResourcePacks
{
    public class ResourcePack : IDisposable
    {
        public string Name { get; private set; }
        public TextureSet Terrain { get; private set; }

        public static ResourcePack Default { get; private set; }

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
                ModBase.Instance.Log($"Failed to load {dir}:\n{e}", LogType.Error);
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

            ModBase.Instance.Log($"Preloading pack: " + name);

            var now = DateTime.Now;

            TextureSet terrain;
            using (var diffuse = LoadTexture(dirTex, "diffuse"))
            using (var normal = LoadTexture(dirTex, "normal"))
            using (var metal = LoadTexture(dirTex, "metal"))
                terrain = TextureSet.Create(diffuse?.ToTexture(ModBase.Instance.Game.GraphicsDevice), normal?.ToTexture(ModBase.Instance.Game.GraphicsDevice), metal?.ToTexture(ModBase.Instance.Game.GraphicsDevice));

            pack = new ResourcePack(name, terrain);

            ModBase.Instance.Log($"Preloading pack took {(DateTime.Now - now).TotalSeconds} seconds", LogType.Success);

            return true;
        }

        public void Dispose()
        {
            if (Disposed || this == Default)
                return;

            Disposed = true;

            Terrain.Dispose();
        }
    }
}