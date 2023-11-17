using DNA.CastleMinerZ;
using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ResourcePacks
{
    public class PackManager
    {
        public static string Folder = "./resourcepacks";

        public Game Game;
        public BlockTerrain Terrain;

        public bool Loaded { get; private set; }

        public ResourcePack Active;
        public Dictionary<string, ResourcePack> Packs = new Dictionary<string, ResourcePack>();

        private FieldInfo TerrainDiffuse;
        private FieldInfo TerrainNormal;
        private FieldInfo TerrainMetal;
        private FieldInfo TerrainNormalMip;
        private FieldInfo TerrainDiffuseMip;

        public PackManager(CastleMinerZGame game)
        {
            Game = game;
            Terrain = game._terrain;

            Fetch();
        }

        public void Fetch()
        {
            Loaded = false;
            if (!Packs.ContainsKey("Default"))
            {
                TerrainDiffuse = Terrain.GetType().GetField("_diffuseAlpha");
                TerrainNormal = Terrain.GetType().GetField("_normalSpec");
                TerrainMetal = Terrain.GetType().GetField("_metalLight");
                TerrainNormalMip = Terrain.GetType().GetField("_mipMapNormals");
                TerrainDiffuseMip = Terrain.GetType().GetField("_mipMapDiffuse");

                Packs.Add("Default", new ResourcePack("Default", new TextureSet(
                    TerrainDiffuse.Get<Texture2D>(Terrain),
                    TerrainNormal.Get<Texture2D>(Terrain),
                    TerrainMetal.Get<Texture2D>(Terrain),
                    TerrainDiffuseMip.Get<Texture2D>(Terrain),
                    TerrainNormalMip.Get<Texture2D>(Terrain))));

                try
                {
                    var dir = Path.Combine(Folder, "Default/textures/terrain");

                    Directory.CreateDirectory(dir);

                    using (var img = ResourcePack.Default.Terrain.Diffuse.ToBitmap())
                        img.Save(Path.Combine(dir, $"diffuse.png"));
                    using (var img = ResourcePack.Default.Terrain.Normal.ToBitmap())
                        img.Save(Path.Combine(dir, $"normal.png"));
                    using (var img = ResourcePack.Default.Terrain.Metal.ToBitmap())
                        img.Save(Path.Combine(dir, $"metal.png"));
                }
                catch (Exception ex)
                {
                    ModBase.Instance.Log(ex.ToString(), LogType.Error);
                }

                Active = ResourcePack.Default;
            }

            if (Active != ResourcePack.Default)
                Reset();

            var keys = Packs.Keys.ToArray();
            foreach (var key in keys)
            {
                if (key == "Default")
                    continue;

                Packs[key].Dispose();
                Packs[key] = null;
            }
            var dirs = Directory.GetDirectories(Folder);
            foreach (var dir in dirs)
            {
                var key = Path.GetFileNameWithoutExtension(dir);
                if (key == "Default")
                    continue;

                if (ResourcePack.TryLoad(key, out var pack))
                    Packs.Add(key, pack);
            }
            Loaded = true;
        }

        public bool Set(string name)
        {
            if (!Loaded || !Packs.TryGetValue(name, out var pack) || pack.Disposed)
                return false;

            TerrainDiffuse.SetValue(Terrain, pack.Terrain.Diffuse);
            TerrainNormal.SetValue(Terrain, pack.Terrain.Normal);
            TerrainMetal.SetValue(Terrain, pack.Terrain.Metal);
            TerrainNormalMip.SetValue(Terrain, pack.Terrain.NormalMip);
            TerrainDiffuseMip.SetValue(Terrain, pack.Terrain.DiffuseMip);

            Terrain._effect.Parameters["DiffuseAlphaTexture"].SetValue(pack.Terrain.Diffuse);
            Terrain._effect.Parameters["NormalSpecTexture"].SetValue(pack.Terrain.Normal);
            Terrain._effect.Parameters["MetalLightTexture"].SetValue(pack.Terrain.Metal);
            Terrain._effect.Parameters["MipMapSpecularTexture"].SetValue(pack.Terrain.NormalMip);
            Terrain._effect.Parameters["MipMapDiffuseTexture"].SetValue(pack.Terrain.DiffuseMip);

            //Terrain.UseSimpleShader = name != "Default";

            ModBase.Instance.Log($"\"{name}\" Loaded", LogType.Success);

            return true;
        }

        public void Reset()
        {
            Set("Default");
        }
    }
}
