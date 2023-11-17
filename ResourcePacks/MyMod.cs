using DNA.CastleMinerZ;
using FastBitmapLib;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static HarmonyLib.AccessTools;
using static System.Net.Mime.MediaTypeNames;
using Color = Microsoft.Xna.Framework.Color;

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

                NormalMip = normal.CreateMipmap(ResourceManager.Instance.Game.GraphicsDevice);
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

                DiffuseMip = diffuse.CreateMipmap(ResourceManager.Instance.Game.GraphicsDevice);
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
                Console.WriteLine($"[ResourcePacks] Failed to load {dir}:\n{e}");
            }

            return null;
        }

        public static bool TryLoad(string name, out ResourcePack pack)
        {
            pack = null;

            var dir = Path.Combine(ResourceManager.Folder, name);
            var dirTex = Path.Combine(dir, "textures/terrain");
            if (!Directory.Exists(dir) || !Directory.Exists(dirTex))
                return false;

            Console.WriteLine("[ResourcePacks] Preloading pack: " + name);

            var now = DateTime.Now;

            TextureSet terrain;
            using (var diffuse = LoadTexture(dirTex, "diffuse"))
            using (var normal = LoadTexture(dirTex, "normal"))
            using (var metal = LoadTexture(dirTex, "metal"))
                terrain = TextureSet.Create(diffuse?.ToTexture(ResourceManager.Instance.Game.GraphicsDevice), normal?.ToTexture(ResourceManager.Instance.Game.GraphicsDevice), metal?.ToTexture(ResourceManager.Instance.Game.GraphicsDevice));

            pack = new ResourcePack(name, terrain);

            Console.WriteLine($"[ResourcePacks] Preloading pack took {(DateTime.Now - now).TotalSeconds} seconds");

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

    public class ResourceManager
    {
        public static string Folder = "./resourcepacks";

        public Game Game;
        public dynamic Terrain;

        public bool Loaded { get; private set; }

        public ResourcePack Active;
        public Dictionary<string, ResourcePack> Packs = new Dictionary<string, ResourcePack>();

        public static ResourceManager Instance;

        private FieldInfo TerrainDiffuse;
        private FieldInfo TerrainNormal;
        private FieldInfo TerrainMetal;
        private FieldInfo TerrainNormalMip;
        private FieldInfo TerrainDiffuseMip;

        public ResourceManager(Game game)
        {
            Instance = this;
            Game = game;
            Terrain = ((dynamic)game)._terrain;

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
                    TerrainDiffuse.GetValue(Terrain),
                    TerrainNormal.GetValue(Terrain),
                    TerrainMetal.GetValue(Terrain),
                    TerrainDiffuseMip.GetValue(Terrain),
                    TerrainNormalMip.GetValue(Terrain))));

                var dir = Path.Combine(Folder, "Default/textures/terrain");

                using (var img = ResourcePack.Default.Terrain.Diffuse.ToBitmap())
                    img.Save(Path.Combine(dir, $"diffuse.png"));
                using (var img = ResourcePack.Default.Terrain.Normal.ToBitmap())
                    img.Save(Path.Combine(dir, $"normal.png"));
                using (var img = ResourcePack.Default.Terrain.Metal.ToBitmap())
                    img.Save(Path.Combine(dir, $"metal.png"));

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
                {
                    Packs.Add(key, pack);

                    // TODO: Test how exactly my loaded data of the same image as default differs 

                    for (int level = pack.Terrain.DiffuseMip.LevelCount - 1; level >= 0; level--)
                    {
                        using (var bmp = pack.Terrain.DiffuseMip.ToBitmap(level))
                        {
                            bmp.Save($"diff_mip_{level}.png");
                        }
                    }
                }
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

            Terrain._effect.Parameters["DiffuseAlphaTexture"].SetValue((Texture)pack.Terrain.Diffuse);
            Terrain._effect.Parameters["NormalSpecTexture"].SetValue((Texture)pack.Terrain.Normal);
            Terrain._effect.Parameters["MetalLightTexture"].SetValue((Texture)pack.Terrain.Metal);
            Terrain._effect.Parameters["MipMapSpecularTexture"].SetValue((Texture)pack.Terrain.NormalMip);
            Terrain._effect.Parameters["MipMapDiffuseTexture"].SetValue((Texture)pack.Terrain.DiffuseMip);

            //Terrain.UseSimpleShader = name != "Default";

            Console.WriteLine($"[ResourcePacks] \"{name}\" Loaded");

            return true;
        }

        public void Reset()
        {
            Set("Default");
        }
    }

    [HarmonyPatch]
    public class GamePatch
    {
        public static ResourceManager Manager { get; private set; }

        static bool keyDown = false;
        static Queue<string> packsQueue = new Queue<string>();

        [HarmonyPrefix, HarmonyPatch("DNA.CastleMinerZ.CastleMinerZGame", "SecondaryLoad")]
        static bool LoadGamePre(Game __instance)
        {
            Console.WriteLine("LoadPre()");

            return true;
        }

        [HarmonyFinalizer, HarmonyPatch(typeof(FrontEndScreen), MethodType.Constructor, [typeof(CastleMinerZGame)])]
        static void OnLoaded(FrontEndScreen __instance)
        {
            Console.WriteLine("Load()");

            Manager = new ResourceManager(__instance._game);
            //Manager.Set("Legacy");

            foreach (var pack in Manager.Packs.Keys)
            {
                packsQueue.Enqueue(pack);
            }
        }

        [HarmonyPostfix, HarmonyPatch("DNA.CastleMinerZ.CastleMinerZGame", "SecondaryLoad")]
        static void LoadGamePost(Game __instance)
        {
            Console.WriteLine("LoadPost()");
        }
        /*
        [HarmonyFinalizer, HarmonyPatch(typeof(OptionsScreen), MethodType.Constructor, [typeof(bool), typeof(ScreenGroup)])]
        static void Ctor(OptionsScreen __instance)
        {
            PackInfo.Init(__instance);
        }*/

        [HarmonyPrefix, HarmonyPatch("Microsoft.Xna.Framework.Game", "Run")]
        static bool Run(Game __instance)
        {
            Console.WriteLine("Run()");

            return true;
        }

        [HarmonyPrefix, HarmonyPatch("Microsoft.Xna.Framework.Game", "Draw")]
        static bool Draw(Game __instance, GameTime ___gameTime)
        {
            return true;
        }

        [HarmonyPrefix, HarmonyPatch("Microsoft.Xna.Framework.Game", "Update")]
        static bool Update(Game __instance, GameTime ___gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Delete))
            {
                if (!keyDown)
                {
                    var pack = packsQueue.Dequeue();
                    packsQueue.Enqueue(pack);

                    Manager.Set(pack);
                }
                keyDown = true;
            }
            else
            {
                keyDown = false;
            }

            return true;
        }

        [HarmonyPrefix, HarmonyPatch("DNA.CastleMinerZ.CastleMinerZGame", "StartGame")]
        static bool StartGame(Game __instance)
        {
            Console.WriteLine("StartGame()");

            return true;
        }

        /*
        [HarmonyPatch("DNA.CastleMinerZ.Terrain.BlockType", "GetType")]
        static bool Prefix(dynamic __instance, dynamic ___t, ref dynamic ____result)
        {
            var _blockTypes = Assembly.GetEntryAssembly().GetType("DNA.CastleMinerZ.Terrain.BlockType").GetField("_blockTypes", BindingFlags.NonPublic | BindingFlags.Static);

            Console.WriteLine($"{_blockTypes}");
            Console.WriteLine($"{__instance}{___t}{____result}");

            /*var types = (dynamic[])_blockTypes.GetValue(__instance);
            Console.WriteLine($"{types}");

            ____result = types[___t];

            return true;
        }*/
    }
}
