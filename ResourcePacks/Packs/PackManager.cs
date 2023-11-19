using DNA.CastleMinerZ;
using DNA.CastleMinerZ.Terrain;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ResourcePacks.Packs
{
    public class PackManager
    {
        public static string Folder = "./@resourcepacks";

        //private FieldInfo _diffuse;
        //private FieldInfo _normal;
        //private FieldInfo _metal;
        //private FieldInfo _normalMip;
        //private FieldInfo _diffuseMip;
        private Texture2D _diffuse;
        private Texture2D _normal;
        private Texture2D _metal;
        private Texture2D _normalMip;
        private Texture2D _diffuseMip;
        private BlockTerrain _terrain;

        public CastleMinerZGame Game;
        public ResourcePack Active;
        public Dictionary<string, ResourcePack> Packs = new Dictionary<string, ResourcePack>();

        public bool IsLoaded { get; private set; }

        public PackManager(CastleMinerZGame game)
        {
            Game = game;
        }

        public void Init()
        {
            _terrain = Game._terrain;

            _diffuse = _terrain.GetValue<Texture2D>("_diffuseAlpha");
            _normal = _terrain.GetValue<Texture2D>("_normalSpec");
            _metal = _terrain.GetValue<Texture2D>("_metalLight");
            _diffuseMip = _terrain.GetValue<Texture2D>("_mipMapDiffuse");
            _normalMip = _terrain.GetValue<Texture2D>("_mipMapNormals");

            Packs.Add("Default", new ResourcePack("Default", new TextureSet(_diffuse, _normal, _metal, _diffuseMip, _normalMip)));
            /*
            _diffuse.GetValue<Texture2D>(_terrain),
            _normal.GetValue<Texture2D>(_terrain),
            _metal.GetValue<Texture2D>(_terrain),
            _diffuseMip.GetValue<Texture2D>(_terrain),
            _normalMip.GetValue<Texture2D>(_terrain))));*/

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

            Fetch();
        }

        public void Fetch()
        {
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
            IsLoaded = true;
        }

        public bool Set(string name) => Packs.TryGetValue(name, out var pack) && Set(pack);

        public bool Set(ResourcePack pack)
        {
            if (!IsLoaded || pack.Disposed)
                return false;

            pack.Terrain.Diffuse.ApplyTo(_diffuse);
            pack.Terrain.Normal.ApplyTo(_normal);
            pack.Terrain.Metal.ApplyTo(_metal);
            pack.Terrain.DiffuseMip.ApplyTo(_diffuseMip);
            pack.Terrain.NormalMip.ApplyTo(_normalMip);

            /*
            _diffuse.SetValue(_terrain, pack.Terrain.Diffuse);
            _normal.SetValue(_terrain, pack.Terrain.Normal);
            _metal.SetValue(_terrain, pack.Terrain.Metal);
            _normalMip.SetValue(_terrain, pack.Terrain.NormalMip);
            _diffuseMip.SetValue(_terrain, pack.Terrain.DiffuseMip);*/
            
            _terrain._effect.Parameters["DiffuseAlphaTexture"].SetValue(_diffuse);
            _terrain._effect.Parameters["NormalSpecTexture"].SetValue(_normal);
            _terrain._effect.Parameters["MetalLightTexture"].SetValue(_metal);
            _terrain._effect.Parameters["MipMapDiffuseTexture"].SetValue(_diffuseMip);
            _terrain._effect.Parameters["MipMapSpecularTexture"].SetValue(_normalMip);

            //Terrain.UseSimpleShader = name != "Default";

            ModBase.Instance.Log($"\"{pack.Name}\" Loaded");

            return true;
        }

        public void Reset()
        {
            Set("Default");
        }
    }
}
